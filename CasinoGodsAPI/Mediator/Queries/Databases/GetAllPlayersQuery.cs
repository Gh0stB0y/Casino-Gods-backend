using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Queries.Databases
{
    public record GetAllPlayersQuery() : IRequest<IActionResult>;
}
