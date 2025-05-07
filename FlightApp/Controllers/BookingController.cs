using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
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

        public BookingController(IService<Ticket> ticketService, IEmailSend emailSender, IWebHostEnvironment webHostEnvironment, ICreatePDF createPDF)
        {
            _emailSender = emailSender;
            _createPDF = createPDF;
            _webHostEnvironment = webHostEnvironment;
            _ticketService = ticketService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> ConfirmBooking()
        {
            
            var ticket = await _ticketService.FindByIdAsync(1);
            var pdfDoc = _createPDF.CreatePDFDocumentAsync(ticket);
            await _emailSender.SendEmailAsync("bonteheleen@hotmail.com", "Tickets", "Here are your tickets", pdfDoc);
            //return File(pdfDoc.ToArray(), "application/pdf", "ticket.pdf");
            return View("Index");
        }
    }
}
