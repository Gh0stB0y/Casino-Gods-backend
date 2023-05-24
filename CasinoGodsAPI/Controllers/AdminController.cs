using System;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using CasinoGodsAPI.Migrations;
using CasinoGodsAPI.BlackjackTableModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CasinoGodsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        public AdminController(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _configuration = configuration;
        }


        [Route("AddGame")]
        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] string gameName)
        {

            if (await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g => g.Name == gameName) != null) return BadRequest("Game already exists in database");
            else
            {

                GamesDatabase newGame = new GamesDatabase
                {
                    Name = gameName,
                };
                await _casinoGodsDbContext.GamesList.AddAsync(newGame);

                var allPlayer = await _casinoGodsDbContext.Players.ToListAsync();
                foreach (var player_from_list in allPlayer)
                {
                    GamePlusPlayer gamePlusPlayer = new GamePlusPlayer
                    {
                        gameName = newGame,
                        player = player_from_list
                    };
                    await _casinoGodsDbContext.GamePlusPlayersTable.AddAsync(gamePlusPlayer);
                }
                await _casinoGodsDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [Route("DeleteGame")]
        [HttpDelete]
        public async Task<IActionResult> DeleteGame([FromBody] string gameName)
        {
            GamesDatabase newGame =await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g=>g.Name== gameName);

            if (newGame != null)
            {
                _casinoGodsDbContext.GamesList.Remove(newGame);

                await _casinoGodsDbContext.GamePlusPlayersTable.Where(p => p.gameName == newGame).ExecuteDeleteAsync();

                /*var allPlayer = await _casinoGodsDbContext.Players.ToListAsync();
                foreach (var player_from_list in allPlayer)
                {
                    GamePlusPlayer gamePlusPlayer = await _casinoGodsDbContext.GamePlusPlayersTable.SingleOrDefaultAsync(p=>p.gameName==newGame &&p.);
                    _casinoGodsDbContext.GamePlusPlayersTable.Remove(gamePlusPlayer);
                }*/
                _casinoGodsDbContext.SaveChanges();
                return Ok();
            }
            else return BadRequest("Game not found");
        }

        [Route("AddBlackjackTable")]
        [HttpPost]
        public async Task<IActionResult> AddTable([FromBody] BlackjackTableDatabase inputTable)
        {
            if (inputTable.CheckTable(inputTable))
            {
                if (await _casinoGodsDbContext.BlackjackTables.SingleOrDefaultAsync(t => t.name == inputTable.name) == null)
                {
                    await _casinoGodsDbContext.BlackjackTables.AddAsync(inputTable);
                    await _casinoGodsDbContext.SaveChangesAsync();
                    return Ok();
                }
                else return BadRequest("Table with given name already exists");
            }
            else return BadRequest("Wrong table data");
        }

        [Route("DeleteBlackjackTable")]
        [HttpDelete]
        public async Task<IActionResult> DeleteTable([FromBody] string input)
        {
            var Table = await _casinoGodsDbContext.BlackjackTables.SingleOrDefaultAsync(play => play.name == input);
            if (Table == null) { return BadRequest("Table not found"); }
            else
            {
                _casinoGodsDbContext.BlackjackTables.Remove(Table);
                await _casinoGodsDbContext.SaveChangesAsync();
                return Ok();
            }
        }
       
        [Route("EditBlackjackTable")]
        [HttpPut]
        public async Task<IActionResult> EditTable([FromBody] BlackjackTableDatabase inputTable)
        {
            if (inputTable.CheckTable(inputTable))
            {
                var Table = await _casinoGodsDbContext.BlackjackTables.SingleOrDefaultAsync(play => play.name == inputTable.name);
                if (Table != null)
                {
                    Table.minBet = inputTable.minBet; Table.maxBet = inputTable.maxBet; Table.betTime = inputTable.betTime; Table.actionTime = inputTable.actionTime;
                    Table.sidebet1 = inputTable.sidebet1; Table.sidebet2 = inputTable.sidebet2; Table.decks = inputTable.decks; Table.seatsCount = inputTable.seatsCount;
                    await _casinoGodsDbContext.SaveChangesAsync();
                    return Ok();
                }
                else return BadRequest("Table not found");

            }
            else return BadRequest("Wrong table data");
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlayer()
        {
            //ta funkcja mowi do bazy danych DAJ graczy
            var player = await _casinoGodsDbContext.Players.ToListAsync();
            return Ok(player);
        }

    }
}
