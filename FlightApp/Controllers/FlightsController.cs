using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
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
            
        private readonly IMapper _mapper;

        public FlightsController(IMapper mapper, IFlightService flightservice, IService<City> cityservice, IRouteService routeservice)
        {
            _mapper = mapper;
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
                return PartialView("_SearchFlightsPartial", listVM);
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
                        catch(Exception ex)
                        {
                            ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de flightVMs";
                            return View("Error");
                        }
                    }
                    route.Flights = flightVMs;
                    if (flightVMs.Count() == 2)
                    {
                        route.Layover1 = flightVMs[1].ArrivalCity;
                    }
                    else if (flightVMs.Count() == 3)
                    {
                        route.Layover1 = flightVMs[2].ArrivalCity;
                        route.Layover2 = flightVMs[1].ArrivalCity;
                    }
                }
                return PartialView("_SearchRoutesPartial", listVM);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                return View("Error");
            }
        }





    }
}


