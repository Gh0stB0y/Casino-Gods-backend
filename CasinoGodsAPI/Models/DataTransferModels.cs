namespace CasinoGodsAPI.Models
{
    public class PlayGameData
    {
        public string game;
        public string table;
        public string jwt;
    }

    public class BetCommand
    {
        public int bet;
        public int sidebet1;
        public int sidebet2;
    }
    public class Action
    {
        public string action;
    }

}
