using Microsoft.VisualBasic;
using System;
using System.Data.SqlTypes;

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
    }


}
