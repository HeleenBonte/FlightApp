using FlightApp.Domains.EntitiesDB;
using System.Text;

namespace FlightApp.Util.Email
{
    public class EmailTemplateService
    {
        /// <summary>
        /// Generates the HTML for a booking confirmation email
        /// </summary>
        /// <param name="passenger">The passenger receiving the email</param>
        /// <param name="routeDescription">Description of the route</param>
        /// <param name="fullTickets">List of tickets for the passenger</param>
        /// <returns>HTML content for the email</returns>
        public static string GenerateBookingConfirmationEmail(Passenger passenger, string routeDescription, List<Ticket> fullTickets)
        {
            var message = new StringBuilder();

            // Add HTML header and styles
            message.Append(GetEmailHeader());

            // Add email content
            message.AppendLine("  <div class=\"content\">");
            message.AppendLine($"    <h2>Thanks for booking with Zephyrus Airlines!</h2>");
            message.AppendLine($"    <p>Dear {passenger.FirstName} {passenger.LastName},</p>");
            message.AppendLine($"    <p>Your booking has been confirmed. Below you will find a summary of your tickets. All details are also attached as PDF documents.</p>");

            // Add passenger information section
            message.AppendLine("    <div class=\"passenger-info\">");
            message.AppendLine("      <h3>Passenger Information</h3>");
            message.AppendLine($"      <p><strong>Name:</strong> {passenger.FirstName} {passenger.LastName}</p>");
            message.AppendLine($"      <p><strong>Email:</strong> {passenger.Email}</p>");
            message.AppendLine("    </div>");

            message.AppendLine("    <div class=\"info\">");
            message.AppendLine($"      <p><strong>Route:</strong> {routeDescription}</p>");
            message.AppendLine("    </div>");
            message.AppendLine("    <h2>Your Flight Details</h2>");

            // Add ticket cards for mobile
            message.AppendLine("    <div class=\"mobile-tickets\">");
            foreach (var ticket in fullTickets.OrderBy(t => t.Flight.DepartureTime))
            {
                string passengerName = $"{ticket.Passenger.FirstName} {ticket.Passenger.LastName}";
                string flightFrom = ticket.Flight.DepartureCityNavigation.CityName;
                string flightTo = ticket.Flight.ArrivalCityNavigation.CityName;
                string departureTime = ticket.Flight.DepartureTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                string bookingClass = ticket.BookingClass.Description;

                message.AppendLine("      <div class=\"ticket-card\">");
                message.AppendLine($"        <div class=\"ticket-header\">{flightFrom} to {flightTo}</div>");
                message.AppendLine("        <div class=\"ticket-details\">");
                message.AppendLine($"          <p><strong>Passenger:</strong> {passengerName}</p>");
                message.AppendLine($"          <p><strong>Route:</strong> {ticket.Flight.FlightId}</p>");
                message.AppendLine($"          <p><strong>Departure:</strong> {departureTime}</p>");
                message.AppendLine($"          <p><strong>Class:</strong> {bookingClass}</p>");
                message.AppendLine("        </div>");
                message.AppendLine("      </div>");
            }
            message.AppendLine("    </div>");

            // Add standard table for desktop
            message.AppendLine("    <div class=\"desktop-table\">");
            message.Append(GenerateTicketTable(fullTickets));
            message.AppendLine("    </div>");

            message.AppendLine("    <p>You will find all your tickets in the attached PDF files.</p>");
            message.AppendLine("    <p>We wish you a pleasant flight!</p>");
            message.AppendLine("    <p>Best regards,<br>The Zephyrus Airlines Team</p>");
            message.AppendLine("  </div>");

            // Add footer and close HTML tags
            message.Append(GetEmailFooter());

            return message.ToString();
        }

