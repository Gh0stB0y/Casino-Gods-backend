using System;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CasinoGodsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;

        public PlayersController(CasinoGodsDbContext CasinoGodsDbContext)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPlayer()
        {
            //ta funkcja mowi do bazy danych DAJ graczy
          var player= await _casinoGodsDbContext.Players.ToListAsync();
            return Ok(player);
        }
        [HttpPost]
        public async Task<IActionResult> RegisterPlayer([FromBody] Player playerRequest)
        {
            //warunki do spelnienia, unikalny login,unikalny mail,
            if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.username == playerRequest.username) == null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.email == playerRequest.email) == null)
            {

                string message=Player.CheckSignUpCredentials(playerRequest);
                if (message != "") return BadRequest(message);
                else
                {
                    //dodawanie nowego gracza
                    playerRequest.Id = Guid.NewGuid();
                    await _casinoGodsDbContext.Players.AddAsync(playerRequest);
                    await _casinoGodsDbContext.SaveChangesAsync();
                    return Ok(playerRequest);
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
        public async Task<IActionResult> LoginPlayer([FromBody] Player playerRequest)
        {
            Console.WriteLine("FUNKCJA WESZLA");
            Player loggedPlayer = _casinoGodsDbContext.Players.SingleOrDefault(play => play.username == playerRequest.username);
            if (loggedPlayer == null) return BadRequest("Username not found");
            else
            {
                if (loggedPlayer.password == playerRequest.password) return Ok();
                else return BadRequest("Password not correct");
            }
        }
        [Route("recovery")]
        [HttpPost]
        public async Task<IActionResult> recoveryPlayer([FromBody] Player recoveryPlayer) {
            var playerToRecover=_casinoGodsDbContext.Players.SingleOrDefault(play => play.email == recoveryPlayer.email);
            if (playerToRecover != null)
            {
                string newPass = Player.GetRandomPassword(10);
                playerToRecover.password = newPass;
                //zamienic tego playerToRecover z poprzednim uzytkownikiem

                //
                Player.sendRecoveryEmail(playerToRecover.email, playerToRecover.password);
                return Ok();
            }
            else return BadRequest("Email not found");
        }
    }
}
