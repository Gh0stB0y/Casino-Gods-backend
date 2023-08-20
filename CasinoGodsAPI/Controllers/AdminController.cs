﻿using System;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
//using CasinoGodsAPI.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Xml.Linq;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.DTOs;

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

        /*[Route("AddGame")]
        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] string gameName)
        {

            if (await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g => g.Name == gameName) != null) return BadRequest("Game already exists in database");
            else
            {
                Games newGame = new Games
                {
                    Name = gameName,
                };
                await _casinoGodsDbContext.GamesList.AddAsync(newGame);

                var allPlayer = await _casinoGodsDbContext.Players.ToListAsync();
                foreach (var player_from_list in allPlayer)
                {
                    GamePlayerTable gamePlusPlayer = new GamePlayerTable
                    {
                        GameName = newGame,
                        Player = player_from_list
                    };
                    await _casinoGodsDbContext.GamePlusPlayersTable.AddAsync(gamePlusPlayer);
                }
                await _casinoGodsDbContext.SaveChangesAsync();
                return Ok();
            }
        }*/
        /*[Route("DeleteGame")]
        [HttpDelete]
        public async Task<IActionResult> DeleteGame([FromBody] string gameName)
        {
            Games newGame = await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g => g.Name == gameName);

            if (newGame != null)
            {
                _casinoGodsDbContext.GamesList.Remove(newGame);

                await _casinoGodsDbContext.GamePlusPlayersTable.Where(p => p.GameName == newGame).ExecuteDeleteAsync();

                var allPlayer = await _casinoGodsDbContext.Players.ToListAsync();
                foreach (var player_from_list in allPlayer)
                {
                    GamePlusPlayer gamePlusPlayer = await _casinoGodsDbContext.GamePlusPlayersTable.SingleOrDefaultAsync(p => p.gameName == newGame && p.);
                    _casinoGodsDbContext.GamePlusPlayersTable.Remove(gamePlusPlayer);
                }
                _casinoGodsDbContext.SaveChanges();
                return Ok();
            }
            else return BadRequest("Game not found");
        }*/

        [Route("ManageTable")]
        [HttpPost]
        public async Task<IActionResult> ManageTable([FromBody] AdminManageTableDTO BJobj) {

 
            var gameObj = await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g => g.Name == BJobj.GameType);
            if (gameObj != null)
            {
                if (BJobj.CheckTable())
                {
                    switch (BJobj.ActionType)
                    {
                        case "add":
                            if (await _casinoGodsDbContext.TablesList.Where(g => g.CKGame == gameObj.Name).SingleOrDefaultAsync(t => t.CKname == BJobj.Name) == null)
                            {

                                Tables newTable = new Tables()
                                {
                                    CKname = BJobj.Name,
                                    CKGame = gameObj.Name,
                                    MinBet = BJobj.MinBet,
                                    MaxBet = BJobj.MaxBet,
                                    BetTime = BJobj.BetTime,
                                    Maxseats = BJobj.MaxSeats,
                                    ActionTime=BJobj.ActionTime,
                                    Sidebet1=BJobj.Sidebet1,
                                    Sidebet2=BJobj.Sidebet2,
                                    Decks=BJobj.Decks
                                };
                                await _casinoGodsDbContext.TablesList.AddAsync(newTable);
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return Ok();
                            }
                            else return BadRequest("Table with given name already exists");

                        case "edit":
                            var Table = await _casinoGodsDbContext.TablesList.Where(g=>g.CKGame==BJobj.GameType).SingleOrDefaultAsync(p => p.CKname == BJobj.Name);
                            if (Table != null)
                            {
                                /*Table.CKname = BJobj.name;*/Table.CKGame = gameObj.Name; Table.MinBet = BJobj.MinBet;Table.MaxBet = BJobj.MaxBet;Table.BetTime = BJobj.BetTime;
                                Table.Maxseats = BJobj.MaxSeats;Table.ActionTime = BJobj.ActionTime;Table.Sidebet1 = BJobj.Sidebet1;Table.Sidebet2 = BJobj.Sidebet2;
                                Table.Decks = BJobj.Decks;                               
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return Ok();
                            }
                            else return BadRequest("Table not found");

                        case "delete":
                            var Table2 = await _casinoGodsDbContext.TablesList.Where(g => g.CKGame == BJobj.GameType).SingleOrDefaultAsync(play => play.CKname == BJobj.Name);
                            if (Table2 == null) { return BadRequest("Table not found"); }
                            else
                            {
                                _casinoGodsDbContext.TablesList.Remove(Table2);
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return Ok();
                            }

                        default:
                            return BadRequest("Wrong action Type");
                    }
                }
                else return BadRequest("Wrong table data");
            }
            else return BadRequest("Wrong game type");        
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
