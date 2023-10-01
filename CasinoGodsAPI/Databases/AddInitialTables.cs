using CasinoGodsAPI.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Databases
{
    public static class InitialTables
    {
        public static List<ActiveTablesDB> AddInitialTables()
        {
            List<ActiveTablesDB> tables = new List<ActiveTablesDB>
            {
                new ActiveTablesDB()
                {
                    Name = "Beginner's Table",
                    Game = "Roulette",
                    MinBet = 1,
                    MaxBet = 100,
                    BetTime = 30,
                    Maxseats = 3,
                    ActionTime = 10,
                    Sidebet1 = false,
                    Sidebet2 = false,
                    Decks = 6
                },
                new ActiveTablesDB()
                {
                    Name = "Adveturous Table",
                    Game = "Roulette",
                    MinBet = 25,
                    MaxBet = 500,
                    BetTime = 20,
                    Maxseats = 6,
                    ActionTime = 5,
                    Sidebet1 = false,
                    Sidebet2 = false,
                    Decks = 6
                }
            };



            return tables;
        }
    }
}
