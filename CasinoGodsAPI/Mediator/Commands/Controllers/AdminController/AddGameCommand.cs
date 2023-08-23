using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.AdminController
{
    public record AddGameCommand(string GameName) : IRequest<IActionResult>;

}
