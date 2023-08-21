using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Queries.Controllers.AdminController
{
    public record GetAllPlayersQuery():IRequest<IActionResult>;
}
