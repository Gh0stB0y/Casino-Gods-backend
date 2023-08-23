using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class ActiveTablesDB
    {
        private ActiveTablesDB parentTable;
        private Guid guid;

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
        public void AddProperties(Tables table, Guid guid)
        {
            TableInstanceId = Guid.NewGuid();
            Name = table.CKname;
            Game = table.CKGame;
            MinBet = table.MinBet;
            MaxBet = table.MaxBet;
            BetTime = table.BetTime;
            Maxseats = table.Maxseats;
            ActionTime = table.ActionTime;
            Sidebet1 = table.Sidebet1;
            Sidebet2 = table.Sidebet2;
            Decks = table.Decks;
        }

        public void AddProperties(ActiveTablesDB parentTable, Guid guid)
        {
            TableInstanceId = guid;
            Name = parentTable.Name;
            Game = parentTable.Game;
            MinBet = parentTable.MinBet;
            MaxBet = parentTable.MaxBet;
            BetTime = parentTable.BetTime;
            Maxseats = parentTable.Maxseats;
            ActionTime = parentTable.ActionTime;
            Sidebet1 = parentTable.Sidebet1;
            Sidebet2 = parentTable.Sidebet2;
            Decks = parentTable.Decks;
        }
    }
}