        /// <summary>
        /// Generates the HTML table with ticket details (for desktop view)
        /// </summary>
        private static string GenerateTicketTable(List<Ticket> tickets)
        {
            var tableHtml = new StringBuilder();

            tableHtml.AppendLine("    <table>");
            tableHtml.AppendLine("      <thead>");
            tableHtml.AppendLine("        <tr>");
            tableHtml.AppendLine("          <th>Passenger</th>");
            tableHtml.AppendLine("          <th>Route</th>");
            tableHtml.AppendLine("          <th>From</th>");
            tableHtml.AppendLine("          <th>To</th>");
            tableHtml.AppendLine("          <th>Departure</th>");
            tableHtml.AppendLine("          <th>Booking Class</th>");
            tableHtml.AppendLine("        </tr>");
            tableHtml.AppendLine("      </thead>");
            tableHtml.AppendLine("      <tbody>");

            foreach (var ticket in tickets.OrderBy(t => t.Flight.DepartureTime))
            {
                string passengerName = $"{ticket.Passenger.FirstName} {ticket.Passenger.LastName}";
                string flightFrom = ticket.Flight.DepartureCityNavigation.CityName;
                string flightTo = ticket.Flight.ArrivalCityNavigation.CityName;
                string departureTime = ticket.Flight.DepartureTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                string bookingClass = ticket.BookingClass.Description;

                tableHtml.AppendLine("        <tr>");
                tableHtml.AppendLine($"          <td>{passengerName}</td>");
                tableHtml.AppendLine($"          <td>{ticket.Flight.FlightId}</td>");
                tableHtml.AppendLine($"          <td>{flightFrom}</td>");
                tableHtml.AppendLine($"          <td>{flightTo}</td>");
                tableHtml.AppendLine($"          <td>{departureTime}</td>");
                tableHtml.AppendLine($"          <td>{bookingClass}</td>");
                tableHtml.AppendLine("        </tr>");
            }

            tableHtml.AppendLine("      </tbody>");
            tableHtml.AppendLine("    </table>");

            return tableHtml.ToString();
        }

        /// <summary>
        /// Returns the HTML header with CSS styles
        /// </summary>
        private static string GetEmailHeader()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    /* Base styles */
    body { 
      font-family: Arial, sans-serif; 
      line-height: 1.6; 
      color: #333; 
      max-width: 100%;
      width: 100% !important;
      margin: 0 auto; 
      padding: 0;
    }
    .container {
      max-width: 600px;
      margin: 0 auto;
      width: 100%;
    }
    .header { 
      background-color: #3a7bd5; 
      color: white; 
      padding: 20px; 
      text-align: center; 
    }
    .content { 
      padding: 20px; 
    }
    .footer { 
      background-color: #f4f4f4; 
      padding: 15px; 
      text-align: center; 
      font-size: 14px; 
      color: #666; 
    }
    h1 { color: white; }
    h2 { color: #2ecc71; margin-top: 20px; }
    h3 { color: #2980b9; margin-top: 15px; }
    
    /* Table styles for desktop */
    table { 
      width: 100%; 
      border-collapse: collapse; 
      margin: 20px 0; 
    }
    th { 
      background-color: #3a7bd5; 
      color: white; 
      text-align: left; 
      padding: 10px; 
    }
    td { 
      padding: 8px; 
      border-bottom: 1px solid #ddd; 
    }
    
    /* Mobile ticket cards */
    .mobile-tickets {
      display: none;
    }
    
    .ticket-card {
      border: 1px solid #ddd;
      border-radius: 8px;
      margin-bottom: 15px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      overflow: hidden;
    }
    
    .ticket-header {
      background-color: #3a7bd5;
      color: white;
      padding: 10px;
      font-weight: bold;
      font-size: 16px;
    }
    
    .ticket-details {
      padding: 12px;
      background-color: #f9f9f9;
    }
    
    .ticket-details p {
      margin: 5px 0;
    }
    
    /* Info boxes */
    .info { 
      background-color: #f9f9f9; 
      padding: 15px; 
      border-radius: 5px; 
      margin: 15px 0; 
    }
    
    .passenger-info { 
      background-color: #e8f4fc; 
      padding: 15px; 
      border-radius: 5px; 
      margin: 15px 0; 
      border-left: 4px solid #3498db; 
    }
    
    /* Responsive styles for mobile devices */
    @media screen and (max-width: 600px) {
      .desktop-table {
        display: none;
      }
      
      .mobile-tickets {
        display: block;
      }
      
      .content {
        padding: 15px 10px;
      }
      
      .passenger-info, .info {
        padding: 12px;
      }
    }
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">
      <h1>Zephyrus Airlines</h1>
    </div>
";
        }

        /// <summary>
        /// Returns the HTML footer
        /// </summary>
        private static string GetEmailFooter()
        {
            return @"    <div class=""footer"">
      <p>This is an automated message. Please do not reply to this email.</p>
      <p>&copy; 2025 Zephyrus Airlines. All rights reserved.</p>
    </div>
  </div>
</body>
</html>";
        }
    }
}
