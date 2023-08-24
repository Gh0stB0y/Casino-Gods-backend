using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class CorrectSignInHandler : IRequestHandler<CorrectSignInCommand,IActionResult>
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;
        public CorrectSignInHandler(IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
        }
        public async Task<IActionResult> Handle(CorrectSignInCommand request, CancellationToken cancellationToken)
        {
            var activePlayerCheck = await _redisDbLogin.StringGetAsync(request.Username);
            if (!activePlayerCheck.IsNull)
                return new UnauthorizedObjectResult("User is already logged in");

            string jwt = request.LoggedPlayer.CreateToken(request.Username, _configuration);
            _redisDbLogin.StringSetAsync(request.Username, jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            _redisDbJwt.StringSetAsync(jwt, request.Username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            ActivePlayerDTO ap = new()
            {
                Username = request.LoggedPlayer.Username,
                Bankroll = request.LoggedPlayer.Bankroll,
                Profit = request.LoggedPlayer.Profit,
                Jwt = jwt
            };
            return new OkObjectResult(ap);
        }
    }
}
