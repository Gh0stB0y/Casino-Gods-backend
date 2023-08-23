using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController
{
    public record MakeGuestCommand() : IRequest<IActionResult>;

}
