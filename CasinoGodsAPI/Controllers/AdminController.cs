using System;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
//using CasinoGodsAPI.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CasinoGodsAPI.TablesModel;

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
                    GamePlusPlayer gamePlusPlayer = await _casinoGodsDbContext.GamePlusPlayersTable.SingleOrDefaultAsync(p => p.gameName == newGame && p.);
                    _casinoGodsDbContext.GamePlusPlayersTable.Remove(gamePlusPlayer);
                }*/
                _casinoGodsDbContext.SaveChanges();
                return Ok();
            }
            else return BadRequest("Game not found");
        }

        [Route("BlackjackManageTable")]
        [HttpPost]
        public async Task<IActionResult> BlackjackManageTable([FromBody]BlackjackTableDTO BJobj) {

            string gameName = "Blackjack";
            if (BJobj.CheckTable())
            {
                switch (BJobj.actionType)
                {
                    case "add":                    
                            if (await _casinoGodsDbContext.MyBlackJackTables.SingleOrDefaultAsync(t =>t.table.name == BJobj.name)==null) 
                                {                             
                                TablesDatabase newTable = new TablesDatabase()
                                {
                                    game = await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(t => t.Name == gameName),
                                    name = BJobj.name,                                   
                                    minBet = BJobj.minBet,
                                    maxBet = BJobj.maxBet,
                                    betTime = BJobj.betTime,
                                };
                                BlackjackTablesDatabase newBjTable = new BlackjackTablesDatabase()
                                {
                                    table=newTable,
                                    actionTime=BJobj.actionTime,
                                    sidebet1= BJobj.sidebet1,
                                    sidebet2= BJobj.sidebet2,
                                    decks= BJobj.decks,
                                    seatsCount= BJobj.seatsCount
                                };
                                await _casinoGodsDbContext.TablesList.AddAsync(newTable);
                                await _casinoGodsDbContext.MyBlackJackTables.AddAsync(newBjTable);
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return Ok();
                            }
                            else return BadRequest("Table with given name already exists");
             
                    case "edit":
                        BlackjackTablesDatabase Table = await _casinoGodsDbContext.MyBlackJackTables.SingleOrDefaultAsync(play => play.table.name == BJobj.name);
                        if (Table != null)
                        {
                            Table.table.minBet = BJobj.minBet; Table.table.maxBet = BJobj.maxBet; Table.table.betTime = BJobj.betTime; Table.actionTime = BJobj.actionTime;
                            Table.sidebet1 = BJobj.sidebet1; Table.sidebet2 = BJobj.sidebet2; Table.decks = BJobj.decks; Table.seatsCount = BJobj.seatsCount;
                            await _casinoGodsDbContext.SaveChangesAsync();
                            return Ok();
                        }
                        else return BadRequest("Table not found"); 

                    case "delete":
                        var Table2 = await _casinoGodsDbContext.TablesList.SingleOrDefaultAsync(play => play.name == BJobj.name);
                        var Table3 = await _casinoGodsDbContext.MyBlackJackTables.SingleOrDefaultAsync(play => play.table.name == BJobj.name);
                        if (Table3 == null) { return BadRequest("Table not found"); }
                        else
                        {
                            _casinoGodsDbContext.TablesList.Remove(Table2);
                            _casinoGodsDbContext.MyBlackJackTables.Remove(Table3);
                            
                            await _casinoGodsDbContext.SaveChangesAsync();
                            return Ok();
                        }
                
                    default:
                        return BadRequest("Wrong action Type");                   
                }
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
