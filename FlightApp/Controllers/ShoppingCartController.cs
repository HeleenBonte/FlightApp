using AutoMapper;
using FlightApp.Domains.DataDB;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Models;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using FlightApp.Services.Interfaces;

namespace FlightApp.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly FlightsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHolidayPriceService _holidayPriceService;

        public ShoppingCartController(
            FlightsDbContext dbContext,
            IMapper mapper,
            IHolidayPriceService holidayPriceService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _holidayPriceService = holidayPriceService;
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
                if (!User.Identity?.IsAuthenticated ?? false)
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

                var cart = GetCartFromSession(id, null); // Pass ID to exclude from cleanup

                // Check if route is already in cart
                var existingRouteItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == id);
                if (existingRouteItem != null)
                {
                    // If route exists but doesn't have passengers, redirect to select them
                    if (existingRouteItem.Passengers == null || !existingRouteItem.Passengers.Any() ||
                        existingRouteItem.Passengers.Count < existingRouteItem.PassengerCount)
                    {
                        return RedirectToAction("SelectPassengers", new { routeId = id });
                    }

                    TempData["Message"] = "This route is already in your basket with passenger information.";
                    return RedirectToAction("Index");
                }

                // Create new route cart item (temporary, not saved to session yet)
                var routeCartItem = new RouteCartItemVM
                {
                    RouteId = route.RouteId,
                    DepartureTime = route.DepartureTime ?? DateTime.Now,
                    DepartureCity = route.DepartureCity?.CityName,
                    ArrivalCity = route.ArrivalCity?.CityName,
                    ArrivalTime = route.ArrivalTime,
                    Flights = new List<FlightVM>(),
                    TotalPrice = 0,
                    Passengers = new List<PassengerVM>(),
                    IsComplete = false, // Mark as incomplete until passengers are added
                    AddedToCartTime = DateTime.Now
                };

                // Fetch flight details and calculate total price with holiday factor
                foreach (var flight in route.Flights)
                {
                    var flightDetail = await _dbContext.Flights
                        .Include(f => f.ArrivalCityNavigation)
                        .Include(f => f.DepartureCityNavigation)
                        .FirstOrDefaultAsync(f => f.FlightId == flight.FlightId);

                    if (flightDetail != null)
                    {
                        var flightVM = _mapper.Map<FlightVM>(flightDetail);

                        // Apply holiday price factor if departure time is set
                        if (flightDetail.DepartureTime.HasValue && flightDetail.Price > 0)
                        {
                            double holidayFactor = await _holidayPriceService.GetHolidayPriceFactor(
                                flightDetail.DepartureCity,
                                flightDetail.DepartureTime.Value);

                            // Apply holiday factor if it's different from 1.0
                            if (Math.Abs(holidayFactor - 1.0) > 0.01)
                            {
                                flightVM.Price = flightDetail.Price * holidayFactor;
                                flightVM.Notes = $"Holiday pricing applied (x{holidayFactor:F2})";
                            }
                            else
                            {
                                flightVM.Price = flightDetail.Price;
                            }
                        }
                        else
                        {
                            flightVM.Price = flightDetail.Price;
                        }

                        routeCartItem.Flights.Add(flightVM);
                        routeCartItem.TotalPrice += flightVM.Price ?? 0;
                    }
                }

                // Add to cart but mark as temporary
                cart.RouteItems.Add(routeCartItem);
                SaveCartToSession(cart);

                // Redirect directly to passenger selection
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
                var cart = GetCartFromSession(routeId, null); // Pass routeId to exclude this item from cleanup
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
                var cart = GetCartFromSession(routeId, null); // Pass routeId to prevent cleanup
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
                            // Check if date of birth is different and update if needed
                            DateOnly newBirthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth);
                            if (existingPassenger.Birthdate != newBirthdate)
                            {
                                // Update the passenger's date of birth in the database
                                existingPassenger.Birthdate = newBirthdate;
                                _dbContext.Passengers.Update(existingPassenger);
                                await _dbContext.SaveChangesAsync();
                            }

                            // Use the existing passenger's ID
                            passengerVM.PassengerId = existingPassenger.PassengerId;
                        }
                    }

                    // Update the passenger list in the cart
                    routeItem.Passengers = selectedPassengers;

                    // Mark as complete now that passenger information is added
                    routeItem.IsComplete = true;

                    // Recalculate the total price based on booking classes
                    routeItem.TotalPrice = routeItem.GetTotalPrice();

                    SaveCartToSession(cart);
                    TempData["Message"] = "Route added to your basket with passenger information.";

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
                var cart = GetCartFromSession(routeId, null); // Pass routeId to prevent cleanup
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
            var cart = GetCartFromSession(routeId, null); // Pass routeId to prevent cleanup
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
            var cart = GetCartFromSession(routeId, null); // Pass routeId to prevent cleanup
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


        private ShoppingCartVM GetCartFromSession(int? excludeRouteId = null, int? excludeFlightId = null)
        {
            try
            {
                var cartJson = HttpContext.Session.GetString("Cart");
                if (string.IsNullOrEmpty(cartJson))
                {
                    return new ShoppingCartVM();
                }

                var cart = JsonSerializer.Deserialize<ShoppingCartVM>(cartJson) ?? new ShoppingCartVM();

                // Clean up incomplete items that have been in the cart too long
                // But preserve the one being edited
                CleanIncompleteCartItems(cart, excludeRouteId, excludeFlightId);

                return cart;
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

        [HttpGet]
        public async Task<IActionResult> AddFlightToCart(int id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? false)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var flight = await _dbContext.Flights
                    .Include(f => f.DepartureCityNavigation)
                    .Include(f => f.ArrivalCityNavigation)
                    .FirstOrDefaultAsync(f => f.FlightId == id);

                if (flight == null)
                {
                    return NotFound();
                }

                var cart = GetCartFromSession(null, id); // Pass ID to exclude from cleanup

                var existingFlightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == id);
                if (existingFlightItem != null)
                {
                    // If flight exists but doesn't have passengers, redirect to select them
                    if (existingFlightItem.Passengers == null || !existingFlightItem.Passengers.Any() ||
                        existingFlightItem.Passengers.Count < existingFlightItem.PassengerCount)
                    {
                        return RedirectToAction("SelectFlightPassengers", new { flightId = id });
                    }

                    TempData["Message"] = "This flight is already in your basket with passenger information.";
                    return RedirectToAction("Index");
                }

                double basePrice = flight.Price;
                double holidayFactor = 1.0;
                string? holidayNotes = null;

                if (flight.DepartureTime.HasValue)
                {
                    holidayFactor = await _holidayPriceService.GetHolidayPriceFactor(
                        flight.DepartureCity,
                        flight.DepartureTime.Value);

                    if (Math.Abs(holidayFactor - 1.0) > 0.01)
                    {
                        holidayNotes = $"Holiday pricing applied (x{holidayFactor:F2})";
                    }
                }

                double adjustedPrice = basePrice * holidayFactor;

                var flightCartItem = new FlightCartItemVM
                {
                    FlightId = flight.FlightId,
                    DepartureTime = flight.DepartureTime,
                    DepartureCity = flight.DepartureCityNavigation?.CityName,
                    ArrivalCity = flight.ArrivalCityNavigation?.CityName,
                    ArrivalTime = flight.ArrivalTime,
                    Price = adjustedPrice,
                    TotalPrice = adjustedPrice,
                    Passengers = new List<PassengerVM>(),
                    Notes = holidayNotes,
                    IsComplete = false, // Mark as incomplete until passengers are added
                    AddedToCartTime = DateTime.Now
                };

                cart.FlightItems.Add(flightCartItem);
                SaveCartToSession(cart);

                // Redirect to select passengers immediately
                return RedirectToAction("SelectFlightPassengers", new { flightId = id });
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SelectFlightPassengers(int flightId, int? passengerCount = null)
        {
            try
            {
                var cart = GetCartFromSession(null, flightId); // Pass flightId to exclude this item from cleanup
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem == null)
                {
                    TempData["Error"] = "Flight not found in your basket.";
                    return RedirectToAction("Index");
                }

                // Update passenger count if provided
                if (passengerCount.HasValue)
                {
                    flightItem.PassengerCount = passengerCount.Value;
                    SaveCartToSession(cart);
                }

                // Get the flight details to determine departure and arrival cities
                var flight = await _dbContext.Flights
                    .FirstOrDefaultAsync(f => f.FlightId == flightId);

                // Store city IDs in ViewBag for filtering meal choices
                if (flight != null)
                {
                    ViewBag.DepartureCityId = flight.DepartureCity;
                    ViewBag.ArrivalCityId = flight.ArrivalCity;
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

                return View(flightItem);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SelectFlightPassengers(int flightId, int passengerCount)
        {
            try
            {
                var cart = GetCartFromSession(null, flightId); // Pass flightId to prevent cleanup
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem == null)
                {
                    TempData["Error"] = "Flight not found in your basket.";
                    return RedirectToAction("Index");
                }

                // Update passenger count
                flightItem.PassengerCount = passengerCount;
                SaveCartToSession(cart);

                // Redirect back to GET action to show the updated form
                return RedirectToAction("SelectFlightPassengers", new { flightId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveFlightPassengers(int flightId, List<PassengerVM> passengers, int passengerCount)
        {
            try
            {
                var cart = GetCartFromSession(null, flightId); // Pass flightId to prevent cleanup
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem != null)
                {
                    // Update the passenger count
                    flightItem.PassengerCount = passengerCount;

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
                        return RedirectToAction("SelectFlightPassengers", new { flightId = flightId });
                    }

                    // Validate that each passenger has selected a meal choice and booking class
                    if (selectedPassengers.Any(p => p.MealChoiceId == 0 || p.BookingClassId == 0))
                    {
                        TempData["Error"] = "Each passenger must select a meal preference and booking class.";
                        return RedirectToAction("SelectFlightPassengers", new { flightId = flightId });
                    }

                    // Check if we have all required fields
                    if (selectedPassengers.Any(p =>
                        string.IsNullOrEmpty(p.FirstName) ||
                        string.IsNullOrEmpty(p.LastName) ||
                        string.IsNullOrEmpty(p.Email) ||
                        p.DateOfBirth == default))
                    {
                        TempData["Error"] = "Please provide all required information for each passenger.";
                        return RedirectToAction("SelectFlightPassengers", new { flightId = flightId });
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
                            // Check if date of birth is different and update if needed
                            DateOnly newBirthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth);
                            if (existingPassenger.Birthdate != newBirthdate)
                            {
                                // Update the passenger's date of birth in the database
                                existingPassenger.Birthdate = newBirthdate;
                                _dbContext.Passengers.Update(existingPassenger);
                                await _dbContext.SaveChangesAsync();
                            }

                            // Use the existing passenger's ID
                            passengerVM.PassengerId = existingPassenger.PassengerId;
                        }
                    }

                    // Update the passenger list in the cart
                    flightItem.Passengers = selectedPassengers;

                    // Mark as complete now that passenger information is added
                    flightItem.IsComplete = true;

                    // Recalculate the total price based on booking classes
                    flightItem.TotalPrice = flightItem.GetTotalPrice();

                    SaveCartToSession(cart);
                    TempData["Message"] = "Flight added to your basket with passenger information.";

                    // Redirect to the Index action
                    return RedirectToAction("Index", "ShoppingCart");
                }
                else
                {
                    TempData["Error"] = "Flight not found in your basket.";
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
        public IActionResult RemoveFlightFromCart(int flightId)
        {
            try
            {
                var cart = GetCartFromSession();
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem != null)
                {
                    cart.FlightItems.Remove(flightItem);
                    SaveCartToSession(cart);
                    TempData["Message"] = "Flight removed from your basket.";
                }
                else
                {
                    TempData["Error"] = "Flight not found in your basket.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while removing the flight: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        private void CleanIncompleteCartItems(ShoppingCartVM cart, int? excludeRouteId = null, int? excludeFlightId = null)
        {
            if (cart == null) return;

            // Count items before cleanup
            int routesBefore = cart.RouteItems.Count;
            int flightsBefore = cart.FlightItems.Count;

            // Remove ALL incomplete route items EXCEPT the one being edited
            cart.RouteItems.RemoveAll(r => !r.IsComplete && r.RouteId != excludeRouteId);

            // Remove ALL incomplete flight items EXCEPT the one being edited
            cart.FlightItems.RemoveAll(f => !f.IsComplete && f.FlightId != excludeFlightId);

            // Count items after cleanup
            int routesAfter = cart.RouteItems.Count;
            int flightsAfter = cart.FlightItems.Count;

            if (routesBefore > routesAfter || flightsBefore > flightsAfter)
            {
                int routesRemoved = routesBefore - routesAfter;
                int flightsRemoved = flightsBefore - flightsAfter;

                System.Diagnostics.Debug.WriteLine(
                    $"Cart cleanup: Removed {routesRemoved} incomplete routes and {flightsRemoved} incomplete flights");
            }
        }
    }
}