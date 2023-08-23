using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Queries.Controllers.AdminController;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class GetAllPlayersHandler : IRequestHandler<GetAllPlayersQuery, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;

        public GetAllPlayersHandler(CasinoGodsDbContext casinoGodsDbContext)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
        }
        public async Task<IActionResult> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
        {
            var player = await _casinoGodsDbContext.Players.ToListAsync(cancellationToken: cancellationToken);
            return new OkObjectResult(player);
        }
    }
}
