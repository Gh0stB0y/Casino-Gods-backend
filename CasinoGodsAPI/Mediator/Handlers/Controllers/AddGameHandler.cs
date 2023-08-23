using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Commands.Controllers.AdminController;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class AddGameHandler : IRequestHandler<AddGameCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;

        public AddGameHandler(CasinoGodsDbContext casinoGodsDbContext)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
        }
        public async Task<IActionResult> Handle(AddGameCommand request, CancellationToken cancellationToken)
        {
            if (await _casinoGodsDbContext.Games.SingleOrDefaultAsync(g => g.Name == request.GameName, cancellationToken: cancellationToken) != null) return new BadRequestObjectResult("Game already exists in database");
            else
            {
                Games newGame = new Games
                {
                    Name = request.GameName,
                };
                await _casinoGodsDbContext.Games.AddAsync(newGame, cancellationToken);

                var allPlayer = await _casinoGodsDbContext.Players.ToListAsync(cancellationToken: cancellationToken);
                foreach (var player_from_list in allPlayer)
                {
                    GamePlayerTable gamePlusPlayer = new GamePlayerTable
                    {
                        GameName = newGame,
                        Player = player_from_list
                    };
                    await _casinoGodsDbContext.GamePlayers.AddAsync(gamePlusPlayer, cancellationToken);
                }
                await _casinoGodsDbContext.SaveChangesAsync(cancellationToken);
                return new OkResult();
            }
        }
    }
}