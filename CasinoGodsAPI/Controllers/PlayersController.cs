using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                //dodawanie nowego gracza
                playerRequest.Id = Guid.NewGuid();
                await _casinoGodsDbContext.Players.AddAsync(playerRequest);
                await _casinoGodsDbContext.SaveChangesAsync();
                return Ok(playerRequest);
                //
            }//dalsze sprawdzanie

            else return BadRequest("Username or email already in use");//username or email already in use



             
          
            //return Ok(playerRequest);
        }
    }
}
