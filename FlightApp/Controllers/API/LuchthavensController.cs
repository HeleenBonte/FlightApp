using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class LuchthavensController : ControllerBase
    {
        private IService<City> _cityService;
        private readonly IMapper _mapper;

        public LuchthavensController(IService<City> cityService, IMapper mapper)
        {
            _cityService = cityService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CityVM>> Get()
        {
            try
            {
                var list = await _cityService.GetAllAsync();
                List<CityVM> data = _mapper.Map<List<CityVM>>(list);

                if (data == null)
                {
                    return NotFound();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
