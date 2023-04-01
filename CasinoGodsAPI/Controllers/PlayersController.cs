using System;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using CasinoGodsAPI.Migrations;

namespace CasinoGodsAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        public PlayersController(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _configuration = configuration;
        }


        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlayer()
        {
            //ta funkcja mowi do bazy danych DAJ graczy
            var player = await _casinoGodsDbContext.Players.ToListAsync();
            return Ok(player);
        }
        [HttpPost]
        public async Task<IActionResult> RegisterPlayer([FromBody] PlayerSignUp playerRequest)
        {
            //warunki do spelnienia, unikalny login,unikalny mail,
            if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.username == playerRequest.username) == null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.email == playerRequest.email) == null)
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
                        var activePlayerCheck = await _casinoGodsDbContext.ActivePlayersTable.SingleOrDefaultAsync(play => play.username == playerRequest.username);
                        if (activePlayerCheck != null) {
                            if (activePlayerCheck.jwtExpires > DateTime.Now) return Unauthorized("User is already logged in");
                            else { _casinoGodsDbContext.ActivePlayersTable.Remove(activePlayerCheck);}
                        }
                        
                        string jwt = loggedPlayer.CreateToken(playerRequest.username, _configuration);
                        var refreshToken = GenerateToken(jwt);
                        SetRefreshToken(loggedPlayer, refreshToken);

                    ActivePlayerDTO ap = new ActivePlayerDTO
                    {
                        username = loggedPlayer.username,
                        bankroll = loggedPlayer.bankroll,
                        profit = loggedPlayer.profit,
                        jwt = jwt
                    }; 
                        await _casinoGodsDbContext.SaveChangesAsync();
                        return Ok(ap);                       
                    }
                }          
        } 
        private RefreshToken GenerateToken(string jwt)
        {
            var refreshToken = new RefreshToken
            {
                Token = jwt,
                Expires = DateTime.Now.AddMinutes(1)
            };

            return refreshToken;
        }
        private async void SetRefreshToken(Player loggedPlayer,RefreshToken refreshToken)
        {
            var newActivePlayer = new ActivePlayers
            {
                username = loggedPlayer.username,
                bankroll = loggedPlayer.bankroll,
                profit = loggedPlayer.profit,
                RefreshToken = refreshToken.Token,
                jwtExpires = refreshToken.Expires
            };
            await _casinoGodsDbContext.ActivePlayersTable.AddAsync(newActivePlayer);
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
        public async Task<IActionResult> guest()
        {
            Player guestPlayer = new Player
            {
                bankroll = 1000,
                profit = 0,
                username="guest"+ new Random().Next(0,100000),
            };
            string jwt = guestPlayer.CreateToken("guest", _configuration);
            var refreshToken = GenerateToken(jwt);
            SetRefreshToken(guestPlayer, refreshToken);
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
            Console.WriteLine("MOJE JWT:");
            Console.WriteLine(jwt.jwtString);
            ActivePlayers ActivePlayer = await _casinoGodsDbContext.ActivePlayersTable.SingleOrDefaultAsync(play => play.RefreshToken == jwt.jwtString);
            if (ActivePlayer == null) { return Unauthorized("Unauthorized JWT"); }

            else if (ActivePlayer.jwtExpires < DateTime.Now)
            {
                ActivePlayer.RefreshToken = null;
                _casinoGodsDbContext.ActivePlayersTable.Remove(ActivePlayer);
                await _casinoGodsDbContext.SaveChangesAsync();
                return Unauthorized("JWT expired,log in again");
            }
            else
            {
                Console.WriteLine("udalo sie");
                jwt.jwtString = ActivePlayer.CreateToken(ActivePlayer.username, _configuration);
                var refreshToken = reGenerateToken(jwt.jwtString);
                ActivePlayer.jwtExpires = refreshToken.Expires;
                ActivePlayer.RefreshToken = refreshToken.Token;
                await _casinoGodsDbContext.SaveChangesAsync();
            }
            return Ok(ActivePlayer.RefreshToken);
        }
        private RefreshToken reGenerateToken(string jwt)
        {
            var refreshToken = new RefreshToken
            {
                Token = jwt,
                Expires = DateTime.Now.AddMinutes(1)
            };

            return refreshToken;
        }

        [Route("recovery")]
        [HttpPut]

        public async Task<IActionResult> recoveryPlayer([FromBody] EmailRecovery email) {

            var playerToRecover=_casinoGodsDbContext.Players.SingleOrDefault(play => play.email == email.emailRec);
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
        public async Task<IActionResult> deleteActivePlayer([FromBody] JwtClass jwt)
        {
            Console.WriteLine(jwt.jwtString);
            var playerToDelete = _casinoGodsDbContext.ActivePlayersTable.SingleOrDefault(play => play.RefreshToken == jwt.jwtString);
            Console.WriteLine(playerToDelete.username);
            Console.WriteLine(playerToDelete.RefreshToken);
          
            if (playerToDelete != null)
            {
                _casinoGodsDbContext.ActivePlayersTable.Remove(playerToDelete);
                await _casinoGodsDbContext.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
