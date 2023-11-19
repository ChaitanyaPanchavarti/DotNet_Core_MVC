using ASP.NET_Core_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC.Controllers
{
    public class EmailConfirmation : Controller
    {
        private readonly IEmailService _emailservice;

        public EmailConfirmation(IEmailService emailservice)
        {
            this._emailservice=emailservice;
            
        }

        
        public IActionResult SentEmail(string email,string link)
        {

            return (IActionResult)Email(email,link);
        }
        public async Task Email(string email, string link) => await _emailservice.SendEmailAsync(email, "Hello", $"Click here {link}");
    }
}
