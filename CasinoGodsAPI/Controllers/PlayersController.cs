using System;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
//using CasinoGodsAPI.Migrations;
using StackExchange.Redis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.DTOs;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace CasinoGodsAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt,redisGuestBankrolls;
        public PlayersController(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _configuration = configuration;
            _redis=redis;
            redisDbLogin = _redis.GetDatabase(0);
            redisDbJwt = _redis.GetDatabase(1);
            redisGuestBankrolls = _redis.GetDatabase(2);
        }
        
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterPlayer([FromBody] SignUpDTO playerRequest)
        {
            //warunki do spelnienia, unikalny login,unikalny mail,
            if ((await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Username == playerRequest.username) == null) &&
                 await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Email == playerRequest.email) == null)
            {

                string message = SignUpDTO.CheckSignUpCredentials(playerRequest);
                if (message != "") return BadRequest(message);
                else
                {
                    Player new_player = new Player();
                    new_player.Id = Guid.NewGuid();
                    new_player.Username = playerRequest.username;
                    new_player.Email = playerRequest.email;
                    new_player.Birthdate = playerRequest.birthdate;
                    new_player.HashPass(playerRequest.password);

                    //dodawanie nowego gracza
                    await _casinoGodsDbContext.Players.AddAsync(new_player);
                    //

                    var gameslist = await _casinoGodsDbContext.GamesList.ToListAsync();
                    foreach (var gameNameFromTable in gameslist)
                    {
                        GamePlayerTable gamePlusPlayer = new GamePlayerTable();
                        gamePlusPlayer.GameName = gameNameFromTable;
                        gamePlusPlayer.Player = new_player;

                        await _casinoGodsDbContext.GamePlusPlayersTable.AddAsync(gamePlusPlayer);
                    }

                    var playerGamelist = await _casinoGodsDbContext.GamePlusPlayersTable.Where(c => c.Player == new_player).ToListAsync();

                    await _casinoGodsDbContext.SaveChangesAsync();
                    return Ok(new_player);

                }

            }//dalsze sprawdzanie


            else if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.Username == playerRequest.username) != null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.Email == playerRequest.email) == null)
            { return BadRequest("Username already in use"); }
            else if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.Username == playerRequest.username) == null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.Email == playerRequest.email) != null)
            { return BadRequest("Email already in use"); }
            else return BadRequest("Username and email already in use");//username or email already in use
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginPlayer([FromBody] SignInDTO playerRequest)
        {
            Player loggedPlayer = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Username == playerRequest.username);
            if (loggedPlayer == null) return BadRequest("Username not found");
            else
            {
                if (!CheckPassword(playerRequest.password, loggedPlayer.PassHash, loggedPlayer.PassSalt)) return BadRequest("Password not correct");
                else
                {
                    var activePlayerCheck =await redisDbLogin.StringGetAsync(playerRequest.username);
                    if (!activePlayerCheck.IsNull) return Unauthorized("User is already logged in");
                    string jwt = loggedPlayer.CreateToken(playerRequest.username, _configuration);
                    redisDbLogin.StringSetAsync(playerRequest.username, jwt,new TimeSpan(0,0,5,0), flags: CommandFlags.FireAndForget);
                    redisDbJwt.StringSetAsync(jwt, playerRequest.username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                    ActivePlayerDTO ap = new ActivePlayerDTO
                    {
                        Username = loggedPlayer.Username,
                        Bankroll = loggedPlayer.Bankroll,
                        Profit = loggedPlayer.Profit,
                        Jwt = jwt
                    };
                    return Ok(ap);
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

        [Route("guest")]
        [HttpPost]
        public async Task<IActionResult> Guest()
        {
            Player guestPlayer = new Player
            {
                Bankroll = 1000,
                Profit = 0,
                Username = "guest" + new Random().Next(0, 100000),
            };
            string jwt = guestPlayer.CreateToken("guest", _configuration);
            redisDbLogin.StringSetAsync(guestPlayer.Username, jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            redisDbJwt.StringSetAsync(jwt, guestPlayer.Username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            redisGuestBankrolls.StringSetAsync(guestPlayer.Username, "1000");
            ActivePlayerDTO ap = new ActivePlayerDTO
            {
                Username = guestPlayer.Username,
                Bankroll = guestPlayer.Bankroll,
                Profit = guestPlayer.Profit,
                Jwt = jwt
            };
            await _casinoGodsDbContext.SaveChangesAsync();
            return Ok(ap);
        }

        [Route("refreshToken")]
        [HttpPut]
        public async Task<IActionResult> RefreshToken([FromBody] JwtDTO jwt)
        {
            string response = await GlobalFunctions.RefreshTokenGlobal(jwt.jwtString, redisDbLogin, redisDbJwt, _configuration);
            if (response == "Session expired, log in again"|| response=="Redis data error, log in again") return BadRequest(response);
            else return Ok(response);
        }
        
        [Route("recovery")]
        [HttpPut]
        public async Task<IActionResult> RecoveryPlayer([FromBody] EmailDTO email)
        {

            var playerToRecover = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Email == email.emailRec);
            if (playerToRecover != null)
            {
                string newPass = Player.GetRandomPassword(10);
                playerToRecover.Password = newPass;

                await _casinoGodsDbContext.SaveChangesAsync();
                Player.SendRecoveryEmail(playerToRecover.Email, playerToRecover.Password);
                return Ok();
            }
            else return BadRequest("Email not found");
        }

        [Route("Logout")]
        [HttpPut]
        public async Task<IActionResult> DeleteActivePlayer([FromBody] JwtDTO jwt)
        {
            var playerToDelete = await redisDbJwt.StringGetAsync(jwt.jwtString);
            if (!playerToDelete.IsNull) {
                redisDbJwt.KeyDeleteAsync(jwt.jwtString, flags: CommandFlags.FireAndForget);
                redisDbLogin.KeyDeleteAsync(playerToDelete.ToString(), flags: CommandFlags.FireAndForget);
             }
            return Ok();
        }

        [Route("TablesData")]
        [HttpPost]
        public async Task<IActionResult> GetTablesData([FromBody] JwtDTO jwt)
        {
            Console.WriteLine(jwt.jwtString);
            Console.WriteLine(jwt.jwtString.Length);


            string response = await GlobalFunctions.RefreshTokenGlobal(jwt.jwtString, redisDbLogin, redisDbJwt, _configuration);
            if (response == "Session expired, log in again") return BadRequest(response);
           
            else
            {
                TableInitialDataDTO obj = new TableInitialDataDTO()
                {
                    gameNames = await _casinoGodsDbContext.GamesList.Select(str => str.Name).ToListAsync(),
                    jwt = response
                };
                return Ok(obj);
            }
        }
      
    }
}
