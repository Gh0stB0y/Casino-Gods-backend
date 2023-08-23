using CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, IActionResult>
    {
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;
        private readonly IConfiguration _configuration;
        public RefreshTokenHandler(IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
            _configuration = configuration;
        }
        public async Task<IActionResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {          
            string response = await LobbyService.RefreshTokenGlobal(request.Jwt.jwtString);

            if (response == "Session expired, log in again" || response == "Redis data error, log in again")
            {
                return new BadRequestObjectResult(response);
            }
            else
            {
                return new OkObjectResult(response);
            }
        }
    }
}
