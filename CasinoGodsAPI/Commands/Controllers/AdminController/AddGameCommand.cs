using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Commands.Controllers.AdminController
{
    public record AddGameCommand(string GameName) : IRequest<IActionResult>;

}
