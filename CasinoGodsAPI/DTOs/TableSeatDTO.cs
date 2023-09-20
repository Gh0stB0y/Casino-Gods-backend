using CasinoGodsAPI.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CasinoGodsAPI.DTOs
{
    public class TableSeatDTO
    {
        public int Id { get; set; } = 0;
        public string TransformVal { get; set; } = string.Empty;
        public string Player { get; set; } = string.Empty;
        public bool Ready { get;set; } = false;
        public TableSeatDTO(int seatId,PlayingTable table)
        {
            Id = seatId;
            TransformVal = "";

            try{Player = table.UsersAtSeat[seatId];}
            catch { Player = ""; }

            if (Player == "")
            {
                Ready = false;
            }
            else
            {
                table.AllBetsPlaced.TryGetValue(table.UsersAtSeat[seatId], out bool isBetPlace);
                Ready = isBetPlace;
            }
           
            
        }
    }
}
