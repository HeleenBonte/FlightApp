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

            // Add passenger information section with more details
            message.AppendLine("    <div class=\"passenger-info\">");
            message.AppendLine("      <h3 class=\"standard-text\">Passenger Information</h3>");
            message.AppendLine($"      <p><strong>Name:</strong> {passenger.FirstName} {passenger.LastName}</p>");
            message.AppendLine($"      <p><strong>Date of Birth:</strong> {passenger.Birthdate.ToString("dd/MM/yyyy")}</p>");

            message.AppendLine("    </div>");

            message.AppendLine("    <div class=\"info\">");
            message.AppendLine($"      <p><strong>Route:</strong> {routeDescription}</p>");
            message.AppendLine("    </div>");
            message.AppendLine("    <h3 class=\"standard-text\">Your Flight Details</h3>"); // Using standard text color

            // Add mobile ticket cards
            message.AppendLine("    <div class=\"mobile-tickets\">");
            foreach (var ticket in fullTickets.OrderBy(t => t.Flight.DepartureTime))
            {
                string passengerName = $"{ticket.Passenger.FirstName} {ticket.Passenger.LastName}";
                string flightFrom = ticket.Flight.DepartureCityNavigation.CityName;
                string flightTo = ticket.Flight.ArrivalCityNavigation.CityName;
                string departureTime = ticket.Flight.DepartureTime?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                string bookingClass = ticket.BookingClass.Description;
                string mealChoice = ticket.MealChoice?.Type ?? "Standard";

                message.AppendLine("      <div class=\"ticket-card\">");
                message.AppendLine($"        <div class=\"ticket-header\">{flightFrom} to {flightTo}</div>");
                message.AppendLine("        <div class=\"ticket-details\">");
                message.AppendLine($"          <p><strong>Passenger:</strong> {passengerName}</p>");
                message.AppendLine($"          <p><strong>Route:</strong> {ticket.Flight.FlightId}</p>");
                message.AppendLine($"          <p><strong>From:</strong> {flightFrom}</p>");
                message.AppendLine($"          <p><strong>To:</strong> {flightTo}</p>");
                message.AppendLine($"          <p><strong>Departure:</strong> {departureTime}</p>");
                message.AppendLine($"          <p><strong>Booking Class:</strong> {bookingClass}</p>");
                message.AppendLine($"          <p><strong>Meal:</strong> {mealChoice}</p>");
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
        /// Generates the HTML table with ticket details
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
            tableHtml.AppendLine("          <th>Meal</th>");
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
                string mealChoice = ticket.MealChoice?.Type ?? "Standard";

                tableHtml.AppendLine("        <tr>");
                tableHtml.AppendLine($"          <td>{passengerName}</td>");
                tableHtml.AppendLine($"          <td>{ticket.Flight.FlightId}</td>");
                tableHtml.AppendLine($"          <td>{flightFrom}</td>");
                tableHtml.AppendLine($"          <td>{flightTo}</td>");
                tableHtml.AppendLine($"          <td>{departureTime}</td>");
                tableHtml.AppendLine($"          <td>{bookingClass}</td>");
                tableHtml.AppendLine($"          <td>{mealChoice}</td>");
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
      color: #e0e0e0; 
      max-width: 100%;
      width: 100% !important;
      margin: 0 auto; 
      padding: 0;
      background-color: #2d2d2d;
    }
    .container {
      max-width: 600px;
      margin: 0 auto;
      width: 100%;
      background-color: #2d2d2d;
    }
    .header { 
      background-color: #3a7bd5; 
      color: white; 
      padding: 20px; 
      text-align: center; 
    }
    .content { 
      padding: 20px; 
      color: #e0e0e0;
      background-color: #2d2d2d;
    }
    .footer { 
      background-color: #222222; 
      padding: 15px; 
      text-align: center; 
      font-size: 14px; 
      color: #a0a0a0; 
    }
    h1 { color: white; }
    h2 { color: #7ce795; margin-top: 20px; } /* Brighter green for thanks message */
    h3 { 
      color: #e0e0e0; 
      margin-top: 15px; 
      font-size: 18px;
      font-weight: bold;
    }
    .standard-text { 
      color: #e0e0e0; 
      font-size: 18px; 
      font-weight: bold; 
      margin-top: 25px; 
      margin-bottom: 15px; 
    }
    
    /* Table styles for desktop */
    table { 
      width: 100%; 
      border-collapse: collapse; 
      margin: 20px 0; 
      background-color: #333333;
    }
    th { 
      background-color: #3a7bd5; 
      color: white; 
      text-align: left; 
      padding: 10px; 
    }
    td { 
      padding: 8px; 
      border-bottom: 1px solid #555555; 
      color: #e0e0e0;
    }
    
    /* Mobile ticket cards */
    .mobile-tickets {
      display: none;
    }
    
    .ticket-card {
      border: 1px solid #555555;
      border-radius: 8px;
      margin-bottom: 15px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.3);
      overflow: hidden;
      background-color: #333333;
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
      background-color: #333333;
      color: #e0e0e0;
    }
    
    .ticket-details p {
      margin: 5px 0;
      color: #e0e0e0;
    }
    
    /* Info boxes */
    .info { 
      background-color: #333333; 
      padding: 15px; 
      border-radius: 5px; 
      margin: 15px 0; 
      color: #e0e0e0;
      border: 1px solid #444444;
    }
    
    .passenger-info { 
      background-color: #303c49; 
      padding: 15px; 
      border-radius: 5px; 
      margin: 15px 0; 
      border-left: 4px solid #4a79b5; 
      color: #e0e0e0;
    }
    
    /* Make all text within these sections the same color */
    .passenger-info p, .info p, .content p, .ticket-details p {
      color: #e0e0e0;
    }
    
    /* Make sure all strong tags have the same color */
    strong {
      color: #80c4ff;
      font-weight: bold;
    }
    
    /* Links style */
    a {
      color: #61a8ff;
      text-decoration: underline;
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
