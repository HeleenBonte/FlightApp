using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services;
using FlightApp.Services.Interfaces;
using FlightApp.Util.Hotels.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using System;

namespace FlightApp.Controllers
{
    public class FlightsController : Controller
    {
        private IFlightService flightService;
        private IService<City> cityService;
        private IRouteService routeService;
        private readonly IHotelService _hotelService;
        private readonly IHolidayPriceService _holidayPriceService;
        private readonly IMapper _mapper;

        public FlightsController(
            IHotelService hotelService,
            IMapper mapper,
            IFlightService flightservice,
            IService<City> cityservice,
            IRouteService routeservice,
            IHolidayPriceService holidayPriceService)
        {
            _mapper = mapper;
            _hotelService = hotelService;
            flightService = flightservice;
            cityService = cityservice;
            routeService = routeservice;
            _holidayPriceService = holidayPriceService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                FlightsCityVM flightsCityVM = new FlightsCityVM();
                flightsCityVM.Cities = new SelectList(await cityService.GetAllAsync(), "CityId", "CityName");
                return View(flightsCityVM);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FlightsCityVM entity)
        {
            if (entity.ArrivalCityID == 0 || entity.DepartureCityID == 0 || entity.DepartureDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("", "Vul alstublieft alle velden in.");
            }

            try
            {
                var flightList = await flightService.GetFlightsByCitiesID(Convert.ToInt16(entity.ArrivalCityID), Convert.ToInt16(entity.DepartureCityID), entity.DepartureDate);
                List<FlightVM> listVM = _mapper.Map<List<FlightVM>>(flightList);

                foreach (var flight in listVM)
                {
                    if (flight.DepartureTime.HasValue && flight.Price.HasValue)
                    {
                        double priceFactor = await _holidayPriceService.GetHolidayPriceFactor(
                            Convert.ToInt16(entity.DepartureCityID),
                            flight.DepartureTime.Value);

                        flight.Price = flight.Price * priceFactor;

                        if (priceFactor > 1.0)
                        {
                            flight.Notes = $"Holiday pricing applied (x{priceFactor})";
                        }
                    }
                }

                Response.Headers.Append("X-Preserve-Auth", "true");

                FlightListHotelListVM flightListHotelListVM = new FlightListHotelListVM();
                flightListHotelListVM.flights = listVM;
                flightListHotelListVM.hotels = new List<HotelVM>();

                if (listVM != null)
                {
                    var hotels = await GetHotelVMList(flightList.First().ArrivalCityNavigation.ApiId.ToString(), entity.DepartureDate);
                    flightListHotelListVM.hotels = hotels;
                }

                return PartialView("_SearchFlightsPartial", flightListHotelListVM);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                return View("Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            try
            {
                RouteCityVM routeCityVM = new RouteCityVM();
                routeCityVM.Cities = new SelectList(await cityService.GetAllAsync(), "CityId", "CityName");
                return View(routeCityVM);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRoutes(RouteCityVM entity)
        {
            if (entity.ArrivalCityID == 0 || entity.DepartureCityID == 0 || entity.DepartureDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("", "Vul alstublieft alle velden in.");
            }

            try
            {
                var routeList = await routeService.GetRoutesByCitiesID(Convert.ToInt16(entity.ArrivalCityID), Convert.ToInt16(entity.DepartureCityID), entity.DepartureDate);
                List<RouteVM> listVM = _mapper.Map<List<RouteVM>>(routeList);

                foreach (var route in listVM)
                {
                    List<FlightVM> flightVMs = new List<FlightVM>();
                    foreach (var v in route.Flights)
                    {
                        try
                        {
                            Flight flight = await flightService.FindByIdAsync(Convert.ToInt16(v.FlightId));
                            var flightVM = _mapper.Map<FlightVM>(flight);

                            if (flightVM.DepartureTime.HasValue && flightVM.Price.HasValue)
                            {
                                double priceFactor = await _holidayPriceService.GetHolidayPriceFactor(
                                    flight.DepartureCity,
                                    flightVM.DepartureTime.Value);

                                flightVM.Price = flightVM.Price * priceFactor;

                                if (priceFactor > 1.0)
                                {
                                    flightVM.Notes = $"Holiday pricing applied (x{priceFactor})";
                                }
                            }

                            flightVMs.Add(flightVM);
                        }
                        catch (Exception ex)
                        {
                            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                return Json(new { success = false, message = "Error loading flight data" });
                            }

                            ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de flightVMs";
                            return View("Error");
                        }
                    }

                    route.Flights = flightVMs;
                    if (flightVMs.Any())
                    {
                        route.ArrivalTime = flightVMs.Last().ArrivalTime;
                    }

                    if (flightVMs.Count() == 2)
                    {
                        route.Layover1 = flightVMs[1].ArrivalCity;
                    }
                    else if (flightVMs.Count() == 3)
                    {
                        route.Layover1 = flightVMs[2].ArrivalCity;
                        route.Layover2 = flightVMs[1].ArrivalCity;
                    }

                    route.Price = flightVMs.Sum(f => f.Price ?? 0) > 0
                        ? Convert.ToDecimal(flightVMs.Sum(f => f.Price ?? 0))
                        : 0;
                }

                RouteListHotelListVM routeListHotelListVM = new RouteListHotelListVM();
                routeListHotelListVM.routes = listVM;
                routeListHotelListVM.hotels = new List<HotelVM>();

                if (!routeList.IsNullOrEmpty())
                {
                    var hotels = await GetHotelVMList(routeList.First().ArrivalCity.ApiId.ToString(), entity.DepartureDate);
                    routeListHotelListVM.hotels = hotels;
                }

                Response.Headers.Append("X-Preserve-Auth", "true");
                return PartialView("_SearchRoutesPartial", routeListHotelListVM);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult ViewCart()
        {
            return RedirectToAction("Index", "ShoppingCart");
        }

        
        [HttpGet]
        public IActionResult AddToCart(int id)
        {
            return RedirectToAction("AddFlightToCart", "ShoppingCart", new { id });
        }



        public async Task<List<HotelVM>> GetHotelVMList(string cityApiID, DateOnly apiArrivalDate)
        {


            var lstHotelIds = await _hotelService.GetHotelIdsAsync(cityApiID, apiArrivalDate);
            if (lstHotelIds.Count > 0)
            {
                lstHotelIds = lstHotelIds.Slice(0, 3);
                List<HotelVM> hotels = new List<HotelVM>();
                foreach (var hotelId in lstHotelIds)
                {
                    var hotel = await _hotelService.GetHotelByIdAsync(hotelId.hotel_id, apiArrivalDate);
                    var hotelvm = new HotelVM();
                    if (hotel != null)
                    {
                        hotelvm = _mapper.Map<HotelVM>(hotel);
                    }
                    else
                    {
                        hotelvm = new HotelVM
                        {
                            Hotel_name = "citizenM Tower of London",
                            Url = "https://www.booking.com/hotel/gb/citizenm-tower-of-london-london.html",
                            Price = 640.375940574,
                            PriceString = "€ 640",
                            PhotoUrls = new List<string>{
                        "https://cf.bstatic.com/xdata/images/hotel/square60/585088806.jpg?k=288d320226e95808f2cf8e288a7984f01e984ba8587e77cbc92470dd54b230b0&o="
                        },
                            ReviewScore = 8.4,
                            isApiData = false
                        };
                    }

                    hotels.Add(hotelvm);
                }
                return hotels;
            }
            else
            {
                List<HotelVM> hotels = new List<HotelVM>
                {
                     new HotelVM
                    {
                        Hotel_name = "The Dilly",
                        Url = "https://www.booking.com/hotel/gb/the-dilly.html",
                        Price = 315.9093475697,
                        PriceString = "€ 316",
                        PhotoUrls = new List<string>{
                        "https://cf.bstatic.com/xdata/images/hotel/square60/286431489.jpg?k=2fbc9b5473242be250625b97f050b7463d3e19f3966264e645b0b534dce0d17f&o="
                        },
                        ReviewScore = 10,
                        isApiData = false
                    },
                    new HotelVM
                    {
                        Hotel_name = "citizenM Tower of London",
                        Url = "https://www.booking.com/hotel/gb/citizenm-tower-of-london-london.html",
                        Price = 640.375940574,
                        PriceString = "€ 640",
                        PhotoUrls = new List<string>{
                        "https://cf.bstatic.com/xdata/images/hotel/square60/585088806.jpg?k=288d320226e95808f2cf8e288a7984f01e984ba8587e77cbc92470dd54b230b0&o="
                        },
                        ReviewScore = 8.4,
                        isApiData = false
                    },
                     new HotelVM
                    {
                        Hotel_name = "Park Plaza County Hall London",
                        Url = "https://www.booking.com/hotel/gb/park-plaza-county-hall.html",
                        Price = 771.872168772,
                        PriceString = "€ 772",
                        PhotoUrls = new List<string>{
                        "https://cf.bstatic.com/xdata/images/hotel/square60/511738782.jpg?k=4d2318d8a7b43166b4888c095a870e8f91faf2ba01937b2f4231722e79c35b79&o="
                        },
                        ReviewScore = 8.6,
                        isApiData = false
                    }
                };
                return hotels;
            }
        }
    }
}


