using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Commands.Controllers.PlayerController
{
    public record MakeGuestCommand():IRequest<IActionResult>;
    
}
