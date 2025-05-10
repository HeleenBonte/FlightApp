using AutoMapper;
using FlightApp.Areas.Identity.Data;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interfaces;
using FlightApp.Util.Hotels.Interfaces;
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
        private readonly IHotelService _hotelService;
        public AccountController(UserManager<ApplicationUser> userManager, IBookingHistoryDAO bookingHistoryDAO, IMapper mapper, IHotelService hotelService)
        {
            _userManager = userManager;
            _bookingHistoryDAO = bookingHistoryDAO;
            _mapper = mapper;
            _hotelService = hotelService;
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

                var hotels = await GetHotelVMList(bookingHistoryVMs.First().ArrivalCityData.ApiId.ToString());
                    BookingHistoryHotelListVM bookingHistoryHotelListVM = new BookingHistoryHotelListVM();
                    bookingHistoryHotelListVM.BookingHistory = bookingHistoryVMs;
                    bookingHistoryHotelListVM.Hotels = hotels;

                    return View(bookingHistoryHotelListVM);
            }
                catch(Exception ex) {
                    ViewBag.ErrorMessage = "Er is een probleem opgetreden bij het ophalen van de lijst";
                    return View("Error");
                }
            }
            return View();
            
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
    }
}
