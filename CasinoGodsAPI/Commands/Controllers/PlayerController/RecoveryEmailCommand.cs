using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Commands.Controllers.PlayerController
{
    public record RecoveryEmailCommand(EmailDTO Email):IRequest<IActionResult>;
}
