using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class ActiveTablesDB
    {
        [Key]
        public Guid TableInstanceId { get; set; } = new Guid();

        public string Name { get; set; }
        public string Game { get; set; }
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;
        public int maxseats { get; set; } = 5;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;

    }
}
