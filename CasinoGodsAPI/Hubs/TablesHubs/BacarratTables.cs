using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class BacarratTables:Tables
    {
        public BacarratTables(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis) : base(casinoGodsDbContext, configuration, redis) { }
    }
 
    public class BacarratTables1 : BacarratTables
    {
        public static Dictionary<string, TablesDatabase> TablesCount = new Dictionary<string, TablesDatabase>();
        public BacarratTables1(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis) 
            : base(casinoGodsDbContext, configuration, redis) {

           
        }
    }
    public class BacarratTables2 : BacarratTables
    {
        public BacarratTables2(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
            : base(casinoGodsDbContext, configuration, redis) { }
    }
    public class BacarratTables3 : BacarratTables
    {
        public BacarratTables3(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
            : base(casinoGodsDbContext, configuration, redis) { }
    }

}
