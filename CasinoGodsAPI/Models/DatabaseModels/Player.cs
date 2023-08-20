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
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = "";
        public int bankroll { get; set; } = 5000;
        public int profit { get; set; } = 0;
        public DateTime birthdate { get; set; }
        public byte[] passSalt { get; set; }
        public byte[] passHash { get; set; }

        public void hashPass(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                this.passSalt = hmac.Key;
                this.passHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public static string GetRandomPassword(int length)
        {
            byte[] rgb = new byte[length];
            RNGCryptoServiceProvider rngCrypt = new RNGCryptoServiceProvider();
            rngCrypt.GetBytes(rgb);
            return Convert.ToBase64String(rgb);
        }

        public static void sendRecoveryEmail(string email, string newPassword)
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
                claims.Add(new Claim(ClaimTypes.Name, this.username));
                claims.Add(new Claim(ClaimTypes.Role, "Guest"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Name, this.username));
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
