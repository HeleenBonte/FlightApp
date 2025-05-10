using AutoMapper;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Models;
using FlightApp.Services.Interfaces;
using FlightApp.Util.Hotels.Interfaces;
using FlightApp.Util.Mail.Interfaces;
using FlightApp.Util.PDF.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FlightApp.Util.Email;
using FlightApp.Util.Hotels;

namespace FlightApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly FlightsDbContext _dbContext;
        private readonly IEmailSend _emailSender;
        private readonly ICreatePDF _createPDF;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITicketService _ticketService;
        private readonly IMapper _mapper;
        private readonly IBookingHistoryService _bookingHistoryService;
        private readonly IHotelService _hotelService;

        public BookingController(
            FlightsDbContext dbContext,
            ITicketService ticketService,
            IEmailSend emailSender,
            IWebHostEnvironment webHostEnvironment,
            ICreatePDF createPDF,
            IMapper mapper,
            IBookingHistoryService bookingHistoryService,
            IHotelService hotelService)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
            _createPDF = createPDF;
            _webHostEnvironment = webHostEnvironment;
            _ticketService = ticketService;
            _mapper = mapper;
            _bookingHistoryService = bookingHistoryService;
            _hotelService = hotelService;
        }



        [HttpGet]
        public IActionResult ConfirmBooking()
        {
            var cart = GetCartFromSession();

            if (!cart.RouteItems.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items to your cart before proceeding to checkout.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var confirmVM = new ConfirmBookingVM
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                BookingTime = DateTime.Now,
                PaymentStatus = false, // Not paid yet
                TotalPrice = cart.ComputeTotalValue()
            };

            // Process all routes in the cart
            foreach (var routeItem in cart.RouteItems)
            {
                // Create a route view model for each route in the cart
                var routeViewModel = new RouteViewModel
                {
                    RouteId = routeItem.RouteId,
                    DepartureCity = routeItem.DepartureCity,
                    ArrivalCity = routeItem.ArrivalCity,
                    DepartureTime = routeItem.DepartureTime,
                    ArrivalTime = routeItem.ArrivalTime,
                    Layover1 = routeItem.Layover1,
                    Layover2 = routeItem.Layover2,
                    Price = routeItem.TotalPrice,
                    Passengers = routeItem.Passengers?.ToList() ?? new List<PassengerVM>()
                };

                // Add flights to the route
                foreach (var flight in routeItem.Flights)
                {
                    routeViewModel.Flights.Add(new FlightViewModel
                    {
                        FlightId = flight.FlightId,
                        DepartureCity = flight.DepartureCity,
                        ArrivalCity = flight.ArrivalCity,
                        DepartureTime = flight.DepartureTime,
                        ArrivalTime = flight.ArrivalTime,
                        Price = flight.Price ?? 0
                    });
                }

                // Add the route to the collection
                confirmVM.Routes.Add(routeViewModel);

                // Add all passengers to the main list too
                if (routeItem.Passengers != null)
                {
                    foreach (var passenger in routeItem.Passengers)
                    {
                        confirmVM.Passengers.Add(passenger);
                    }
                }
            }

            // For backward compatibility, set the first route info as the main properties
            if (confirmVM.Routes.FirstOrDefault() is var firstRoute && firstRoute != null)
            {
                confirmVM.RouteId = firstRoute.RouteId;
                confirmVM.DepartureCity = firstRoute.DepartureCity;
                confirmVM.ArrivalCity = firstRoute.ArrivalCity;
                confirmVM.DepartureTime = firstRoute.DepartureTime;
                confirmVM.ArrivalTime = firstRoute.ArrivalTime;
                confirmVM.Layover1 = firstRoute.Layover1;
                confirmVM.Layover2 = firstRoute.Layover2;
            }

            return View(confirmVM);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ConfirmBookingVM model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("ConfirmBooking") });
            }

            var cart = GetCartFromSession();
            if (!cart.RouteItems.Any())
            {
                TempData["Error"] = "Your cart is empty. Cannot process payment.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                List<int> savedBookingIds = new List<int>();
                List<Ticket> allCreatedTickets = new List<Ticket>();
                Dictionary<int, string> routeDescriptions = new Dictionary<int, string>();

                // Process each route in the cart
                foreach (var routeItem in cart.RouteItems)
                {
                    // Create a new booking
                    var booking = new Booking
                    {
                        UserId = userId ?? string.Empty, // Fix nullable warning
                        BookingTime = DateTime.Now,
                        PaymentStatus = true, // Payment confirmed
                        RouteId = routeItem.RouteId
                    };

                    await _dbContext.Bookings.AddAsync(booking);
                    await _dbContext.SaveChangesAsync(); // Save to get the booking ID

                    savedBookingIds.Add(booking.BookingId);

                    // Save route description for email
                    string routeDescription = $"{routeItem.DepartureCity} to {routeItem.ArrivalCity}";
                    if (!string.IsNullOrEmpty(routeItem.Layover1))
                    {
                        routeDescription += $" via {routeItem.Layover1}";
                        if (!string.IsNullOrEmpty(routeItem.Layover2))
                        {
                            routeDescription += $" and {routeItem.Layover2}";
                        }
                    }
                    routeDescriptions[booking.BookingId] = routeDescription;


                    // Process each passenger for each flight in the route
                    foreach (var flight in routeItem.Flights)
                    {
                        // Get flight capacity
                        var flightDetails = await _dbContext.Flights
                            .Include(f => f.DepartureCityNavigation)
                            .Include(f => f.ArrivalCityNavigation)
                            .FirstOrDefaultAsync(f => f.FlightId == flight.FlightId);

                        if (flightDetails == null)
                        {
                            TempData["Error"] = $"Flight {flight.FlightId} details could not be found.";
                            continue;
                        }

                        int maxSeats = flightDetails.Seating;

                        // Get count of existing tickets to determine seat numbers
                        var bookedSeatsCount = await _dbContext.Tickets
                            .Where(t => t.FlightId == flight.FlightId)
                            .CountAsync();

                        // Check if there are enough seats available
                        if (bookedSeatsCount + routeItem.Passengers.Count > maxSeats)
                        {
                            TempData["Error"] = $"Not enough seats available on flight {flight.FlightId}.";
                            return RedirectToAction("ConfirmBooking");
                        }

                        // Create tickets for each passenger
                        int nextSeatNumber = bookedSeatsCount + 1;
                        foreach (var passenger in routeItem.Passengers)
                        {
                            // Create ticket with BookingId set
                            var ticket = new Ticket
                            {
                                FlightId = flight.FlightId,
                                BookingClassId = passenger.BookingClassId,
                                PassengerId = passenger.PassengerId,
                                SeatNumber = nextSeatNumber++,
                                MealChoiceId = passenger.MealChoiceId,
                                BookingId = booking.BookingId
                            };

                            await _dbContext.Tickets.AddAsync(ticket);
                            allCreatedTickets.Add(ticket);
                        }
                    }

                    // Add passengers to booking relationship
                    foreach (var passenger in routeItem.Passengers)
                    {
                        var dbPassenger = await _dbContext.Passengers
                            .FirstOrDefaultAsync(p => p.PassengerId == passenger.PassengerId);

                        if (dbPassenger != null)
                        {
                            booking.Passengers.Add(dbPassenger);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();

                // Send confirmation emails to each passenger with their tickets
                var passengerGroups = allCreatedTickets
                    .GroupBy(t => t.PassengerId)
                    .ToList();

                foreach (var passengerGroup in passengerGroups)
                {
                    var passenger = await _dbContext.Passengers
                        .FirstOrDefaultAsync(p => p.PassengerId == passengerGroup.Key);

                    if (passenger != null && !string.IsNullOrEmpty(passenger.Email))
                    {
                        // Group tickets by booking
                        var bookingTickets = passengerGroup
                            .GroupBy(t => t.BookingId)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        // Get all full ticket data for this passenger
                        var fullTickets = new List<Ticket>();
                        foreach (var ticket in passengerGroup)
                        {
                            var fullTicket = await _dbContext.Tickets
                                .Include(t => t.Passenger)
                                .Include(t => t.Flight)
                                    .ThenInclude(f => f.DepartureCityNavigation)
                                .Include(t => t.Flight)
                                    .ThenInclude(f => f.ArrivalCityNavigation)
                                .Include(t => t.BookingClass)
                                .Include(t => t.MealChoice)
                                .FirstOrDefaultAsync(t => t.TicketId == ticket.TicketId);

                            if (fullTicket != null)
                            {
                                fullTickets.Add(fullTicket);
                            }
                        }

                        if (fullTickets.Count > 0)
                        {
                            // Create combined route description for all bookings this passenger has
                            var passengerBookingIds = fullTickets.Select(t => t.BookingId).Distinct();
                            var combinedRoutes = new List<string>();

                            foreach (var bookingId in passengerBookingIds)
                            {
                                if (routeDescriptions.TryGetValue(bookingId, out string? route) && route != null)
                                {
                                    combinedRoutes.Add(route);
                                }
                            }

                            string routeDescription = string.Join(" and ", combinedRoutes);

                            // Generate email HTML using the EmailTemplateService
                            string emailHtml = EmailTemplateService.GenerateBookingConfirmationEmail(
                                passenger,
                                routeDescription,
                                fullTickets
                            );

                            // Prepare list of PDF attachments
                            var attachments = new List<(string fileName, byte[] content, string contentType)>();

                            // Generate all PDFs and add to attachments list
                            for (int i = 0; i < fullTickets.Count; i++)
                            {
                                var ticket = fullTickets[i];
                                var pdfStream = _createPDF.CreatePDFDocumentAsync(ticket);

                                // Convert the stream to byte array
                                byte[] pdfBytes;
                                pdfStream.Position = 0;
                                using (var memoryStream = new MemoryStream())
                                {
                                    await pdfStream.CopyToAsync(memoryStream);
                                    pdfBytes = memoryStream.ToArray();
                                }

                                var fileName = $"Ticket-{ticket.Flight.DepartureCityNavigation.CityName}-to-{ticket.Flight.ArrivalCityNavigation.CityName}-Seat{ticket.SeatNumber}.pdf";
                                attachments.Add((fileName, pdfBytes, "application/pdf"));
                            }

                            // Send a single email with all ticket PDFs as attachments
                            await _emailSender.SendEmailWithAttachmentsAsync(
                                passenger.Email,
                                $"Your Zephyrus Airlines Tickets - {routeDescription}",
                                emailHtml,
                                attachments);
                        }
                    }
                }

                // Add each booking to booking history
                foreach (var bookingId in savedBookingIds)
                {
                    var bookingHistory = new BookingHistory
                    {
                        UserId = userId ?? string.Empty,
                        BookingId = bookingId
                    };
                    await _bookingHistoryService.AddAsync(bookingHistory);
                }


                // Clear the cart
                var emptyCart = new ShoppingCartVM();
                SaveCartToSession(emptyCart);

                // Redirect to the first booking's confirmation page
                // Include all related bookings to show them on the confirmation page
                return RedirectToAction("BookingConfirmed", new
                {
                    id = savedBookingIds.First(),
                    relatedIds = string.Join(",", savedBookingIds.Skip(1))
                });
            }
            catch (DbUpdateException ex)
            {
                // Get detailed error information including inner exceptions
                string errorMsg = ex.Message;
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMsg += " -> " + innerEx.Message;
                    innerEx = innerEx.InnerException;
                }

                TempData["Error"] = $"An error occurred while processing your payment: {errorMsg}";
                return RedirectToAction("ConfirmBooking");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while processing your payment: {ex.Message}";
                return RedirectToAction("ConfirmBooking");
            }
        }


        [HttpGet]
        public async Task<IActionResult> BookingConfirmed(int id, string? relatedIds = null)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                List<int> allBookingIds = new List<int> { id };

                // Parse related booking IDs if provided
                if (!string.IsNullOrEmpty(relatedIds))
                {
                    var relatedBookingIds = relatedIds.Split(',')
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse);

                    allBookingIds.AddRange(relatedBookingIds);
                }

                // Create the view model
                var confirmVM = new ConfirmBookingVM
                {
                    BookingId = id,
                    UserId = userId ?? string.Empty,
                    BookingTime = DateTime.Now,
                    Passengers = new List<PassengerVM>(),
                    Tickets = new List<TicketVM>()
                };

                // Process all bookings (primary and related)
                foreach (var bookingId in allBookingIds)
                {
                    // Retrieve the booking with all related data
                    var booking = await _dbContext.Bookings
                        .Include(b => b.Route)
                            .ThenInclude(r => r != null ? r.DepartureCity : null)
                        .Include(b => b.Route)
                            .ThenInclude(r => r != null ? r.ArrivalCity : null)
                        .Include(b => b.Passengers)
                        .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                    if (booking == null) continue;

                    // Get the layover information from tickets
                    var layovers = await GetLayoversForBooking(booking);

                    // Create a route view model
                    var routeViewModel = new RouteViewModel
                    {
                        RouteId = booking.RouteId,
                        DepartureCity = booking.Route?.DepartureCity?.CityName,
                        ArrivalCity = booking.Route?.ArrivalCity?.CityName,
                        DepartureTime = booking.Route?.DepartureTime,
                        ArrivalTime = booking.Route?.ArrivalTime,
                        // Use layovers identified from booking tickets
                        Layover1 = layovers.layover1,
                        Layover2 = layovers.layover2,
                        Price = 0 // Will be updated with ticket prices
                    };

                    // Get all tickets for this booking
                    var tickets = await _dbContext.Tickets
                        .Include(t => t.Passenger)
                        .Include(t => t.BookingClass)
                        .Include(t => t.Flight)
                            .ThenInclude(f => f.DepartureCityNavigation)
                        .Include(t => t.Flight)
                            .ThenInclude(f => f.ArrivalCityNavigation)
                        .Include(t => t.MealChoice)
                        .Where(t => t.BookingId == bookingId)
                        .ToListAsync();

                    // Calculate route price
                    var ticketsTotal = tickets.Sum(t => t.Flight.Price * (t.BookingClass?.PriceFactor ?? 1.0));
                    routeViewModel.Price = ticketsTotal;
                    confirmVM.TotalPrice += ticketsTotal;

                    // Add tickets to the view model with complete flight details
                    foreach (var ticket in tickets)
                    {
                        var ticketVM = new TicketVM
                        {
                            Id = ticket.TicketId,
                            FlightId = ticket.FlightId,
                            BookingClassId = ticket.BookingClassId,
                            PassengerId = ticket.PassengerId,
                            SeatNumber = ticket.SeatNumber,
                            MealChoiceId = ticket.MealChoiceId,
                            PassengerName = $"{ticket.Passenger?.FirstName} {ticket.Passenger?.LastName}",
                            FlightDeparture = ticket.Flight?.DepartureCityNavigation?.CityName,
                            FlightArrival = ticket.Flight?.ArrivalCityNavigation?.CityName,
                            DepartureTime = ticket.Flight?.DepartureTime,
                            ArrivalTime = ticket.Flight?.ArrivalTime,
                            BookingClassName = ticket.BookingClass?.Description,
                            MealChoiceType = ticket.MealChoice?.Type
                        };

                        confirmVM.Tickets.Add(ticketVM);
                        routeViewModel.Tickets.Add(ticketVM);
                    }

                    // Add passengers for this route
                    foreach (var passenger in booking.Passengers)
                    {
                        var passengerVM = new PassengerVM
                        {
                            PassengerId = passenger.PassengerId,
                            FirstName = passenger.FirstName,
                            LastName = passenger.LastName,
                            Email = passenger.Email,
                            DateOfBirth = passenger.Birthdate.ToDateTime(TimeOnly.MinValue)
                        };

                        // Get passenger's booking class from their tickets
                        var passengerTickets = tickets.Where(t => t.PassengerId == passenger.PassengerId).ToList();
                        if (passengerTickets.Any())
                        {
                            var firstTicket = passengerTickets.First();
                            passengerVM.BookingClassId = firstTicket.BookingClassId;
                            passengerVM.BookingClassName = firstTicket.BookingClass?.Description;

                            // Add ticket details to passenger for easy reference
                            passengerVM.TicketDetails = passengerTickets.Select(t => new TicketDetailVM
                            {
                                TicketId = t.TicketId,
                                FlightId = t.FlightId,
                                FlightNumber = t.Flight.FlightId.ToString(),
                                DepartureCity = t.Flight.DepartureCityNavigation?.CityName ?? "N/A",
                                ArrivalCity = t.Flight.ArrivalCityNavigation?.CityName ?? "N/A",
                                DepartureTime = t.Flight.DepartureTime,
                                ArrivalTime = t.Flight.ArrivalTime,
                                SeatNumber = t.SeatNumber,
                                BookingClassName = t.BookingClass?.Description ?? "Standard"
                            }).ToList();
                        }

                        routeViewModel.Passengers.Add(passengerVM);

                        // Also add to main passengers list if not already there
                        if (!confirmVM.Passengers.Any(p => p.PassengerId == passenger.PassengerId))
                        {
                            confirmVM.Passengers.Add(passengerVM);
                        }
                    }

                    confirmVM.Routes.Add(routeViewModel);
                }

                // Set basic info from the primary booking for backward compatibility
                if (confirmVM.Routes.FirstOrDefault() is var firstRoute && firstRoute != null)
                {
                    confirmVM.RouteId = firstRoute.RouteId;
                    confirmVM.DepartureCity = firstRoute.DepartureCity;
                    confirmVM.ArrivalCity = firstRoute.ArrivalCity;
                    confirmVM.DepartureTime = firstRoute.DepartureTime;
                    confirmVM.ArrivalTime = firstRoute.ArrivalTime;
                    confirmVM.Layover1 = firstRoute.Layover1;
                    confirmVM.Layover2 = firstRoute.Layover2;
                }
                ConfirmBookingHotelListVM confirmBookingHotelListVM = new ConfirmBookingHotelListVM
                {
                    ConfirmBooking = confirmVM,
                    Hotels = await GetHotelVMList(booking.Route?.ArrivalCity?.ApiId.ToString())
                };



                return View(confirmVM);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(new ConfirmBookingVM
                {
                    BookingId = id,
                    BookingTime = DateTime.Now,
                    DepartureCity = "Error Loading City",
                    ArrivalCity = "Error Loading City",
                    Passengers = new List<PassengerVM>()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadTicket(int ticketId)
        {
            try
            {
                var ticket = await _dbContext.Tickets
                    .Include(t => t.Passenger)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.DepartureCityNavigation)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.ArrivalCityNavigation)
                    .Include(t => t.BookingClass)
                    .Include(t => t.MealChoice)
                    .FirstOrDefaultAsync(t => t.TicketId == ticketId);

                if (ticket == null)
                {
                    return NotFound();
                }

                var pdfDoc = _createPDF.CreatePDFDocumentAsync(ticket);
                return File(pdfDoc.ToArray(), "application/pdf", $"ticket-{ticketId}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating ticket: {ex.Message}");
            }
        }

        private ShoppingCartVM GetCartFromSession()
        {
            try
            {
                var cartJson = HttpContext.Session.GetString("Cart");
                if (string.IsNullOrEmpty(cartJson))
                {
                    return new ShoppingCartVM();
                }
                return JsonSerializer.Deserialize<ShoppingCartVM>(cartJson) ?? new ShoppingCartVM();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to retrieve cart from session: {ex.Message}";
                return new ShoppingCartVM();
            }
        }

        private void SaveCartToSession(ShoppingCartVM cart)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
                var cartJson = JsonSerializer.Serialize(cart, options);
                HttpContext.Session.SetString("Cart", cartJson);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to save cart to session: {ex.Message}";
            }
        }

        private async Task<(string? layover1, string? layover2)> GetLayoversForBooking(Booking booking)
        {
            // Get all tickets for this booking
            var bookingTickets = await _dbContext.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DepartureCityNavigation)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.ArrivalCityNavigation)
                .Where(t => t.BookingId == booking.BookingId)
                .ToListAsync();

            // Group by flight to get unique flights
            var flights = bookingTickets
                .Select(t => t.Flight)
                .Distinct()
                .OrderBy(f => f.DepartureTime)
                .ToList();

            // Extract layover information based on flight order
            string? layover1 = null;
            string? layover2 = null;

            if (flights.Count >= 2)
            {
                // For routes with at least 1 layover
                layover1 = flights[0].ArrivalCityNavigation?.CityName;
            }

            if (flights.Count >= 3)
            {
                // For routes with at least 2 layovers
                layover2 = flights[1].ArrivalCityNavigation?.CityName;
            }

            return (layover1, layover2);
        }


        public async Task<List<HotelVM>> GetHotelVMList(string cityApiID)
        {


            var lstHotelIds = await _hotelService.GetHotelIdsAsync(cityApiID);
            //var lstHotelIdsVm = _mapper.Map<List<HotelIDVm>>(lstHotelIds);
            lstHotelIds = lstHotelIds.Slice(0, 3);
            List<HotelVM> hotels = new List<HotelVM>();
            foreach (var hotelId in lstHotelIds)
            {
                var hotel = await _hotelService.GetHotelByIdAsync(hotelId.hotel_id);
                var hotelvm = _mapper.Map<HotelVM>(hotel);

                hotels.Add(hotelvm);
            }
            return hotels;
        }
    }
}