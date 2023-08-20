using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class ActiveTablesDB
    {
        [Key]
        public Guid TableInstanceId { get; set; } = new Guid();

        public string Name { get; set; }
        public string Game { get; set; }
        public int MinBet { get; set; } = 0;
        public int MaxBet { get; set; } = 100000;
        public int BetTime { get; set; } = 30;
        public int Maxseats { get; set; } = 5;
        public int ActionTime { get; set; } = 15;
        public bool Sidebet1 { get; set; } = true;
        public bool Sidebet2 { get; set; } = true;
        public int Decks { get; set; } = 6;

    }
}
