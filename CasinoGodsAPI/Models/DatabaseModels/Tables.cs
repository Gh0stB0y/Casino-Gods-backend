using StackExchange.Redis;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models.DatabaseModels
{
    public class Tables
    {
        [Key, Column(Order = 0)]
        public string CKname { get; set; }
        [Key, Column(Order = 1)]
        public string CKGame { get; set; }
        public Games Game { get; set; }
        public int minBet { get; set; } = 0;
        public int maxBet { get; set; } = 100000;
        public int betTime { get; set; } = 30;
        public int maxseats { get; set; } = 5;
        public int actionTime { get; set; } = 15;
        public bool sidebet1 { get; set; } = true;
        public bool sidebet2 { get; set; } = true;
        public int decks { get; set; } = 6;

        public ICollection<LobbyTableData> ActiveTables { get; set; }
        public Tables()
        {
            ActiveTables = new HashSet<LobbyTableData>();
        }


    }
}
