using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Util.Hotels.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

namespace FlightApp.Controllers
{
    public class FlightsController : Controller
    {
        private IFlightService flightService;
        private IService<City> cityService;
        private IRouteService routeService;
        private readonly IHotelService _hotelService;

        private readonly IMapper _mapper;

        public FlightsController(IHotelService hotelService, IMapper mapper, IFlightService flightservice, IService<City> cityservice, IRouteService routeservice)
        {
            _mapper = mapper;
            _hotelService = hotelService;

            flightService = flightservice;
            cityService = cityservice;
            routeService = routeservice;
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

                Response.Headers.Append("X-Preserve-Auth", "true");

                FlightListHotelListVM flightListHotelListVM = new FlightListHotelListVM();
                flightListHotelListVM.flights = listVM;
                flightListHotelListVM.hotels = new List<HotelVM>();

                //if (listVM != null)
                //{
                //    var hotels = await GetHotelVMList(flightList.First().ArrivalCityNavigation.ApiId.ToString());
                //    flightListHotelListVM.hotels = hotels;
                //}

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
                            flightVMs.Add(_mapper.Map<FlightVM>(flight));
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

                    // Calculate and set the price for each route based on its flights
                    route.Price = flightVMs.Sum(f => f.Price ?? 0) > 0
                        ? Convert.ToDecimal(flightVMs.Sum(f => f.Price ?? 0))
                        : 0;
                }

                RouteListHotelListVM routeListHotelListVM = new RouteListHotelListVM();
                routeListHotelListVM.routes = listVM;
                routeListHotelListVM.hotels = new List<HotelVM>();

                //if (!routeList.IsNullOrEmpty())
                //{
                //    var hotels = await GetHotelVMList(routeList.First().ArrivalCity.ApiId.ToString());
                //    routeListHotelListVM.hotels = hotels;
                //}

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
        [HttpGet]
        public IActionResult AddToCart(int id)
        {
            return RedirectToAction("AddFlightToCart", "ShoppingCart", new { id });
        }
    }
}


