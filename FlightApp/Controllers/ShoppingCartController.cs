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
        public IActionResult SavePassengers(int routeId, List<PassengerVM> passengers, int passengerCount)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem != null)
                {
                    // Update the passenger count
                    routeItem.PassengerCount = passengerCount;

                    // Update the passenger list - ensure we have the right number of passengers
                    routeItem.Passengers = passengers.Take(passengerCount).ToList();

                    // Validate that each passenger has selected a meal choice
                    if (routeItem.Passengers.Any(p => p.MealChoiceId == 0))
                    {
                        TempData["Error"] = "Each passenger must select a meal preference.";
                        return RedirectToAction("SelectPassengers", new { routeId = routeId });
                    }

                    // Recalculate the total price based on the number of passengers
                    routeItem.TotalPrice = routeItem.Flights.Sum(f => f.Price ?? 0) * passengerCount;

                    SaveCartToSession(cart);
                    TempData["Message"] = "Passenger information saved successfully.";
                }
                else
                {
                    TempData["Error"] = "Route not found in your basket.";
                }

                return RedirectToAction("Index");
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
        public IActionResult RemoveRouteFromCart(int id)
        {
            try
            {
                var cart = GetCartFromSession();
                var routeToRemove = cart.RouteItems.FirstOrDefault(r => r.RouteId == id);

                if (routeToRemove != null)
                {
                    cart.RouteItems.Remove(routeToRemove);
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
    }
}
