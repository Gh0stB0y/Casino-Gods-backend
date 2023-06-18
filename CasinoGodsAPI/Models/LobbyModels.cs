namespace CasinoGodsAPI.Models
{
    public class LobbyTableData
    {
       public string jwt { get; set; }
       public string TableName { get; set; }
       public int MinBet { get; set; }
       public int MaxBet { get; set; }
       public int MaxSeats { get; set; }
       public int SeatCount { get; set; }
    }
}
