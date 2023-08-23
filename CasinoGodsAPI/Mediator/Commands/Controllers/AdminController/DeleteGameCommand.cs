using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.AdminController
{
    public record DeleteGameCommand(string GameName) : IRequest<IActionResult>;
}
