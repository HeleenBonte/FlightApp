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
        private readonly IMapper _mapper;
        private readonly IHolidayPriceService _holidayPriceService;
        private readonly IRouteService _routeService;
        private readonly IFlightService _fligthService;
        private readonly IService<MealChoice> _mealChoiceService;
        private readonly IService<BookingClass> _bookingClassService;
        private readonly IPassengerService _passengerService;

        public ShoppingCartController(
            IMapper mapper,
            IHolidayPriceService holidayPriceService,
            IRouteService routeService,
            IFlightService flightService,
            IService<MealChoice> mealChoiceService,
            IService<BookingClass> bookingClassService,
            IPassengerService passengerService)
        {
            _mapper = mapper;
            _holidayPriceService = holidayPriceService;
            _routeService = routeService;
            _fligthService = flightService;
            _mealChoiceService = mealChoiceService;
            _bookingClassService = bookingClassService;
            _passengerService = passengerService;
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
                if (!User.Identity?.IsAuthenticated ?? false)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                var route = await _routeService.FindByIdAsync(id);


                if (route == null)
                {
                    return NotFound();
                }

                var cart = GetCartFromSession(id, null);

                var existingRouteItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == id);
                if (existingRouteItem != null)
                {
                    if (existingRouteItem.Passengers == null || !existingRouteItem.Passengers.Any() ||
                        existingRouteItem.Passengers.Count < existingRouteItem.PassengerCount)
                    {
                        return RedirectToAction("SelectPassengers", new { routeId = id });
                    }

                    TempData["Message"] = "This route is already in your basket with passenger information.";
                    return RedirectToAction("Index");
                }

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
                    IsComplete = false,
                    AddedToCartTime = DateTime.Now
                };

                foreach (var flight in route.Flights)
                {
                    var flightDetail = await _fligthService.GetFlightByIDAsync(flight.FlightId);

                    if (flightDetail != null)
                    {
                        var flightVM = _mapper.Map<FlightVM>(flightDetail);

                        if (flightDetail.DepartureTime.HasValue && flightDetail.Price > 0)
                        {
                            double holidayFactor = await _holidayPriceService.GetHolidayPriceFactor(
                                flightDetail.DepartureCity,
                                flightDetail.DepartureTime.Value);

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

                cart.RouteItems.Add(routeCartItem);
                SaveCartToSession(cart);

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
                var cart = GetCartFromSession(routeId, null); 
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem == null)
                {
                    TempData["Error"] = "Route not found in your basket.";
                    return RedirectToAction("Index");
                }

                if (passengerCount.HasValue)
                {
                    routeItem.PassengerCount = passengerCount.Value;
                    SaveCartToSession(cart);
                }

                var route = await _routeService.FindByIdAsync(routeId);

                if (route != null)
                {
                    ViewBag.DepartureCityId = route.DepartureCityId;
                    ViewBag.ArrivalCityId = route.ArrivalCityId;
                }

                var mealChoiceEntities = await _mealChoiceService.GetAllAsync();

                var mealChoices = _mapper.Map<List<MealChoiceVM>>(mealChoiceEntities);
                ViewBag.MealChoices = mealChoices;

                var bookingClassEntities = await _bookingClassService.GetAllAsync();
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
                var cart = GetCartFromSession(routeId, null);
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem != null)
                {
                    routeItem.PassengerCount = passengerCount;

                    var selectedPassengers = passengers.Take(passengerCount).ToList();

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

                    if (selectedPassengers.Any(p => p.MealChoiceId == 0 || p.BookingClassId == 0))
                    {
                        TempData["Error"] = "Each passenger must select a meal preference and booking class.";
                        return RedirectToAction("SelectPassengers", new { routeId = routeId });
                    }

                    if (selectedPassengers.Any(p =>
                        string.IsNullOrEmpty(p.FirstName) ||
                        string.IsNullOrEmpty(p.LastName) ||
                        string.IsNullOrEmpty(p.Email) ||
                        p.DateOfBirth == default))
                    {
                        TempData["Error"] = "Please provide all required information for each passenger.";
                        return RedirectToAction("SelectPassengers", new { routeId = routeId });
                    }

                    foreach (var passengerVM in selectedPassengers)
                    {
                        var bookingClass = await _bookingClassService.FindByIdAsync(passengerVM.BookingClassId);

                        if (bookingClass != null)
                        {
                            passengerVM.BookingClassName = bookingClass.Description;
                            passengerVM.BookingClassPriceFactor = bookingClass.PriceFactor;
                        }

                        var existingPassenger = await _passengerService.FindIsExistingPassenger(passengerVM.FirstName.ToLower(),
                            passengerVM.LastName.ToLower(),
                            passengerVM.Email.ToLower());

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

                            await _passengerService.AddAsync(passenger);

                            passengerVM.PassengerId = passenger.PassengerId;
                        }
                        else
                        {
                            DateOnly newBirthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth);
                            if (existingPassenger.Birthdate != newBirthdate)
                            {
                                existingPassenger.Birthdate = newBirthdate;
                                await _passengerService.UpdateAsync(existingPassenger);
                            }

                            passengerVM.PassengerId = existingPassenger.PassengerId;
                        }
                    }

                    routeItem.Passengers = selectedPassengers;

                    routeItem.IsComplete = true;

                    routeItem.TotalPrice = routeItem.GetTotalPrice();

                    SaveCartToSession(cart);
                    TempData["Message"] = "Route added to your basket with passenger information.";

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
                var cart = GetCartFromSession(routeId, null);
                var routeItem = cart.RouteItems.FirstOrDefault(r => r.RouteId == routeId);

                if (routeItem == null)
                {
                    TempData["Error"] = "Route not found in your basket.";
                    return RedirectToAction("Index");
                }

                routeItem.PassengerCount = passengerCount;
                SaveCartToSession(cart);

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
            var cart = GetCartFromSession(routeId, null);
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
            var cart = GetCartFromSession(routeId, null); 
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
                var emptyCart = new ShoppingCartVM();

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

                var flight = await _fligthService.FindByIdAsync(id);

                if (flight == null)
                {
                    return NotFound();
                }

                var cart = GetCartFromSession(null, id);

                var existingFlightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == id);
                if (existingFlightItem != null)
                {
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
                    IsComplete = false,
                    AddedToCartTime = DateTime.Now
                };

                cart.FlightItems.Add(flightCartItem);
                SaveCartToSession(cart);

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
                var cart = GetCartFromSession(null, flightId);
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem == null)
                {
                    TempData["Error"] = "Flight not found in your basket.";
                    return RedirectToAction("Index");
                }

                if (passengerCount.HasValue)
                {
                    flightItem.PassengerCount = passengerCount.Value;
                    SaveCartToSession(cart);
                }

                var flight = await _fligthService.FindByIdAsync(flightId);

                if (flight != null)
                {
                    ViewBag.DepartureCityId = flight.DepartureCity;
                    ViewBag.ArrivalCityId = flight.ArrivalCity;
                }

                var mealChoiceEntities = await _mealChoiceService.GetAllAsync();

                var mealChoices = _mapper.Map<List<MealChoiceVM>>(mealChoiceEntities);
                ViewBag.MealChoices = mealChoices;

                var bookingClassEntities = await _bookingClassService.GetAllAsync();
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
                var cart = GetCartFromSession(null, flightId);
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem == null)
                {
                    TempData["Error"] = "Flight not found in your basket.";
                    return RedirectToAction("Index");
                }

                flightItem.PassengerCount = passengerCount;
                SaveCartToSession(cart);

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
                var cart = GetCartFromSession(null, flightId);
                var flightItem = cart.FlightItems.FirstOrDefault(f => f.FlightId == flightId);

                if (flightItem != null)
                {
                    flightItem.PassengerCount = passengerCount;

                    var selectedPassengers = passengers.Take(passengerCount).ToList();

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

                    if (selectedPassengers.Any(p => p.MealChoiceId == 0 || p.BookingClassId == 0))
                    {
                        TempData["Error"] = "Each passenger must select a meal preference and booking class.";
                        return RedirectToAction("SelectFlightPassengers", new { flightId = flightId });
                    }

                    if (selectedPassengers.Any(p =>
                        string.IsNullOrEmpty(p.FirstName) ||
                        string.IsNullOrEmpty(p.LastName) ||
                        string.IsNullOrEmpty(p.Email) ||
                        p.DateOfBirth == default))
                    {
                        TempData["Error"] = "Please provide all required information for each passenger.";
                        return RedirectToAction("SelectFlightPassengers", new { flightId = flightId });
                    }

                    foreach (var passengerVM in selectedPassengers)
                    {
                        var bookingClass = await _bookingClassService.FindByIdAsync(passengerVM.BookingClassId);

                        if (bookingClass != null)
                        {
                            passengerVM.BookingClassName = bookingClass.Description;
                            passengerVM.BookingClassPriceFactor = bookingClass.PriceFactor;
                        }

                        var existingPassenger = await _passengerService.FindIsExistingPassenger(passengerVM.FirstName,
                            passengerVM.LastName,
                            passengerVM.Email);


                        if (existingPassenger == null)
                        {
                            var passenger = new Passenger
                            {
                                FirstName = passengerVM.FirstName,
                                LastName = passengerVM.LastName,
                                Email = passengerVM.Email,
                                Birthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth)
                            };

                            await _passengerService.AddAsync(passenger);

                            passengerVM.PassengerId = passenger.PassengerId;
                        }
                        else
                        {
                            DateOnly newBirthdate = DateOnly.FromDateTime(passengerVM.DateOfBirth);
                            if (existingPassenger.Birthdate != newBirthdate)
                            {
                                existingPassenger.Birthdate = newBirthdate;
                                await _passengerService.UpdateAsync(existingPassenger);
                            }

                            passengerVM.PassengerId = existingPassenger.PassengerId;
                        }
                    }

                    flightItem.Passengers = selectedPassengers;

                    flightItem.IsComplete = true;

                    flightItem.TotalPrice = flightItem.GetTotalPrice();

                    SaveCartToSession(cart);
                    TempData["Message"] = "Flight added to your basket with passenger information.";

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

            int routesBefore = cart.RouteItems.Count;
            int flightsBefore = cart.FlightItems.Count;

            cart.RouteItems.RemoveAll(r => !r.IsComplete && r.RouteId != excludeRouteId);

            cart.FlightItems.RemoveAll(f => !f.IsComplete && f.FlightId != excludeFlightId);

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