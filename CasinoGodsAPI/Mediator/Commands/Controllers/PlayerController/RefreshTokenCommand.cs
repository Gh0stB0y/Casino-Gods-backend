using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController
{
    public record RefreshTokenCommand(JwtDTO Jwt) : IRequest<IActionResult>;
}
