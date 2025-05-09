using AutoMapper;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Util.Mail.Interfaces;
using FlightApp.Util.PDF.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

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

        public BookingController(
            FlightsDbContext dbContext,
            ITicketService ticketService,
            IEmailSend emailSender,
            IWebHostEnvironment webHostEnvironment,
            ICreatePDF createPDF,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _emailSender = emailSender;
            _createPDF = createPDF;
            _webHostEnvironment = webHostEnvironment;
            _ticketService = ticketService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
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

            if (cart.RouteItems.FirstOrDefault() is var routeItem && routeItem != null)
            {
                confirmVM.RouteId = routeItem.RouteId;
                confirmVM.DepartureCity = routeItem.DepartureCity;
                confirmVM.ArrivalCity = routeItem.ArrivalCity;
                confirmVM.DepartureTime = routeItem.DepartureTime;
                confirmVM.ArrivalTime = routeItem.ArrivalTime;
                confirmVM.Layover1 = routeItem.Layover1;
                confirmVM.Layover2 = routeItem.Layover2;
                confirmVM.Passengers = routeItem.Passengers?.ToList() ?? new List<PassengerVM>();
            }

            return View(confirmVM);
        }


        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ConfirmBookingVM model)
        {
            if (!User.Identity.IsAuthenticated)
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
                int savedBookingId = 0;
                List<Ticket> createdTickets = new List<Ticket>();
                // Store route information for later use in emails
                var routeDepartureCity = "";
                var routeArrivalCity = "";
                var routeLayover1 = "";
                var routeLayover2 = "";

                // Process each route in the cart
                foreach (var routeItem in cart.RouteItems)
                {
                    // Save route details for email
                    routeDepartureCity = routeItem.DepartureCity;
                    routeArrivalCity = routeItem.ArrivalCity;
                    routeLayover1 = routeItem.Layover1;
                    routeLayover2 = routeItem.Layover2;

                    // Create a new booking
                    var booking = new Booking
                    {
                        UserId = userId,
                        BookingTime = DateTime.Now,
                        PaymentStatus = true, // Payment confirmed
                        RouteId = routeItem.RouteId
                    };

                    await _dbContext.Bookings.AddAsync(booking);
                    await _dbContext.SaveChangesAsync(); // Save to get the booking ID

                    savedBookingId = booking.BookingId; // Store the booking ID

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
                            TempData["Error"] = $"Not enough seats available on flight {flight.FlightId}. Available: {maxSeats - bookedSeatsCount}, Requested: {routeItem.Passengers.Count}";
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
                                BookingId = booking.BookingId  // THIS IS THE FIX - Set the BookingId
                            };

                            await _dbContext.Tickets.AddAsync(ticket);
                            createdTickets.Add(ticket);
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
                var passengerGroups = createdTickets
                    .GroupBy(t => t.PassengerId)
                    .ToList();

                foreach (var passengerGroup in passengerGroups)
                {
                    var passenger = await _dbContext.Passengers
                        .FirstOrDefaultAsync(p => p.PassengerId == passengerGroup.Key);

                    if (passenger != null && !string.IsNullOrEmpty(passenger.Email))
                    {
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
                            // Get route description using stored values
                            string routeDescription = $"{routeDepartureCity} to {routeArrivalCity}";

                            // Add layover information if present
                            if (!string.IsNullOrEmpty(routeLayover1))
                            {
                                routeDescription += $" via {routeLayover1}";
                                if (!string.IsNullOrEmpty(routeLayover2))
                                {
                                    routeDescription += $" and {routeLayover2}";
                                }
                            }

                            // Create simplified email message
                            var message = new StringBuilder();
                            message.AppendLine($"Dear {passenger.FirstName} {passenger.LastName},");
                            message.AppendLine();
                            message.AppendLine($"Thank you for booking with Zephyrus Airlines. Here are all your tickets for route: {routeDescription}");
                            message.AppendLine();

                            // Include flight details
                            foreach (var ticket in fullTickets.OrderBy(t => t.Flight.DepartureTime))
                            {
                                message.AppendLine($"Flight: {ticket.Flight.DepartureCityNavigation.CityName} to {ticket.Flight.ArrivalCityNavigation.CityName}");
                                message.AppendLine($"Departure: {ticket.Flight.DepartureTime?.ToString("dd/MM/yyyy HH:mm")}");
                                message.AppendLine($"Arrival: {ticket.Flight.ArrivalTime?.ToString("dd/MM/yyyy HH:mm")}");
                                message.AppendLine($"Seat: {ticket.SeatNumber}");
                                message.AppendLine($"Class: {ticket.BookingClass.Description}");

                                // Improve meal description
                                string mealDescription = ticket.MealChoice?.Type ?? "Standard Meal";
                                if (mealDescription == "Vegetarian") mealDescription = "Vegetarian Meal";
                                if (mealDescription == "Vegan") mealDescription = "Vegan Meal";
                                if (mealDescription == "Gluten-free") mealDescription = "Gluten-Free Meal";
                                if (mealDescription == "Kosher") mealDescription = "Kosher Meal";
                                if (mealDescription == "Halal") mealDescription = "Halal Meal";
                                if (mealDescription == "Diabetic") mealDescription = "Diabetic Meal";
                                if (mealDescription == "Low sodium") mealDescription = "Low Sodium Meal";
                                if (mealDescription == "Standard") mealDescription = "Standard Meal";

                                message.AppendLine($"Meal: {mealDescription}");
                                message.AppendLine();
                            }

                            message.AppendLine("Safe travels!");
                            message.AppendLine("Zephyrus Airlines Team");

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
                                message.ToString(),
                                attachments);
                        }
                    }
                }

                // Clear the cart
                var emptyCart = new ShoppingCartVM();
                SaveCartToSession(emptyCart);

                // Important: Use the saved booking ID for redirection
                return RedirectToAction("BookingConfirmed", new { id = savedBookingId });
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
        public async Task<IActionResult> BookingConfirmed(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Retrieve the booking with all related data
                var booking = await _dbContext.Bookings
                    .Include(b => b.Route)
                        .ThenInclude(r => r.DepartureCity)
                    .Include(b => b.Route)
                        .ThenInclude(r => r.ArrivalCity)
                    .Include(b => b.Passengers)
                    .FirstOrDefaultAsync(b => b.BookingId == id);

                if (booking == null)
                {
                    ViewBag.ErrorMessage = $"Booking with ID {id} not found.";
                    return View(new ConfirmBookingVM
                    {
                        BookingId = id,
                        BookingTime = DateTime.Now,
                        DepartureCity = "Not Found",
                        ArrivalCity = "Not Found",
                        Passengers = new List<PassengerVM>()
                    });
                }

                // Get all tickets related specifically to this booking
                var tickets = await _dbContext.Tickets
                    .Include(t => t.Passenger)
                    .Include(t => t.BookingClass)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.DepartureCityNavigation)
                    .Include(t => t.Flight)
                        .ThenInclude(f => f.ArrivalCityNavigation)
                    .Include(t => t.MealChoice)
                    .Where(t => t.BookingId == id) // Filter by BookingId instead of passengers
                    .ToListAsync();

                // Store flight info for the view
                var flightInfo = new Dictionary<int, object>();
                foreach (var ticket in tickets)
                {
                    if (!flightInfo.ContainsKey(ticket.FlightId) && ticket.Flight != null)
                    {
                        flightInfo[ticket.FlightId] = new
                        {
                            DepartureCity = ticket.Flight.DepartureCityNavigation?.CityName,
                            ArrivalCity = ticket.Flight.ArrivalCityNavigation?.CityName,
                            DepartureTime = ticket.Flight.DepartureTime,
                            ArrivalTime = ticket.Flight.ArrivalTime,
                            Price = ticket.Flight.Price
                        };
                    }
                }
                ViewBag.FlightInfo = flightInfo;

                var confirmVM = new ConfirmBookingVM
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    BookingTime = booking.BookingTime,
                    PaymentStatus = booking.PaymentStatus,
                    RouteId = booking.RouteId,
                    DepartureCity = booking.Route?.DepartureCity?.CityName ?? "Unknown Departure",
                    ArrivalCity = booking.Route?.ArrivalCity?.CityName ?? "Unknown Destination",
                    DepartureTime = booking.Route?.DepartureTime,
                    ArrivalTime = booking.Route?.ArrivalTime,
                    Passengers = new List<PassengerVM>(),
                    Tickets = _mapper.Map<List<TicketVM>>(tickets) // Use AutoMapper to map the tickets
                };

                // Map passengers
                var passengerIds = tickets.Select(t => t.PassengerId).Distinct().ToList();

                // Only include passengers who have tickets in this booking
                foreach (var passenger in booking.Passengers.Where(p => passengerIds.Contains(p.PassengerId)))
                {
                    var passengerVM = new PassengerVM
                    {
                        PassengerId = passenger.PassengerId,
                        FirstName = passenger.FirstName,
                        LastName = passenger.LastName,
                        Email = passenger.Email,
                        DateOfBirth = passenger.Birthdate.ToDateTime(TimeOnly.MinValue)
                    };

                    // Find passenger's tickets for this booking
                    var passengerTickets = tickets.Where(t => t.PassengerId == passenger.PassengerId).ToList();
                    if (passengerTickets.Any())
                    {
                        // Use values from the first ticket for display purposes
                        var firstTicket = passengerTickets.First();
                        passengerVM.BookingClassId = firstTicket.BookingClassId;
                        passengerVM.BookingClassName = firstTicket.BookingClass?.Description ?? "Standard";
                    }

                    confirmVM.Passengers.Add(passengerVM);
                }

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

        // Shopping cart session management methods
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
    }
}