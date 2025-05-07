using FlightApp.Util.Mail.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace FlightApp.Util.Mail
{
    public class EmailSend : IEmailSend
    {
        private readonly EmailSettings _emailSettings;
        public EmailSend(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailAsync(
        string email, string subject, string message, Stream stream)
        {
            var mail = new MailMessage(); // aanmaken van een mail-object
            mail.To.Add(new MailAddress(email));
            mail.From = new MailAddress("bonteheleen@gmail.com"); // hier komt jullie Gmail-adres
            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = true;
            var attachment = new Attachment(stream, "Ticket.pdf");
            mail.Attachments.Add(attachment);
            try
            {
                using (var smtp = new SmtpClient(_emailSettings.MailServer))
                {
                    smtp.Port = _emailSettings.MailPort;
                    smtp.EnableSsl = true;
                    smtp.Credentials =
                    new NetworkCredential(_emailSettings.Sender,
                    _emailSettings.Password);
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            { throw ex; }
        }
    }
}
