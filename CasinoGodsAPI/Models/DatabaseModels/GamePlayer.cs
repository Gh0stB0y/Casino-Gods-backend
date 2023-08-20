namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class GamePlayerTable
    {
        public int Id { get; set; }
        public Games GameName { get; set; }
        public Player Player { get; set; }
        public int GamesPlayed { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Loses { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public float Winratio { get; set; } = 0;
        public float Profit { get; set; } = 0;

    }
}
