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
using System.Xml.Linq;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.DTOs;
using MediatR;
using CasinoGodsAPI.Queries.Controllers.AdminController;
using CasinoGodsAPI.Commands.Controllers.AdminController;

namespace CasinoGodsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("ManageTable")]
        [HttpPost]
        public async Task<IActionResult> ManageTable([FromBody] AdminManageTableDTO obj) {
            /*
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
                                    ActionTime = BJobj.ActionTime,
                                    Sidebet1 = BJobj.Sidebet1,
                                    Sidebet2 = BJobj.Sidebet2,
                                    Decks = BJobj.Decks
                                };
                                await _casinoGodsDbContext.TablesList.AddAsync(newTable);
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return Ok();
                            }
                            else return BadRequest("Table with given name already exists");

                        case "edit":
                            var Table = await _casinoGodsDbContext.TablesList.Where(g => g.CKGame == BJobj.GameType).SingleOrDefaultAsync(p => p.CKname == BJobj.Name);
                            if (Table != null)
                            {
                                Table.CKname = BJobj.name; Table.CKGame = gameObj.Name; Table.MinBet = BJobj.MinBet; Table.MaxBet = BJobj.MaxBet; Table.BetTime = BJobj.BetTime;
                                Table.Maxseats = BJobj.MaxSeats; Table.ActionTime = BJobj.ActionTime; Table.Sidebet1 = BJobj.Sidebet1; Table.Sidebet2 = BJobj.Sidebet2;
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
*/
            return await _mediator.Send(new ManageTableCommand(obj));
        }          

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlayer()
        {
            return await _mediator.Send(new GetAllPlayersQuery());
        }
    }
}
