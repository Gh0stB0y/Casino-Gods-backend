namespace CasinoGodsAPI.Models
{
   

    public class PlayerLobbyData
    {
        public string connectionID { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string jwt { get; set; } =string.Empty;
        public int bankroll { get; set; } = 0;
    }
}
