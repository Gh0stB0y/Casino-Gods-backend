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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TablesDatabase>()
            .HasKey(k => new { k.CKname, k.CKGame });
            modelBuilder.Entity<GamesDatabase>()
            .HasKey(k => k.Name);
            modelBuilder.Entity<ActiveTablesDatabase>()
            .HasKey(k => k.TableInstanceId);

            modelBuilder.Entity<GamesDatabase>()
            .HasMany(t => t.Tables)
            .WithOne(m => m.Game)
            .HasForeignKey(k => new { k.CKGame });

        }
        public DbSet<Player> Players { get; set; }
        public DbSet<GamesDatabase> GamesList { get; set; }
        public DbSet<GamePlusPlayer> GamePlusPlayersTable{ get; set; }
        public DbSet<TablesDatabase> TablesList { get; set; }
        //public DbSet<Dealer> Dealers { get; set; }
        public DbSet<ActiveTablesDatabase>ActiveTables { get; set; }


    }
    public class CasinoGodsDbContextFactory : IDbContextFactory<CasinoGodsDbContext>
    {
        private readonly IConfiguration _configuration;

        public CasinoGodsDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CasinoGodsDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<CasinoGodsDbContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("FullStackConnectionString"));
            return new CasinoGodsDbContext(optionsBuilder.Options);
        }
    }
}
