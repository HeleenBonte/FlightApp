using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Models;
using FlightApp.Services.Interfaces;
using FlightApp.Util.Hotels.Interfaces;
using FlightApp.Util.Mail.Interfaces;
using FlightApp.Util.PDF.Interfaces;
using FlightApp.ViewModels;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace FlightApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly IEmailSend _emailSender;
        private readonly ICreatePDF _createPDF;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IService<Ticket> _ticketService;
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;


        public BookingController(IMapper mapper, IService<Ticket> ticketService, IEmailSend emailSender, IWebHostEnvironment webHostEnvironment, ICreatePDF createPDF, IHotelService hotelService)
        {
            _emailSender = emailSender;
            _createPDF = createPDF;
            _webHostEnvironment = webHostEnvironment;
            _ticketService = ticketService;
            _hotelService = hotelService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            
            
            var lstHotelIds = await _hotelService.GetHotelIdsAsync("-2601889");
            var lstHotelIdsVm = _mapper.Map<List<HotelIDVm>>(lstHotelIds);
            lstHotelIdsVm = lstHotelIdsVm.Slice(0,3);
            List<HotelVM> hotels = new List<HotelVM>();
            foreach(var hotelId in lstHotelIdsVm) {
                var hotel = await _hotelService.GetHotelByIdAsync(hotelId.Id);
                var hotelvm = _mapper.Map<HotelVM>(hotel);
                hotels.Add(hotelvm);
            }
                return View(hotels);
        }
        
        public async Task<IActionResult> ConfirmBooking()
        {
            
            var ticket = await _ticketService.FindByIdAsync(98);
            var pdfDoc = _createPDF.CreatePDFDocumentAsync(ticket);
            await _emailSender.SendEmailAsync(ticket.Passenger.Email, "Tickets", "Here are your tickets", pdfDoc);
            //return File(pdfDoc.ToArray(), "application/pdf", "ticket.pdf");
            return View("Index");
        }
    }
}
