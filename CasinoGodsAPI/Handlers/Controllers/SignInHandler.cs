using CasinoGodsAPI.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class SignInHandler : IRequestHandler<SignInCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _redisDbLogin;
        private readonly IDatabase _redisDbJwt;
        public SignInHandler(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _configuration = configuration;
            _redisDbLogin = redis.GetDatabase(0);
            _redisDbJwt = redis.GetDatabase(1);
        }
        public async Task<IActionResult> Handle(SignInCommand request, CancellationToken cancellationToken)
        {            
            Player loggedPlayer = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Username == request.Player.username);
            if (loggedPlayer == null) return new BadRequestObjectResult("Username not found");
            else
            {
                if (!CheckPassword(request.Player.password, loggedPlayer.PassHash, loggedPlayer.PassSalt))
                    return new UnauthorizedObjectResult("Password not correct");
                else
                {
                    var activePlayerCheck = await _redisDbLogin.StringGetAsync(request.Player.username);
                    if (!activePlayerCheck.IsNull)                    
                        return new UnauthorizedObjectResult("User is already logged in");
                    
                    string jwt = loggedPlayer.CreateToken(request.Player.username, _configuration);
                    _redisDbLogin.StringSetAsync(request.Player.username, jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                    _redisDbJwt.StringSetAsync(jwt, request.Player.username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                    ActivePlayerDTO ap = new ActivePlayerDTO
                    {
                        Username = loggedPlayer.Username,
                        Bankroll = loggedPlayer.Bankroll,
                        Profit = loggedPlayer.Profit,
                        Jwt = jwt
                    };            
                    return new OkObjectResult(ap);
                }
            }
        }
        private bool CheckPassword(string password, byte[] passHash, byte[] passSalt)
        {
            using (var hmac = new HMACSHA512(passSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passHash);

            }
        }
    }
}
