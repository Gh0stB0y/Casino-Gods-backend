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

        [Route("AddGame")]
        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] string gameName)
        {
            return await _mediator.Send(new AddGameCommand(gameName));
        }
        [Route("DeleteGame")]
        [HttpDelete]
        public async Task<IActionResult> DeleteGame([FromBody] string gameName)
        {
           return await _mediator.Send(new DeleteGameCommand(gameName));
        }

        [Route("ManageTable")]
        [HttpPost]
        public async Task<IActionResult> ManageTable([FromBody] AdminManageTableDTO obj) {
            
            return await _mediator.Send(new ManageTableCommand(obj));
        }          

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlayer()
        {
            return await _mediator.Send(new GetAllPlayersQuery());
        }
    }
}
