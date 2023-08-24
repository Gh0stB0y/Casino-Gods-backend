using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController
{
    public record CorrectSignInCommand(Player LoggedPlayer , string Username):IRequest<IActionResult>;

}
