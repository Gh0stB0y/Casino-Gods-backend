using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Commands.Controllers.AdminController;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class DeleteGameHandler : IRequestHandler<DeleteGameCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;

        public DeleteGameHandler(CasinoGodsDbContext casinoGodsDbContext)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
        }
        public async Task<IActionResult> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
        {
            Games newGame = await _casinoGodsDbContext.Games.SingleOrDefaultAsync(g => g.Name == request.GameName, cancellationToken: cancellationToken);

            if (newGame != null)
            {
                _casinoGodsDbContext.Games.Remove(newGame);
                await _casinoGodsDbContext.GamePlayers.Where(p => p.GameName == newGame).ExecuteDeleteAsync(cancellationToken: cancellationToken);
                await _casinoGodsDbContext.SaveChangesAsync(cancellationToken);
                return new OkResult();
            }
            else return new BadRequestObjectResult("Game not found");
        }
    }
}
