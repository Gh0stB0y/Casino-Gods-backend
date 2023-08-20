using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Queries.Controllers.PlayerController
{
    public record GamesListQuery(JwtDTO Jwt):IRequest<IActionResult>;
}
