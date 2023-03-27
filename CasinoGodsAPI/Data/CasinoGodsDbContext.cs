using CasinoGodsAPI.Models;
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
        public DbSet<ActivePlayers> ActivePlayersTable { get; set; }
    }
}
