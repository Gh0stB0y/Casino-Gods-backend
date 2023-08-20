using CasinoGodsAPI.Commands.Controllers.PlayerController;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, IActionResult>
    {
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;
        private readonly IConfiguration _configuration;

        public LogoutHandler(IConnectionMultiplexer redis,IConfiguration configuration)
        {
            _configuration = configuration;
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
        }
        public async Task<IActionResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var playerToDelete = await _redisDbJwt.StringGetAsync(request.Jwt.jwtString);
            if (!playerToDelete.IsNull)
            {
                _redisDbJwt.KeyDeleteAsync(request.Jwt.jwtString, flags: CommandFlags.FireAndForget);
                _redisDbLogin.KeyDeleteAsync(playerToDelete.ToString(), flags: CommandFlags.FireAndForget);
            }
            return new OkResult();

        }
    }
}
