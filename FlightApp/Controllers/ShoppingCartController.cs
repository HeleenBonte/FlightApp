using AutoMapper;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Models;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                // Instead of redirecting to Index, redirect to SelectPassengers with the route ID
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

                // Retrieve meal choices from the database directly
                var mealChoices = await _dbContext.MealChoices.ToListAsync();
                ViewBag.MealChoices = mealChoices;

                // Log meal choices count for debugging
                Console.WriteLine($"Found {mealChoices.Count} meal choices");

                // Return the view with the route item to allow passenger selection
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

                    // Validate that each passenger has selected a meal choice
                    if (selectedPassengers.Any(p => p.MealChoiceId == 0))
                    {
                        TempData["Error"] = "Each passenger must select a meal preference.";
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
                                Birthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth),
                                // Don't set Country field - you mentioned to forget this
                                Country = string.Empty // Set to empty string as it's required in the model
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

                    // Recalculate the total price based on the number of passengers and all flights in the route
                    routeItem.TotalPrice = routeItem.Flights.Sum(f => f.Price ?? 0) * passengerCount;

                    SaveCartToSession(cart);
                    TempData["Message"] = "Passenger information saved successfully.";

                    // Explicitly redirect to the Index action
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
        public IActionResult RemoveFlightFromCart(int routeId, int flightId)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem != null)
                {
                    var flight = routeItem.Flights.FirstOrDefault(f => f.FlightId == flightId);
                    if (flight != null)
                    {
                        // If this is the only flight in the route, remove the whole route
                        if (routeItem.Flights.Count == 1)
                        {
                            cart.RouteItems.Remove(routeItem);
                            TempData["Message"] = "Flight removed from your basket.";
                        }
                        else
                        {
                            // Otherwise remove just this flight
                            routeItem.Flights.Remove(flight);

                            // Recalculate the total price
                            routeItem.TotalPrice = routeItem.Flights.Sum(f => f.Price ?? 0) * routeItem.PassengerCount;

                            TempData["Message"] = "Flight removed from your basket.";
                        }

                        SaveCartToSession(cart);
                    }
                    else
                    {
                        TempData["Error"] = "Flight not found in your basket.";
                    }
                }
                else
                {
                    TempData["Error"] = "Route not found in your basket.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while removing the flight: {ex.Message}";
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
    }
}
