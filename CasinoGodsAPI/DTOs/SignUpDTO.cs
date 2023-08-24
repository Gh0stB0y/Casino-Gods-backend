namespace CasinoGodsAPI.DTOs
{
    public class SignUpDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }
        public string Password { get; set; } = string.Empty;


        public static string CheckSignUpCredentials(SignUpDTO p)
        {
            string response = "";
            if (p.Password.Length < 8) response = "Password too short";
            if (!SpecialLetterGood(p)) response = "Password does not contain a special character";
            if (!PassNumGood(p)) response = "Password does not contain a number";
            if (!CheckLowecase(p)) response = "Password does not contain lowercase letter";
            if (!CheckUppercase(p)) response = "Password does not contain upercase letter";

            if (!AgeGood(p)) response = "User is not an adult";
            if (!p.Email.Contains('@')) response = "Invalid email";

            if (p.Username.Length < 4) response = "Username too short";

            return response;
        }
        private static bool AgeGood(SignUpDTO p)
        {
            if (p.Birthdate.Year + 18 < DateTime.Today.Year) return true;
            else if (p.Birthdate.Year + 18 == DateTime.Today.Year)
            {
                if (p.Birthdate.Month < DateTime.Today.Month) return true;
                else if (p.Birthdate.Month == DateTime.Today.Month)
                {
                    if (p.Birthdate.Day <= DateTime.Today.Day) return true;
                    else return false;
                }
                else return false;
            }
            else return false;
        }
        private static bool PassNumGood(SignUpDTO p)
        {
            
            int res;
            bool numExist = false;
            foreach (char character in p.Password)
            {
                //if (int.TryParse(character.ToString(), out res)) { numExist = true; break; }
                if (char.IsDigit(character)) { numExist = true; break; }
            }
            return numExist;
        }
        private static bool SpecialLetterGood(SignUpDTO p)
        {
            return p.Password.Any(ch => !char.IsLetterOrDigit(ch));
        }
        private static bool CheckUppercase(SignUpDTO p)
        {
            bool upperExist = false;
            foreach (char character in p.Password)
            {
                if (char.IsUpper(character)) { upperExist = true; break; }
            }
            return upperExist;
        }
        private static bool CheckLowecase(SignUpDTO p)
        {
            bool lowerExist = false;
            foreach (char character in p.Password)
            {
                if (char.IsLower(character)) { lowerExist = true; break; }
            }
            return lowerExist;
        }
    }
}
