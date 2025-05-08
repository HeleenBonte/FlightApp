using AutoMapper;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Models;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FlightApp.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly FlightsDbContext _dbContext;
        private readonly IMapper _mapper;

        public ShoppingCartController(FlightsDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> AddRouteToCart(int id)
        {
            try
            {
                // Ensure authentication state is preserved
                if (!User.Identity.IsAuthenticated)
                {
                    // If accessing directly and not authenticated, redirect to login
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var route = await _dbContext.Routes
                    .Include(r => r.DepartureCity)
                    .Include(r => r.ArrivalCity)
                    .Include(r => r.Flights)
                    .FirstOrDefaultAsync(r => r.RouteId == id);

                if (route == null)
                {
                    return NotFound();
                }

                var cart = GetCartFromSession();

                // Check if route is already in cart
                var existingRouteItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == id);
                if (existingRouteItem != null)
                {
                    TempData["Message"] = "This route is already in your basket.";
                    return RedirectToAction("Index");
                }

                // Create new route cart item
                var routeCartItem = new RouteCartItemVM
                {
                    RouteId = route.RouteId,
                    DepartureTime = route.DepartureTime ?? DateTime.Now,
                    DepartureCity = route.DepartureCity?.CityName,
                    ArrivalCity = route.ArrivalCity?.CityName,
                    ArrivalTime = route.ArrivalTime,
                    Flights = new List<FlightVM>(),
                    TotalPrice = 0,
                    Passengers = new List<PassengerVM>()
                };

                // Fetch flight details and calculate total price
                foreach (var flight in route.Flights)
                {
                    var flightDetail = await _dbContext.Flights
                        .Include(f => f.ArrivalCityNavigation)
                        .Include(f => f.DepartureCityNavigation)
                        .FirstOrDefaultAsync(f => f.FlightId == flight.FlightId);

                    if (flightDetail != null)
                    {
                        var flightVM = _mapper.Map<FlightVM>(flightDetail);
                        routeCartItem.Flights.Add(flightVM);
                        routeCartItem.TotalPrice += flightDetail.Price;

                        var passengers = await _dbContext.Passengers
                            .Where(p => p.Tickets.Any(t => t.FlightId == flight.FlightId))
                            .ToListAsync();

                        foreach (var passenger in passengers)
                        {
                            routeCartItem.Passengers.Add(new PassengerVM
                            {
                                FirstName = passenger.FirstName,
                                LastName = passenger.LastName,
                                Email = passenger.Email,
                                DateOfBirth = passenger.Birthdate.ToDateTime(TimeOnly.MinValue),
                                FlightId = flight.FlightId
                            });
                        }
                    }
                }

                if (routeCartItem.Flights.Count == 2)
                {
                    var flights = routeCartItem.Flights.ToList();
                    routeCartItem.Layover1 = flights[1].ArrivalCity;
                }
                else if (routeCartItem.Flights.Count == 3)
                {
                    var flights = routeCartItem.Flights.ToList();
                    routeCartItem.Layover1 = flights[2].ArrivalCity;
                    routeCartItem.Layover2 = flights[1].ArrivalCity;
                }

                cart.RouteItems.Add(routeCartItem);
                SaveCartToSession(cart);

                TempData["Message"] = "Route added to your basket successfully.";
                return RedirectToAction("SelectPassengers", new { routeId = id });
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SelectPassengers(int routeId, int? passengerCount = null)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem == null)
                {
                    TempData["Error"] = "Route not found in your basket.";
                    return RedirectToAction("Index");
                }

                // Update passenger count if provided
                if (passengerCount.HasValue)
                {
                    routeItem.PassengerCount = passengerCount.Value;
                    SaveCartToSession(cart);
                }

                // Get the route details to determine departure and arrival cities
                var route = await _dbContext.Routes
                    .FirstOrDefaultAsync(r => r.RouteId == routeId);

                // Store city IDs in ViewBag for filtering meal choices
                if (route != null)
                {
                    ViewBag.DepartureCityId = route.DepartureCityId;
                    ViewBag.ArrivalCityId = route.ArrivalCityId;
                }

                // Retrieve meal choices from the database and map to view models
                var mealChoiceEntities = await _dbContext.MealChoices
                    .Include(m => m.City)
                    .ToListAsync();

                var mealChoices = _mapper.Map<List<MealChoiceVM>>(mealChoiceEntities);
                ViewBag.MealChoices = mealChoices;

                // Retrieve booking classes from the database and map to view models
                var bookingClassEntities = await _dbContext.BookingClasses.ToListAsync();
                var bookingClasses = _mapper.Map<List<BookingClassVM>>(bookingClassEntities);
                ViewBag.BookingClasses = bookingClasses;

                return View(routeItem);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePassengers(int routeId, List<PassengerVM> passengers, int passengerCount)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem != null)
                {
                    // Update the passenger count
                    routeItem.PassengerCount = passengerCount;

                    // Take only the required number of passengers
                    var selectedPassengers = passengers.Take(passengerCount).ToList();

                    // Check for duplicate names
                    var duplicateNames = selectedPassengers
                        .GroupBy(p => new { FirstName = p.FirstName?.ToLower(), LastName = p.LastName?.ToLower() })
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateNames.Any())
                    {
                        TempData["Error"] = "Each passenger must have a unique name. Please ensure there are no duplicate names.";
                        return RedirectToAction("SelectPassengers", new { routeId = routeId });
                    }

                    // Validate that each passenger has selected a meal choice and booking class
                    if (selectedPassengers.Any(p => p.MealChoiceId == 0 || p.BookingClassId == 0))
                    {
                        TempData["Error"] = "Each passenger must select a meal preference and booking class.";
                        return RedirectToAction("SelectPassengers", new { routeId = routeId });
                    }

                    // Check if we have all required fields
                    if (selectedPassengers.Any(p =>
                        string.IsNullOrEmpty(p.FirstName) ||
                        string.IsNullOrEmpty(p.LastName) ||
                        string.IsNullOrEmpty(p.Email) ||
                        p.DateOfBirth == default))
                    {
                        TempData["Error"] = "Please provide all required information for each passenger.";
                        return RedirectToAction("SelectPassengers", new { routeId = routeId });
                    }

                    // Save passengers to database (if they don't already exist)
                    foreach (var passengerVM in selectedPassengers)
                    {
                        // Get booking class information
                        var bookingClass = await _dbContext.BookingClasses
                            .FirstOrDefaultAsync(bc => bc.BookingClassId == passengerVM.BookingClassId);

                        if (bookingClass != null)
                        {
                            passengerVM.BookingClassName = bookingClass.Description;
                            passengerVM.BookingClassPriceFactor = bookingClass.PriceFactor;
                        }

                        // Check if passenger with same name and email already exists
                        var existingPassenger = await _dbContext.Passengers
                            .FirstOrDefaultAsync(p =>
                                p.FirstName.ToLower() == passengerVM.FirstName.ToLower() &&
                                p.LastName.ToLower() == passengerVM.LastName.ToLower() &&
                                p.Email.ToLower() == passengerVM.Email.ToLower());

                        if (existingPassenger == null)
                        {
                            // Create new passenger entity
                            var passenger = new Passenger
                            {
                                FirstName = passengerVM.FirstName,
                                LastName = passengerVM.LastName,
                                Email = passengerVM.Email,
                                Birthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth)
                            };

                            // Add new passenger to database
                            await _dbContext.Passengers.AddAsync(passenger);
                            await _dbContext.SaveChangesAsync();

                            // Update the PassengerId in the ViewModel
                            passengerVM.PassengerId = passenger.PassengerId;
                        }
                        else
                        {
                            // Use the existing passenger's ID
                            passengerVM.PassengerId = existingPassenger.PassengerId;
                        }
                    }

                    // Update the passenger list in the cart
                    routeItem.Passengers = selectedPassengers;

                    // Recalculate the total price based on booking classes
                    routeItem.TotalPrice = routeItem.GetTotalPrice();

                    SaveCartToSession(cart);
                    TempData["Message"] = "Passenger information saved successfully.";

                    // Redirect to the Index action
                    return RedirectToAction("Index", "ShoppingCart");
                }
                else
                {
                    TempData["Error"] = "Route not found in your basket.";
                    return RedirectToAction("Index", "ShoppingCart");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }


        [HttpPost]
        public async Task<IActionResult> SelectPassengers(int routeId, int passengerCount)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem == null)
                {
                    TempData["Error"] = "Route not found in your basket.";
                    return RedirectToAction("Index");
                }

                // Update passenger count
                routeItem.PassengerCount = passengerCount;
                SaveCartToSession(cart);

                // Redirect back to GET action to show the updated form
                return RedirectToAction("SelectPassengers", new { routeId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult IncreasePassengerCount(int routeId)
        {
            var cart = GetCartFromSession();
            var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

            if (routeItem != null)
            {
                routeItem.PassengerCount++;
                routeItem.TotalPrice = routeItem.Flights.Sum(f => f.Price ?? 0) * routeItem.PassengerCount;
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DecreasePassengerCount(int routeId)
        {
            var cart = GetCartFromSession();
            var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

            if (routeItem != null && routeItem.PassengerCount > 1)
            {
                routeItem.PassengerCount--;
                routeItem.TotalPrice = routeItem.Flights.Sum(f => f.Price ?? 0) * routeItem.PassengerCount;
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveRouteFromCart(int routeId)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem != null)
                {
                    cart.RouteItems.Remove(routeItem);
                    SaveCartToSession(cart);
                    TempData["Message"] = "Route removed from your basket.";
                }
                else
                {
                    TempData["Error"] = "Route not found in your basket.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while removing the route: {ex.Message}";
            }

            return RedirectToAction("Index");
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
        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
                // Create a new empty cart
                var emptyCart = new ShoppingCartVM();

                // Save the empty cart to session
                SaveCartToSession(emptyCart);

                TempData["Message"] = "Your cart has been cleared.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to clear cart: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        public IActionResult ProceedToCheckout()
        {
            return RedirectToAction("ConfirmBooking", "Booking");
        }
    }
}