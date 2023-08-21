using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Commands.Controllers.AdminController
{
    public record DeleteGameCommand(string GameName):IRequest<IActionResult>;
}
