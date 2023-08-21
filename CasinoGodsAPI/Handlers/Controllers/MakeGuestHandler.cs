using CasinoGodsAPI.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class MakeGuestHandler : IRequestHandler<MakeGuestCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;
        public MakeGuestHandler(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _configuration = configuration;
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
        }
        public async Task<IActionResult> Handle(MakeGuestCommand request, CancellationToken cancellationToken)
        {
            Player guestPlayer = new Player
            {
                Bankroll = 1000,
                Profit = 0,
                Username = "guest" + new Random().Next(0, 100000),
            };
            string jwt = guestPlayer.CreateToken("guest", _configuration);
            _redisDbLogin.StringSetAsync(guestPlayer.Username, jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            _redisDbJwt.StringSetAsync(jwt, guestPlayer.Username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            //_redisGuestBankrolls.StringSetAsync(guestPlayer.Username, "1000");
            ActivePlayerDTO ap = new ActivePlayerDTO
            {
                Username = guestPlayer.Username,
                Bankroll = guestPlayer.Bankroll,
                Profit = guestPlayer.Profit,
                Jwt = jwt
            };
            await _casinoGodsDbContext.SaveChangesAsync(cancellationToken);
            return new OkObjectResult(ap);
        }
    }
}
