namespace CasinoGodsAPI.DTOs
{
    public class SignUpDTO
    {
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public DateTime birthdate { get; set; }
        public string password { get; set; } = string.Empty;


        public static string CheckSignUpCredentials(SignUpDTO p)
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

        private static bool ageGood(SignUpDTO p)
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
        private static bool passNumGood(SignUpDTO p)
        {
            int res;
            bool numExist = false;
            foreach (char character in p.password)
            {
                if (int.TryParse(character.ToString(), out res)) { numExist = true; break; }
            }
            return numExist;
        }
        private static bool specialLetterGood(SignUpDTO p)
        {
            return p.password.Any(ch => !char.IsLetterOrDigit(ch));
        }
        private static bool checkUppercase(SignUpDTO p)
        {
            bool upperExist = false;
            foreach (char character in p.password)
            {
                if (Char.IsUpper(character)) { upperExist = true; break; }
            }
            return upperExist;
        }
        private static bool checkLowecase(SignUpDTO p)
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
