using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Util.Mail.Interfaces
{
    public interface IEmailSend
    {
        Task SendEmailAsync(string email, string subject, string message, Stream stream);
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailWithAttachmentsAsync(string email, string subject, string message, List<(string fileName, byte[] content, string contentType)> attachments);

    }
}
