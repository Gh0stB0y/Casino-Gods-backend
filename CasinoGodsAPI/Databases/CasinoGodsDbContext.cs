using CasinoGodsAPI.Models;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.TablesModel;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Data
{
    public class CasinoGodsDbContext : DbContext
    {
        public CasinoGodsDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tables>()
            .HasKey(k => new { k.CKname, k.CKGame });
            modelBuilder.Entity<Games>()
            .HasKey(k => k.Name);
            modelBuilder.Entity<ActiveTablesDB>()
            .HasKey(k => k.TableInstanceId);

            modelBuilder.Entity<Games>()
            .HasMany(t => t.Tables)
            .WithOne(m => m.Game)
            .HasForeignKey(k => new { k.CKGame });

        }
        public DbSet<Player> Players { get; set; }
        public DbSet<Games> GamesList { get; set; }
        public DbSet<GamePlayerTable> GamePlusPlayersTable{ get; set; }
        public DbSet<Tables> TablesList { get; set; }
        public DbSet<ActiveTablesDB>ActiveTables { get; set; }
    }
   
}
