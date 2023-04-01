using MailKit.Security;
using MimeKit;
using System.Security.Cryptography;
using MailKit.Net.Smtp;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

using System.ComponentModel.DataAnnotations;


namespace CasinoGodsAPI.Models
{
    public class Player
    {
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
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uname),
                new Claim(ClaimTypes.Role, "Player"),
            };
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


    public class PlayerSignUp
    {
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public DateTime birthdate { get; set; }
        public string password { get; set; } = string.Empty;


        public static string CheckSignUpCredentials(PlayerSignUp p)
        {
            string response = "";
            if (p.password.Length < 8) response = ("Password too short");
            if (!specialLetterGood(p)) response = ("Password does not contain a special character");
            if (!passNumGood(p)) response = ("Password does not contain a number");
            if (!checkLowecase(p)) response = ("Password does not contain lowercase letter");
            if (!checkUppercase(p)) response = ("Password does not contain upercase letter");

            if (!ageGood(p)) response = ("User is not an adult");
            if (!p.email.Contains('@')) response = ("Invalid email");

            if (p.username.Length < 4) response = ("Username too short");

            return response;
        }

        private static bool ageGood(PlayerSignUp p)
        {
            if ((p.birthdate.Year + 18) < DateTime.Today.Year) return true;
            else if ((p.birthdate.Year + 18) == DateTime.Today.Year)
            {
                if (p.birthdate.Month < DateTime.Today.Month) return true;
                else if (p.birthdate.Month == DateTime.Today.Month)
                {
                    if (p.birthdate.Day <= DateTime.Today.Day) return true;
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        private static bool passNumGood(PlayerSignUp p)
        {
            int res;
            bool numExist = false;
            foreach (char character in p.password)
            {
                if (int.TryParse(character.ToString(), out res)) { numExist = true; break; }
            }
            return numExist;
        }
        private static bool specialLetterGood(PlayerSignUp p)
        {
            return p.password.Any(ch => !char.IsLetterOrDigit(ch));
        }
        private static bool checkUppercase(PlayerSignUp p)
        {
            bool upperExist = false;
            foreach (char character in p.password)
            {
                if (Char.IsUpper(character)) { upperExist = true; break; }
            }
            return upperExist;
        }
        private static bool checkLowecase(PlayerSignUp p)
        {
            bool lowerExist = false;
            foreach (char character in p.password)
            {
                if (Char.IsLower(character)) { lowerExist = true; break; }
            }
            return lowerExist;
        }
    }

    public class PlayerSignIn
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;

    }
    public class JwtClass
    {
        public string jwtString { get; set; } = string.Empty;
    }
    public class EmailRecovery
    {
        public string emailRec { get; set; } = string.Empty;
    }
    public class ActivePlayerDTO
    {
        public string username { get; set; } = string.Empty;
        public int bankroll { get; set; } = 0;
        public int profit { get; set; } = 0;
        public string jwt { get; set; } = string.Empty;
    }
}