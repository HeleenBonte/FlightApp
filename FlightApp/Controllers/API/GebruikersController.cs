using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FlightApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class GebruikersController : ControllerBase
    {

        // /api/gebruikers geeft een lijst van alle geregistreerde gebruikers

        private IService<AspNetUser> _UserService;
        private readonly IMapper _mapper;

        public GebruikersController(IService<AspNetUser> userService, IMapper mapper)
        {
            _UserService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ASPNetUserVM>> Get()
        {
            try
            {
                var user = await _UserService.GetAllAsync();
                List<ASPNetUserVM> data = _mapper.Map<List<ASPNetUserVM>>(user);
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
