using CasinoGodsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Database
{
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
