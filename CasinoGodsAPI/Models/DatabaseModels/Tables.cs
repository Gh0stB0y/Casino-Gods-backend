using StackExchange.Redis;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class Tables
    {
        [Key, Column(Order = 0)]
        public string CKname { get; set; }
        [Key, Column(Order = 1)]
        public string CKGame { get; set; }
        public Games Game { get; set; }
        public int MinBet { get; set; } = 0;
        public int MaxBet { get; set; } = 100000;
        public int BetTime { get; set; } = 30;
        public int Maxseats { get; set; } = 5;
        public int ActionTime { get; set; } = 15;
        public bool Sidebet1 { get; set; } = true;
        public bool Sidebet2 { get; set; } = true;
        public int Decks { get; set; } = 6;

        public ICollection<LobbyTableData> ActiveTables { get; set; }
        public Tables()
        {
            ActiveTables = new HashSet<LobbyTableData>();
        }
    }
}
