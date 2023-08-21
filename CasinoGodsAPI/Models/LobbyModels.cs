using CasinoGodsAPI.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace CasinoGodsAPI.Models
{

    public class LobbyTableData
    {
        [Key]
        public Guid TableInstanceId { get; set; } = new Guid();
        public string TablePath { get; set; }=string.Empty;
        public Tables TableType { get; set; }

    }
    
    public class PlayerLobbyData
    {
        public string connectionID { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string jwt { get; set; } =string.Empty;
        public int bankroll { get; set; } = 0;
    }
    
}
