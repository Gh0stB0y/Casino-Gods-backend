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
    }
}
