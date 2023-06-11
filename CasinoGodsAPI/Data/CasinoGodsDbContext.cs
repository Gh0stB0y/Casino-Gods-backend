using CasinoGodsAPI.Models;
using CasinoGodsAPI.TablesModel;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Data
{
    public class CasinoGodsDbContext : DbContext
    {
        public CasinoGodsDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Player> Players { get; set; }
        public DbSet<GamesDatabase> GamesList { get; set; }
        public DbSet<GamePlusPlayer> GamePlusPlayersTable{ get; set; }
        public DbSet<TablesDatabase> TablesList { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<BlackjackTablesDatabase> MyBlackJackTables { get; set; }
        public List<BlackjackTable> ActiveBlackjackTable { get; set; }




    }
}
