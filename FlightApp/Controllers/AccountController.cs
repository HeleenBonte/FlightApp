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
        private readonly ITicketService _ticketService;
        public AccountController(UserManager<ApplicationUser> userManager, IBookingHistoryService bookingHistoryService, IMapper mapper, IHotelService hotelService, IService<Booking> bookingService, ITicketService ticketService)
        {
            _userManager = userManager;
            _bookingHistoryService = bookingHistoryService;
            _mapper = mapper;
            _hotelService = hotelService;
            _bookingService = bookingService;
            _ticketService = ticketService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                try { 
                var history = await _bookingHistoryService.GetAllByUserIdAsync(userId);
                List<BookingHistoryVM> bookingHistoryVMs = _mapper.Map<List<BookingHistoryVM>>(history);
                    if (!bookingHistoryVMs.Any())
                    {
                        BookingHistoryHotelListVM emptyModel = new BookingHistoryHotelListVM
                        {
                            BookingHistory = new List<BookingHistoryVM>(),
                            Hotels = new List<HotelVM>()
                        };
                        return View(emptyModel);
                    }

                    var firstBookingWithCity = bookingHistoryVMs.FirstOrDefault(b => b.ArrivalCityData?.ApiId != null);
                    List<HotelVM> hotels = new List<HotelVM>();

                    if (firstBookingWithCity != null)
                    {
                        hotels = await GetHotelVMList(firstBookingWithCity.ArrivalCityData.ApiId.ToString(), DateOnly.FromDateTime(DateTime.Now));
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
                var tickets = await _ticketService.GetTicketsByBookingIdAsync(Convert.ToInt32(id));
                if (tickets == null)
                {
                    return NotFound();
                }
                foreach (var ticket in tickets)
                {
                    await _ticketService.DeleteAsync(ticket);
                }
                Booking? booking = await _bookingService.FindByIdAsync(Convert.ToInt32(id));
                if (booking == null)
                {
                    return NotFound();
                }
                booking.PaymentStatus = false;
                await _bookingService.UpdateAsync(booking);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to delete data.");
                return View("Error");
            }
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
