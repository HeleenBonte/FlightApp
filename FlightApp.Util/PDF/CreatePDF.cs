using FlightApp.Domains.EntitiesDB;
using FlightApp.Services.Interfaces;
using FlightApp.Util.PDF.Interfaces;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Util.PDF
{
    public class CreatePDF : ICreatePDF
    { 
        public MemoryStream CreatePDFDocumentAsync(Ticket ticket)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                iText.Layout.Document document = new iText.Layout.Document(pdf);

                iText.Layout.Element.Image logo = new iText.Layout.Element.Image(ImageDataFactory.Create("wwwroot/images/ZephyrusLogo.jpg")).ScaleToFit(50,50);
                logo.SetHorizontalAlignment(HorizontalAlignment.LEFT);
                document.Add(logo);
                string companyName = "Zephyrus Airlines";


                document.Add(new Paragraph(companyName)
                    .SetFontSize(20)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.BLUE)
                    .SetMarginBottom(20));

                if (ticket == null)
                {
                    throw new Exception("Ticket not found");
                }
                var namePassenger = ticket.Passenger.FirstName + " " + ticket.Passenger.LastName;
                var nameFlight = ticket.Flight.DepartureCityNavigation.CityName + " - " + ticket.Flight.ArrivalCityNavigation.CityName;
                var dateFlight = ticket.Flight.DepartureTime.ToString();
                var seatNumber = ticket.SeatNumber;
                var bookingClass = ticket.BookingClass.Description;
                var mealChoice = ticket.MealChoice.Type;

                var qrDescription = $"Passenger: {namePassenger}\nFlight: {nameFlight}\nDate: {dateFlight}\nSeat Number: {seatNumber}\nBooking Class: {bookingClass}\nMeal Choice: {mealChoice}";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrDescription, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                iText.Layout.Element.Image qrCodeImageElement = new iText.Layout.Element.Image(ImageDataFactory.Create(BitmapToBytes(qrCodeImage)))
                    .SetWidth(100)
                    .SetHeight(100)
                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                document.Add(qrCodeImageElement);

                iText.Layout.Element.Paragraph paragraph = new iText.Layout.Element.Paragraph("Ticket Confirmation")
                    .SetFontSize(20)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(paragraph);

                iText.Layout.Element.Paragraph passengerInfo = new iText.Layout.Element.Paragraph($"Passenger: {namePassenger}")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(passengerInfo);

                iText.Layout.Element.Paragraph flightInfo = new iText.Layout.Element.Paragraph($"Flight: {nameFlight}")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(flightInfo);

                iText.Layout.Element.Paragraph dateInfo = new iText.Layout.Element.Paragraph($"Date: {dateFlight}")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(dateInfo);

                iText.Layout.Element.Paragraph seatInfo = new iText.Layout.Element.Paragraph($"Seat Number: {seatNumber}")
                        .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(seatInfo);

                iText.Layout.Element.Paragraph bookingClassInfo = new iText.Layout.Element.Paragraph($"Booking Class: {bookingClass}")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(bookingClassInfo);

                iText.Layout.Element.Paragraph mealChoiceInfo = new iText.Layout.Element.Paragraph($"Meal Choice: {mealChoice}")
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT);
                document.Add(mealChoiceInfo);




                document.Close();
                pdf.Close();
                writer.Close();
                return new MemoryStream(stream.ToArray());
            }
        }
        private static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
