using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MimeKit;
using System;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;

namespace CasinoGodsAPI.Models
{
    public class Player
    {
        public Guid Id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int bankroll { get; set; }
        public int profit { get; set; }
        public DateTime birthdate { get; set; }


        public static string CheckSignUpCredentials(Player p)
        {
            string response = "";
            if (p.password.Length < 8) response = ("Password too short");
            if (!specialLetterGood(p)) response = ("Password does not contain a special character");
            if (!passNumGood(p)) response = ("Password does not contain a number");
            if (!checkLowecase(p)) response = ("Password does not contain lowercase letter");
            if (!checkUppercase(p)) response = ("Password does not contain upercase letter");

            if (!ageGood(p)) response = ("User is not an adult");
            if (!p.email.Contains('@')) response = ("Invalid email");

            if (p.username.Length<4) response = ("Username too short");
        
            return response;
        }
        private static bool ageGood(Player p)
        {
            if ((p.birthdate.Year + 18) < DateTime.Today.Year) return true;
            else if ((p.birthdate.Year + 18) == DateTime.Today.Year) {
                if(p.birthdate.Month< DateTime.Today.Month) return true;
                else if(p.birthdate.Month == DateTime.Today.Month)
                {
                    if(p.birthdate.Day <= DateTime.Today.Day) return true;
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        private static bool passNumGood(Player p)
        {
            int res;
            bool numExist=false;
            foreach (char character in p.password)
            { 
               if( int.TryParse(character.ToString(),out res)) { numExist = true; break; }  
            }
            return numExist;
        }
        private static bool specialLetterGood(Player p)
        {
            return p.password.Any(ch => !char.IsLetterOrDigit(ch));
        }
        private static bool checkUppercase(Player p)
        {
            bool upperExist = false;
            foreach(char character in p.password)
            {
                if (Char.IsUpper(character)) { upperExist = true; break; }
            }
            return upperExist;
        }
        private static bool checkLowecase(Player p)
        {
            bool lowerExist = false;
            foreach (char character in p.password)
            {
                if (Char.IsLower(character)) { lowerExist = true; break; }
            }
            return lowerExist;
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
            recEmail.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = "Witaj,\nTwoje haslo tymczasowe to: "+ newPassword +".\nPozdrawiamy,\nZespol Casino Gods"};
            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.wp.pl", 465, SecureSocketOptions.SslOnConnect);
                smtp.Authenticate("kapi38134@wp.pl", "dorsz1");
                smtp.Send(recEmail);
                smtp.Disconnect(true);
            }
        }
    }


}
