using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Queries.Controllers.PlayerController;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class GamesListHandler : IRequestHandler<GamesListQuery, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;

        public GamesListHandler(CasinoGodsDbContext casinoGodsDbContext, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _configuration = configuration;
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
        }
        public async Task<IActionResult> Handle(GamesListQuery request, CancellationToken cancellationToken)
        {
            //string response = await RefreshToken.RefreshTokenGlobal(request.Jwt.jwtString, _redisDbLogin, _redisDbJwt, _configuration);
            string response = await LobbyService.RefreshTokenGlobal(request.Jwt.jwtString);
            if (response == "Session expired, log in again") return new BadRequestObjectResult(response);
            else
            {
                GamesListDTO obj = new GamesListDTO()
                {
                    gameNames = await _casinoGodsDbContext.Games.Select(str => str.Name).ToListAsync(cancellationToken: cancellationToken),
                    jwt = response
                };
                return new OkObjectResult(obj);
            }
        }
    }
}
