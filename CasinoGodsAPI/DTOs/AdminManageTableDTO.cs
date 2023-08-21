namespace CasinoGodsAPI.DTOs
{
    public class AdminManageTableDTO  //DTO do zarzadzania stolem przez admina
    {
        public string GameType { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Name { get; set; }
        public int MinBet { get; set; } = 0;
        public int MaxBet { get; set; } = 100000;
        public int BetTime { get; set; } = 30;
        public int ActionTime { get; set; } = 15;
        public bool Sidebet1 { get; set; } = true;
        public bool Sidebet2 { get; set; } = true;
        public int Decks { get; set; } = 6;
        public int MaxSeats { get; set; } = 6;

        public AdminManageTableDTO(int maxSeats)
        {
            MaxSeats = maxSeats;
        }

        public bool CheckTable()
        {
            if (
                Name == null ||
                MinBet < 0 ||
                MaxBet < MinBet ||
                BetTime < 0 ||
                ActionTime < 0 ||
                Decks < 1 ||
                Decks > 10 ||
                MaxSeats < 1 ||
                MaxSeats > 10) 
                        return false;
            else        return true;
        }
    }
}
