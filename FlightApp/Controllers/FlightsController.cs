using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            if (entity.ArrivalCityID == 0 || entity.DepartureCityID == 0)
            {
                ModelState.AddModelError("", "Vul alstublieft alle velden in.");
            }
            try
            {
                var flightList = await flightService.GetFlightsByCitiesID(Convert.ToInt16(entity.ArrivalCityID), Convert.ToInt16(entity.DepartureCityID));
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
            if (entity.ArrivalCityID == 0 || entity.DepartureCityID == 0)
            {
                ModelState.AddModelError("", "Vul alstublieft alle velden in.");
            }
            try
            {
                var routeList = await routeService.GetRoutesByCitiesID(Convert.ToInt16(entity.ArrivalCityID), Convert.ToInt16(entity.DepartureCityID));
                List<RouteVM> listVM = _mapper.Map<List<RouteVM>>(routeList);
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


