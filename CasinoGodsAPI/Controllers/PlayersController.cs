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
using CasinoGodsAPI.TablesModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IHubContext<BlackJackLobby> _BlackJackLobbyContext;
        private readonly IDatabase redisDbLogin, redisDbJwt;
        public PlayersController(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis,
                                 IHubContext<BlackJackLobby>BlackJackLobbyContext)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _configuration = configuration;
            _redis=redis;        
            redisDbLogin = redis.GetDatabase();
            redisDbJwt = redis.GetDatabase();
            _BlackJackLobbyContext=BlackJackLobbyContext;
        }
        
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterPlayer([FromBody] PlayerSignUp playerRequest)
        {
            //warunki do spelnienia, unikalny login,unikalny mail,
            if ((await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.username == playerRequest.username) == null) &&
                 await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.email == playerRequest.email) == null)
            {

                string message = PlayerSignUp.CheckSignUpCredentials(playerRequest);
                if (message != "") return BadRequest(message);
                else
                {
                    Player new_player = new Player();
                    new_player.Id = Guid.NewGuid();
                    new_player.username = playerRequest.username;
                    new_player.email = playerRequest.email;
                    new_player.birthdate = playerRequest.birthdate;
                    new_player.hashPass(playerRequest.password);

                    //dodawanie nowego gracza
                    await _casinoGodsDbContext.Players.AddAsync(new_player);
                    //

                    var gameslist = await _casinoGodsDbContext.GamesList.ToListAsync();
                    foreach (var gameNameFromTable in gameslist)
                    {
                        GamePlusPlayer gamePlusPlayer = new GamePlusPlayer();
                        gamePlusPlayer.gameName = gameNameFromTable;
                        gamePlusPlayer.player = new_player;

                        await _casinoGodsDbContext.GamePlusPlayersTable.AddAsync(gamePlusPlayer);
                    }

                    var playerGamelist = await _casinoGodsDbContext.GamePlusPlayersTable.Where(c => c.player == new_player).ToListAsync();

                    await _casinoGodsDbContext.SaveChangesAsync();
                    return Ok(new_player);

                }

            }//dalsze sprawdzanie


            else if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.username == playerRequest.username) != null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.email == playerRequest.email) == null)
            { return BadRequest("Username already in use"); }
            else if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.username == playerRequest.username) == null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.email == playerRequest.email) != null)
            { return BadRequest("Email already in use"); }
            else return BadRequest("Username and email already in use");//username or email already in use
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginPlayer([FromBody] PlayerSignIn playerRequest)
        {
            Player loggedPlayer = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.username == playerRequest.username);
            if (loggedPlayer == null) return BadRequest("Username not found");
            else
            {
                if (!CheckPassword(playerRequest.password, loggedPlayer.passHash, loggedPlayer.passSalt)) return BadRequest("Password not correct");
                else
                {
                    var activePlayerCheck =await redisDbLogin.StringGetAsync(playerRequest.username);
                    if (!activePlayerCheck.IsNull) return Unauthorized("User is already logged in");
                    string jwt = loggedPlayer.CreateToken(playerRequest.username, _configuration);
                    redisDbLogin.StringSetAsync(playerRequest.username, jwt,new TimeSpan(0,0,5,0), flags: CommandFlags.FireAndForget);
                    redisDbJwt.StringSetAsync(jwt, playerRequest.username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
                    ActivePlayerDTO ap = new ActivePlayerDTO
                    {
                        username = loggedPlayer.username,
                        bankroll = loggedPlayer.bankroll,
                        profit = loggedPlayer.profit,
                        jwt = jwt
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
                bankroll = 1000,
                profit = 0,
                username = "guest" + new Random().Next(0, 100000),
            };
            string jwt = guestPlayer.CreateToken("guest", _configuration);
            redisDbLogin.StringSetAsync(guestPlayer.username, jwt, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            redisDbJwt.StringSetAsync(jwt, guestPlayer.username, new TimeSpan(0, 0, 5, 0), flags: CommandFlags.FireAndForget);
            ActivePlayerDTO ap = new ActivePlayerDTO
            {
                username = guestPlayer.username,
                bankroll = guestPlayer.bankroll,
                profit = guestPlayer.profit,
                jwt = jwt
            };
            await _casinoGodsDbContext.SaveChangesAsync();
            return Ok(ap);
        }

        [Route("refreshToken")]
        [HttpPut]
        public async Task<IActionResult> RefreshToken([FromBody] JwtClass jwt)
        {
            string response = await GlobalFunctions.RefreshTokenGlobal(jwt.jwtString, redisDbLogin, redisDbJwt, _configuration);
            if (response == "Session expired, log in again") return BadRequest(response);
            else return Ok(response);
        }
        
        [Route("recovery")]
        [HttpPut]
        public async Task<IActionResult> RecoveryPlayer([FromBody] EmailRecovery email)
        {

            var playerToRecover = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.email == email.emailRec);
            if (playerToRecover != null)
            {
                string newPass = Player.GetRandomPassword(10);
                playerToRecover.password = newPass;

                await _casinoGodsDbContext.SaveChangesAsync();
                Player.sendRecoveryEmail(playerToRecover.email, playerToRecover.password);
                return Ok();
            }
            else return BadRequest("Email not found");
        }

        [Route("Logout")]
        [HttpPut]
        public async Task<IActionResult> DeleteActivePlayer([FromBody] JwtClass jwt)
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
        public async Task<IActionResult> GetTablesData([FromBody] JwtClass jwt)
        {
            string response = await GlobalFunctions.RefreshTokenGlobal(jwt.jwtString, redisDbLogin, redisDbJwt, _configuration);
            if (response == "Session expired, log in again") return BadRequest(response);
            else
            {
                TableDataDTO obj= new TableDataDTO()
                {
                    gameNames= await _casinoGodsDbContext.GamesList.Select(str => str.Name).ToListAsync(),
                    jwt=response
                };           
                return Ok(obj);
            }
        }                                
   
        [Route("HubTest")]
        [HttpPost]
        public async Task<IActionResult> HubTest(string msg)
        {
                await _BlackJackLobbyContext.Clients.All.SendAsync("ChatMessages", msg);
                return Ok();                     
        }
    
    
    
    
    }
}
