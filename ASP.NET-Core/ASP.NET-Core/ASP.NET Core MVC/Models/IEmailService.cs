using System.Threading.Tasks;

namespace ASP.NET_Core_MVC.Models
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
