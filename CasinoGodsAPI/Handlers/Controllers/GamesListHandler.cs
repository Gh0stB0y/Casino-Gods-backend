using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Queries.Controllers.PlayerController;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class GamesListHandler : IRequestHandler<GamesListQuery, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;

        public GamesListHandler(CasinoGodsDbContext casinoGodsDbContext,IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _configuration = configuration;
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
        }
        public async Task<IActionResult> Handle(GamesListQuery request, CancellationToken cancellationToken)
        {
            string response = await GlobalFunctions.RefreshTokenGlobal(request.Jwt.jwtString, _redisDbLogin, _redisDbJwt, _configuration);
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
