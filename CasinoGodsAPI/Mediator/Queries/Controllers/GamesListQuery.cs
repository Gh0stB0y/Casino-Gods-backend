using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Queries.Controllers
{
    public record GamesListQuery(JwtDTO Jwt) : IRequest<IActionResult>;
}
