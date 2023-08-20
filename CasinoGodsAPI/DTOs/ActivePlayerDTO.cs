namespace CasinoGodsAPI.DTOs
{
    public class ActivePlayerDTO
    {
        public string Username { get; set; } = string.Empty;
        public int Bankroll { get; set; } = 0;
        public int Profit { get; set; } = 0;
        public string Jwt { get; set; } = string.Empty;
    }
}
