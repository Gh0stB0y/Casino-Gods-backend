using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Queries.Controllers.AdminController
{
    public record GetAllPlayersQuery() : IRequest<IActionResult>;
}
