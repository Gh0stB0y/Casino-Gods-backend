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

      /*  protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TablesDatabase>()
                .HasKey(t => new { t.game, t.name });

            modelBuilder.Entity<TablesDatabase>()
                .HasOne(t => t.GameDatabase)
                .WithMany(g => g.Tables)
                .HasForeignKey(t => t.game);

            base.OnModelCreating(modelBuilder);
        }*/

    }
}
