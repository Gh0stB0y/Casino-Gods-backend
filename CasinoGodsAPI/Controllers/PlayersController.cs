using Microsoft.AspNetCore.Mvc;
using CasinoGodsAPI.DTOs;
using MediatR;
using CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Mediator.Queries.Controllers;

namespace CasinoGodsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        private readonly IMediator _mediator;
        public PlayersController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> SignUpPlayer([FromBody] SignUpDTO playerRequest)
        {
             return await _mediator.Send(new SignUpCommand(playerRequest));
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> SignInPlayer([FromBody] SignInDTO playerRequest)
        {
            return await _mediator.Send(new SignInCommand(playerRequest));
        }     

        [Route("Guest")]
        [HttpPost]
        public async Task<IActionResult> Guest()
        {
            return await _mediator.Send(new MakeGuestCommand());
        }

        [Route("RefreshToken")]
        [HttpPut]
        public async Task<IActionResult> RefreshToken([FromBody] JwtDTO jwt)
        {
            return await _mediator.Send(new RefreshTokenCommand(jwt));
        }
        
        [Route("Recovery")]
        [HttpPut]
        public async Task<IActionResult> RecoveryPlayer([FromBody] EmailDTO email)
        {
            return await _mediator.Send(new RecoveryEmailCommand(email));
        }

        [Route("Logout")]
        [HttpPut]
        public async Task<IActionResult> DeleteActivePlayer([FromBody] JwtDTO jwt)
        {
            return await _mediator.Send(new LogoutCommand(jwt));
        }

        [Route("GetGamesList")]
        [HttpPost]
        public async Task<IActionResult> GetGamesList([FromBody] JwtDTO jwt)
        {
            return await _mediator.Send(new GamesListQuery(jwt));
        } 
    }
}
