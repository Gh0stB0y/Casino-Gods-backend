using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace CasinoGodsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        [HttpPost]
        public IActionResult sendRecoveryEmail(string email,string newPassword)
        {
            var recEmail = new MimeMessage();
            recEmail.From.Add(MailboxAddress.Parse("kapi38134@wp.pl"));
            recEmail.To.Add(MailboxAddress.Parse("kacper.a.przybylski@gmail.com"));
            recEmail.Subject = "Password recovery";
            recEmail.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = ""};
            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.wp.pl", 465 , SecureSocketOptions.SslOnConnect);
                smtp.Authenticate("kapi38134@wp.pl", "dorsz1");
                smtp.Send(recEmail);
                smtp.Disconnect(true);
            }
            return Ok();
        }

    }
}
