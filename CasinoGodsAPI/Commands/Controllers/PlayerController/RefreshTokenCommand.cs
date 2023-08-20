using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Commands.Controllers.PlayerController
{
    public record RefreshTokenCommand(JwtDTO Jwt):IRequest<IActionResult>;    
}
