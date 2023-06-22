using CasinoGodsAPI.Data;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CasinoGodsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LobbyHubController:Controller
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private IDatabase redisDbLogin, redisDbJwt;
        private readonly IHubContext<LobbyHub> _hubContext;
        public LobbyHubController(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis, 
            IHubContext<LobbyHub> hubContext)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _configuration = configuration;
            _redis = redis;
            redisDbLogin = redis.GetDatabase();
            redisDbJwt = redis.GetDatabase();
            _hubContext = hubContext;
        }

    }
}
