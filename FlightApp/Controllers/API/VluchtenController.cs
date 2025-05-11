using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace FlightApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class VluchtenController : ControllerBase
    {
        // /api/vluchten geeft een lijst van alle vluchten die tussen twee opgegeven luchthavens
        // parameters: vertrekluchthavenID && aankomstluchthavenID
        private IFlightService _flightService;
        private readonly IMapper _mapper;
        
        public VluchtenController(IMapper mapper, IFlightService flightService)
        {
            _mapper = mapper;
            _flightService = flightService;
        }

        [HttpGet("{vertrekID, aankomstID}", Name = "Get")]
        public async Task<ActionResult<FlightVM>> Get(int vertrekID, int aankomstID)
        {
            try
            {
                var flights = await _flightService.GetFlightsByCitiesID(vertrekID, aankomstID);
                List<FlightVM> data = _mapper.Map<List<FlightVM>>(flights);
                if (data == null)
                {
                    return NotFound();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Er is een interne fout opgetreden. Neem contact op met de beheerder."
                });
            }
        }
        
    }
}
