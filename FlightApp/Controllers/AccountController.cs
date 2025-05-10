using AutoMapper;
using FlightApp.Areas.Identity.Data;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interfaces;
using FlightApp.Services.Interfaces;
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
        private readonly IBookingHistoryService _bookingHistoryService;
        private readonly IMapper _mapper;
        private readonly IHotelService _hotelService;
        private readonly IService <Booking> _bookingService;
        public AccountController(UserManager<ApplicationUser> userManager, IBookingHistoryService bookingHistoryService, IMapper mapper, IHotelService hotelService, IService<Booking> bookingService)
        {
            _userManager = userManager;
            _bookingHistoryService = bookingHistoryService;
            _mapper = mapper;
            _hotelService = hotelService;
            _bookingService = bookingService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                try { 
                // User is logged in, redirect to the desired page
                var history = await _bookingHistoryService.GetAllByUserIdAsync(userId);
                List<BookingHistoryVM> bookingHistoryVMs = _mapper.Map<List<BookingHistoryVM>>(history);

                    // Handle case with no bookings
                    if (!bookingHistoryVMs.Any())
                    {
                        BookingHistoryHotelListVM emptyModel = new BookingHistoryHotelListVM
                        {
                            BookingHistory = new List<BookingHistoryVM>(),
                            Hotels = new List<HotelVM>()
                        };
                        return View(emptyModel);
                    }

                    // Get hotel recommendations for the first booking's arrival city
                    var firstBookingWithCity = bookingHistoryVMs.FirstOrDefault(b => b.ArrivalCityData?.ApiId != null);
                    List<HotelVM> hotels = new List<HotelVM>();

                    if (firstBookingWithCity != null)
                    {
                        hotels = await GetHotelVMList(firstBookingWithCity.ArrivalCityData.ApiId.ToString());
                    }

                    BookingHistoryHotelListVM bookingHistoryHotelListVM = new BookingHistoryHotelListVM
                    {
                        BookingHistory = bookingHistoryVMs,
                        Hotels = hotels
                    };

                    return View(bookingHistoryHotelListVM);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An error occurred while retrieving your booking history.";
                    return View("Error");
                }
            }
            return View();
            
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Booking? booking = await _bookingService.FindByIdAsync(id.Value);
            if (booking == null)
            {
                return NotFound();
            }
            var bookingVM = _mapper.Map<BookingVM>(booking);
            return View(bookingVM);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                Booking? booking = await _bookingService.FindByIdAsync(Convert.ToInt32(id));
                if (booking == null)
                {
                    return NotFound();
                }
                await _bookingService.DeleteAsync(booking);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to delete data.");
                return View("Error");
            }
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
