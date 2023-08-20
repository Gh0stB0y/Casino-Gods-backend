using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class Player 
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = "";
        public int Bankroll { get; set; } = 5000;
        public int Profit { get; set; } = 0;
        public DateTime Birthdate { get; set; }
        public byte[] PassSalt { get; set; }
        public byte[] PassHash { get; set; }

        public void HashPass(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                this.PassSalt = hmac.Key;
                this.PassHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public static string GetRandomPassword(int length)
        {
            byte[] rgb = new byte[length];
            RNGCryptoServiceProvider rngCrypt = new RNGCryptoServiceProvider();
            rngCrypt.GetBytes(rgb);
            return Convert.ToBase64String(rgb);
        }

        public static void SendRecoveryEmail(string email, string newPassword)
        {
            var recEmail = new MimeMessage();
            recEmail.From.Add(MailboxAddress.Parse("kapi38134@wp.pl"));
            recEmail.To.Add(MailboxAddress.Parse("kacper.a.przybylski@gmail.com"));
            recEmail.Subject = "Password recovery";
            recEmail.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = "<p>Witaj,</p><p>Twoje haslo tymczasowe to: " + newPassword + ".</p><p>Pozdrawiamy,</p><p>Zespol Casino Gods</p>" };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.wp.pl", 465, SecureSocketOptions.SslOnConnect);
                smtp.Authenticate("kapi38134@wp.pl", "dorsz1");
                smtp.Send(recEmail);
                smtp.Disconnect(true);
            }
        }
        public string CreateToken(string uname, IConfiguration configuration)
        {
            List<Claim> claims = new List<Claim>();
            if (uname == "guest")
            {
                claims.Add(new Claim(ClaimTypes.Name, this.Username));
                claims.Add(new Claim(ClaimTypes.Role, "Guest"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Name, this.Username));
                claims.Add(new Claim(ClaimTypes.Role, "Player"));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            //string jsonString = JsonSerializer.Serialize(jwt);

            return jwt;
        }
    }
}
