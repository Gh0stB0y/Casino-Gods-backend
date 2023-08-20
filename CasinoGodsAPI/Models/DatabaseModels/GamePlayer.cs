namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class GamePlayerTable
    {
        public int Id { get; set; }
        public Games gameName { get; set; }
        public Player player { get; set; }
        public int gamesPlayed { get; set; } = 0;
        public int wins { get; set; } = 0;
        public int loses { get; set; } = 0;
        public int draws { get; set; } = 0;
        public float winratio { get; set; } = 0;
        public float profit { get; set; } = 0;

    }
}
