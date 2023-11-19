using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC.Models
{
    public class EmailService:IEmailService
    {
        Task IEmailService.SendEmailAsync(string email, string subject, string Message)
        {
            var mail = "panchavarthichaitanya@gmail.com";
            var pswrd = "Rischai@270502";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pswrd)
            };
            return client.SendMailAsync(new MailMessage(from: mail,
                to: mail, subject: subject, Message));
        }
    }
}
