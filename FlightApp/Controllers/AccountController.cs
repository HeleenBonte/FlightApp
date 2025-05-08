using AutoMapper;
using FlightApp.Areas.Identity.Data;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interfaces;
using FlightApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlightApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingHistoryDAO _bookingHistoryDAO;
        private readonly IMapper _mapper;
        public AccountController(UserManager<ApplicationUser> userManager, IBookingHistoryDAO bookingHistoryDAO, IMapper mapper)
        {
            _userManager = userManager;
            _bookingHistoryDAO = bookingHistoryDAO;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                try { 
                // User is logged in, redirect to the desired page
                var history = await _bookingHistoryDAO.GetAllByUserIdAsync(userId);
                List<BookingHistoryVM> bookingHistoryVMs = _mapper.Map<List<BookingHistoryVM>>(history);
                return View(bookingHistoryVMs);
            }
                catch(Exception ex) {
                    ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                    return View("Error");
                }
            }
            return View();
            
        }
    }
}
