using AutoMapper;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
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
using SQLitePCL;

namespace FlightApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly IEmailSend _emailSender;
        private readonly ICreatePDF _createPDF;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITicketService _ticketService;
        private readonly IMapper _mapper;
        private readonly IBookingHistoryService _bookingHistoryService;
        private readonly IHotelService _hotelService;
        private readonly IHolidayPriceService _holidayPriceService;
        private readonly IService<Booking> _bookingService;
        private readonly IFlightService _flightService;
        private readonly IService<Passenger> _passengerService;


        public BookingController(
            ITicketService ticketService,
            IEmailSend emailSender,
            IWebHostEnvironment webHostEnvironment,
            ICreatePDF createPDF,
            IMapper mapper,
            IBookingHistoryService bookingHistoryService,
            IHotelService hotelService,
            IHolidayPriceService holidayPriceService,
            IService<Booking> bookingService,
            IFlightService flightService,
            IService<Passenger> passengerService)
        {
            _emailSender = emailSender;
            _createPDF = createPDF;
            _webHostEnvironment = webHostEnvironment;
            _ticketService = ticketService;
            _mapper = mapper;
            _bookingHistoryService = bookingHistoryService;
            _hotelService = hotelService;
            _holidayPriceService = holidayPriceService;
            _bookingService = bookingService;
            _flightService = flightService;
            _passengerService = passengerService;
        }



        [HttpGet]
        public IActionResult ConfirmBooking()
        {
            var cart = GetCartFromSession();

            if (!cart.RouteItems.Any() && !cart.FlightItems.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items to your cart before proceeding to checkout.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var confirmVM = new ConfirmBookingVM
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                BookingTime = DateTime.Now,
                PaymentStatus = false, 
                TotalPrice = cart.ComputeTotalValue(),
                Cart = cart 
            };

            foreach (var routeItem in cart.RouteItems)
            {
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

                foreach (var flight in routeItem.Flights)
                {
                    routeViewModel.Flights.Add(new FlightViewModel
                    {
                        FlightId = flight.FlightId,
                        DepartureCity = flight.DepartureCity,
                        ArrivalCity = flight.ArrivalCity,
                        DepartureTime = flight.DepartureTime,
                        ArrivalTime = flight.ArrivalTime,
                        Price = flight.Price ?? 0,  
                        Notes = flight.Notes   
                    });
                }

                confirmVM.Routes.Add(routeViewModel);

                if (routeItem.Passengers != null)
                {
                    foreach (var passenger in routeItem.Passengers)
                    {
                        confirmVM.Passengers.Add(passenger);
                    }
                }
            }

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
            if (!cart.RouteItems.Any() && !cart.FlightItems.Any())
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

                foreach (var routeItem in cart.RouteItems)
                {
                    var booking = new Booking
                    {
                        UserId = userId ?? string.Empty,
                        BookingTime = DateTime.Now,
                        PaymentStatus = true, 
                        RouteId = routeItem.RouteId,
                        FlightId = null
                    };
                    await _bookingService.AddAsync(booking);

                    savedBookingIds.Add(booking.BookingId);

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

                    foreach (var flight in routeItem.Flights)
                    {
                        var flightDetails = await _flightService.GetFlightByIDAsync(flight.FlightId);

                        if (flightDetails == null)
                        {
                            TempData["Error"] = $"Flight {flight.FlightId} details could not be found.";
                            continue;
                        }

                        int maxSeats = flightDetails.Seating;

                        var bookedSeatsCount = await _ticketService.GetCountByFlightIDAsync(flight.FlightId);

                        if (bookedSeatsCount + routeItem.Passengers.Count > maxSeats)
                        {
                            TempData["Error"] = $"Not enough seats available on flight {flight.FlightId}.";
                            return RedirectToAction("ConfirmBooking");
                        }

                        int nextSeatNumber = bookedSeatsCount + 1;
                        foreach (var passenger in routeItem.Passengers)
                        {
                            var ticket = new Ticket
                            {
                                FlightId = flight.FlightId,
                                BookingClassId = passenger.BookingClassId,
                                PassengerId = passenger.PassengerId,
                                SeatNumber = nextSeatNumber++,
                                MealChoiceId = passenger.MealChoiceId,
                                BookingId = booking.BookingId
                            };

                            await _ticketService.AddAsync(ticket);
                            allCreatedTickets.Add(ticket);
                        }
                    }


                    foreach (var passenger in routeItem.Passengers)
                    {
                        var dbPassenger = await _passengerService.FindByIdAsync(passenger.PassengerId);

                        if (dbPassenger != null)
                        {
                            booking.Passengers.Add(dbPassenger);
                        }
                    }
                }

                foreach (var flightItem in cart.FlightItems)
                {
                    var flightDetails = await _flightService.GetFlightByIDAsync(flightItem.FlightId);

                    if (flightDetails == null)
                    {
                        TempData["Error"] = $"Flight {flightItem.FlightId} details could not be found.";
                        continue;
                    }

                    var booking = new Booking
                    {
                        UserId = userId ?? string.Empty,
                        BookingTime = DateTime.Now,
                        PaymentStatus = true,
                        FlightId = flightItem.FlightId,
                        RouteId = null
                    };

                    await _bookingService.AddAsync(booking);

                    savedBookingIds.Add(booking.BookingId);

                    string flightDescription = $"{flightItem.DepartureCity} to {flightItem.ArrivalCity}";
                    routeDescriptions[booking.BookingId] = flightDescription;

                    int maxSeats = flightDetails.Seating;

                    var bookedSeatsCount = await _ticketService.GetCountByFlightIDAsync(flightItem.FlightId);

                    if (bookedSeatsCount + flightItem.Passengers.Count > maxSeats)
                    {
                        TempData["Error"] = $"Not enough seats available on flight {flightItem.FlightId}.";
                        return RedirectToAction("ConfirmBooking");
                    }

                    int nextSeatNumber = bookedSeatsCount + 1;
                    foreach (var passenger in flightItem.Passengers)
                    {
                        var ticket = new Ticket
                        {
                            FlightId = flightItem.FlightId,
                            BookingClassId = passenger.BookingClassId,
                            PassengerId = passenger.PassengerId,
                            SeatNumber = nextSeatNumber++,
                            MealChoiceId = passenger.MealChoiceId,
                            BookingId = booking.BookingId
                        };

                        await _ticketService.AddAsync(ticket);
                        allCreatedTickets.Add(ticket);
                    }

                    foreach (var passenger in flightItem.Passengers)
                    {
                        var dbPassenger = await _passengerService.FindByIdAsync(passenger.PassengerId);

                        if (dbPassenger != null)
                        {
                            booking.Passengers.Add(dbPassenger);
                        }
                    }
                }

                var passengerGroups = allCreatedTickets
                    .GroupBy(t => t.PassengerId)
                    .ToList();

                foreach (var passengerGroup in passengerGroups)
                {
                    var passenger = await _passengerService.FindByIdAsync(passengerGroup.Key);

                    if (passenger != null && !string.IsNullOrEmpty(passenger.Email))
                    {
                        var bookingTickets = passengerGroup
                            .GroupBy(t => t.BookingId)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        var fullTickets = new List<Ticket>();
                        foreach (var ticket in passengerGroup)
                        {
                            var fullTicket = await _ticketService.FindByIdAsync(ticket.TicketId);

                            if (fullTicket != null)
                            {
                                fullTickets.Add(fullTicket);
                            }
                        }

                        if (fullTickets.Count > 0)
                        {
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

                            string emailHtml = EmailTemplateService.GenerateBookingConfirmationEmail(
                                passenger,
                                routeDescription,
                                fullTickets
                            );

                            var attachments = new List<(string fileName, byte[] content, string contentType)>();

                            for (int i = 0; i < fullTickets.Count; i++)
                            {
                                var ticket = fullTickets[i];
                                var pdfStream = _createPDF.CreatePDFDocumentAsync(ticket);

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

                            await _emailSender.SendEmailWithAttachmentsAsync(
                                passenger.Email,
                                $"Your Zephyrus Airlines Tickets - {routeDescription}",
                                emailHtml,
                                attachments);
                        }
                    }
                }

                foreach (var bookingId in savedBookingIds)
                {
                    var bookingHistory = new BookingHistory
                    {
                        UserId = userId ?? string.Empty,
                        BookingId = bookingId
                    };
                    await _bookingHistoryService.AddAsync(bookingHistory);
                }

                var emptyCart = new ShoppingCartVM();
                SaveCartToSession(emptyCart);

                return RedirectToAction("BookingConfirmed", new
                {
                    id = savedBookingIds.First(),
                    relatedIds = string.Join(",", savedBookingIds.Skip(1))
                });
            }
            catch (DbUpdateException ex)
            {
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

                List<HotelVM> hotels = new List<HotelVM>();

                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    List<int> allBookingIds = new List<int> { id };

                    if (!string.IsNullOrEmpty(relatedIds))
                    {
                        var relatedBookingIds = relatedIds.Split(',')
                            .Where(s => int.TryParse(s, out _))
                            .Select(int.Parse);

                        allBookingIds.AddRange(relatedBookingIds);
                    }

                    var confirmVM = new ConfirmBookingVM
                    {
                        BookingId = id,
                        UserId = userId ?? string.Empty,
                        BookingTime = DateTime.Now,
                        Passengers = new List<PassengerVM>(),
                        Tickets = new List<TicketVM>(),
                        Routes = new List<RouteViewModel>(),
                        DirectFlights = new List<DirectFlightViewModel>()
                    };

                    foreach (var bookingId in allBookingIds)
                    {
                    var booking = await _bookingService.FindByIdAsync(bookingId);

                        if (booking == null) continue;

                        string? cityApiId = null;
                        if (booking.Route?.ArrivalCity?.ApiId != null)
                        {
                            cityApiId = booking.Route.ArrivalCity.ApiId.ToString();
                        }
                        else if (booking.Flight?.ArrivalCityNavigation?.ApiId != null)
                        {
                            cityApiId = booking.Flight.ArrivalCityNavigation.ApiId.ToString();
                        }
                    //if (firstBookingWithCity != null)
                    //{
                    //   hotels = await GetHotelVMList(cityApiId);
                    //}

                    var tickets = await _ticketService.GetTicketsByBookingIdAsync(bookingId);
                    
                        Dictionary<int, string> holidayNotes = new Dictionary<int, string>();

                        double bookingTotal = 0;
                        foreach (var ticket in tickets)
                        {
                            double flightPrice = ticket.Flight.Price;
                            double bookingClassFactor = ticket.BookingClass?.PriceFactor ?? 1.0;

                            if (ticket.Flight.DepartureTime.HasValue)
                            {
                                double holidayFactor = await _holidayPriceService.GetHolidayPriceFactor(
                                    ticket.Flight.DepartureCity,
                                    ticket.Flight.DepartureTime.Value);

                                if (Math.Abs(holidayFactor - 1.0) > 0.01)
                                {
                                    flightPrice *= holidayFactor;

                                    if (!holidayNotes.ContainsKey(ticket.FlightId))
                                    {
                                        holidayNotes[ticket.FlightId] = $"Holiday pricing applied (x{holidayFactor:F2})";
                                    }
                                }
                            }

                            bookingTotal += flightPrice * bookingClassFactor;
                        }

                        confirmVM.TotalPrice += bookingTotal;

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
                                MealChoiceType = ticket.MealChoice?.Type,
                                BookingId = bookingId,
                                Notes = holidayNotes.TryGetValue(ticket.FlightId, out string? note) ? note : null
                            };

                            confirmVM.Tickets.Add(ticketVM);
                        }

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

                            var passengerTickets = tickets.Where(t => t.PassengerId == passenger.PassengerId).ToList();
                            if (passengerTickets.Any())
                            {
                                var firstTicket = passengerTickets.First();
                                passengerVM.BookingClassId = firstTicket.BookingClassId;
                                passengerVM.BookingClassName = firstTicket.BookingClass?.Description;

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
                                    BookingClassName = t.BookingClass?.Description ?? "Standard",
                                    Notes = holidayNotes.TryGetValue(t.FlightId, out string? note) ? note : null
                                }).ToList();
                            }

                            if (!confirmVM.Passengers.Any(p => p.PassengerId == passenger.PassengerId))
                            {
                                confirmVM.Passengers.Add(passengerVM);
                            }
                        }

                        if (booking.RouteId.HasValue && booking.Route != null)
                        {
                            var layovers = await GetLayoversForBooking(booking);

                            var routeViewModel = new RouteViewModel
                            {
                                RouteId = booking.RouteId.Value,
                                DepartureCity = booking.Route?.DepartureCity?.CityName,
                                ArrivalCity = booking.Route?.ArrivalCity?.CityName,
                                DepartureTime = booking.Route?.DepartureTime,
                                ArrivalTime = booking.Route?.ArrivalTime,
                                Layover1 = layovers.layover1,
                                Layover2 = layovers.layover2,
                                Price = bookingTotal,
                                BookingId = bookingId
                            };

                            foreach (var ticket in tickets.GroupBy(t => t.FlightId).Select(g => g.First()))
                            {
                                routeViewModel.Flights.Add(new FlightViewModel
                                {
                                    FlightId = ticket.FlightId,
                                    DepartureCity = ticket.Flight?.DepartureCityNavigation?.CityName,
                                    ArrivalCity = ticket.Flight?.ArrivalCityNavigation?.CityName,
                                    DepartureTime = ticket.Flight?.DepartureTime,
                                    ArrivalTime = ticket.Flight?.ArrivalTime,
                                    Price = ticket.Flight.Price,
                                    Notes = holidayNotes.TryGetValue(ticket.FlightId, out string? note) ? note : null
                                });
                            }

                            var routePassengers = booking.Passengers.Select(p =>
                            {
                                var pVM = confirmVM.Passengers.FirstOrDefault(vm => vm.PassengerId == p.PassengerId);
                                return pVM ?? new PassengerVM
                                {
                                    PassengerId = p.PassengerId,
                                    FirstName = p.FirstName,
                                    LastName = p.LastName,
                                    Email = p.Email,
                                    DateOfBirth = p.Birthdate.ToDateTime(TimeOnly.MinValue)
                                };
                            }).ToList();

                            routeViewModel.Passengers = routePassengers;
                            routeViewModel.Tickets = confirmVM.Tickets.Where(t => t.BookingId == bookingId).ToList();

                            confirmVM.Routes.Add(routeViewModel);
                        }
                        else if (booking.FlightId.HasValue && booking.Flight != null)
                        {
                            string? directFlightNote = null;
                            if (booking.Flight.DepartureTime.HasValue)
                            {
                                double holidayFactor = await _holidayPriceService.GetHolidayPriceFactor(
                                    booking.Flight.DepartureCity,
                                    booking.Flight.DepartureTime.Value);

                                if (Math.Abs(holidayFactor - 1.0) > 0.01)
                                {
                                    directFlightNote = $"Holiday pricing applied (x{holidayFactor:F2})";
                                }
                            }

                            var directFlightViewModel = new DirectFlightViewModel
                            {
                                FlightId = booking.FlightId.Value,
                                DepartureCity = booking.Flight?.DepartureCityNavigation?.CityName,
                                ArrivalCity = booking.Flight?.ArrivalCityNavigation?.CityName,
                                DepartureTime = booking.Flight?.DepartureTime,
                                ArrivalTime = booking.Flight?.ArrivalTime,
                                Price = bookingTotal,
                                BookingId = bookingId,
                                Notes = directFlightNote
                            };

                            var flightPassengers = booking.Passengers.Select(p =>
                            {
                                var pVM = confirmVM.Passengers.FirstOrDefault(vm => vm.PassengerId == p.PassengerId);
                                return pVM ?? new PassengerVM
                                {
                                    PassengerId = p.PassengerId,
                                    FirstName = p.FirstName,
                                    LastName = p.LastName,
                                    Email = p.Email,
                                    DateOfBirth = p.Birthdate.ToDateTime(TimeOnly.MinValue)
                                };
                            }).ToList();

                            directFlightViewModel.Passengers = flightPassengers;
                            directFlightViewModel.Tickets = confirmVM.Tickets.Where(t => t.BookingId == bookingId).ToList();

                            confirmVM.DirectFlights.Add(directFlightViewModel);
                        }
                    }

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
                    else if (confirmVM.DirectFlights.FirstOrDefault() is var firstFlight && firstFlight != null)
                    {
                        confirmVM.RouteId = 0;
                        confirmVM.DepartureCity = firstFlight.DepartureCity;
                        confirmVM.ArrivalCity = firstFlight.ArrivalCity;
                        confirmVM.DepartureTime = firstFlight.DepartureTime;
                        confirmVM.ArrivalTime = firstFlight.ArrivalTime;
                    }

                    ConfirmBookingHotelListVM confirmBookingHotelListVM = new ConfirmBookingHotelListVM
                    {
                        ConfirmBooking = confirmVM,
                        Hotels = hotels
                    };

                    return View(confirmBookingHotelListVM);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                    return View(new ConfirmBookingHotelListVM
                    {
                        ConfirmBooking = new ConfirmBookingVM
                        {
                            BookingId = id,
                            BookingTime = DateTime.Now,
                            DepartureCity = "Error Loading City",
                            ArrivalCity = "Error Loading City",
                            Passengers = new List<PassengerVM>()
                        },
                        Hotels = new List<HotelVM>()
                    });
                }
            }

    [HttpGet]
        public async Task<IActionResult> DownloadTicket(int ticketId)
        {
            try
            {
                var ticket = await _ticketService.FindByIdAsync(ticketId);

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
            var bookingTickets = await _ticketService.GetTicketsByBookingIdAsync(booking.BookingId);

            var flights = bookingTickets
                .Select(t => t.Flight)
                .Distinct()
                .OrderBy(f => f.DepartureTime)
                .ToList();

            string? layover1 = null;
            string? layover2 = null;

            if (flights.Count >= 2)
            {
                layover1 = flights[0].ArrivalCityNavigation?.CityName;
            }

            if (flights.Count >= 3)
            {
                layover2 = flights[1].ArrivalCityNavigation?.CityName;
            }

            return (layover1, layover2);
        }


        public async Task<List<HotelVM>> GetHotelVMList(string? cityApiID)
        {
            List<HotelVM> hotels = new List<HotelVM>();

            if (string.IsNullOrEmpty(cityApiID))
            {
                return hotels;
            }

            try
            {
                var lstHotelIds = await _hotelService.GetHotelIdsAsync(cityApiID);
                lstHotelIds = lstHotelIds.Slice(0, 3);

                foreach (var hotelId in lstHotelIds)
                {
                    var hotel = await _hotelService.GetHotelByIdAsync(hotelId.hotel_id);
                    var hotelvm = _mapper.Map<HotelVM>(hotel);
                    hotels.Add(hotelvm);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading hotels: {ex.Message}");
            }

            return hotels;
        }
    }
}