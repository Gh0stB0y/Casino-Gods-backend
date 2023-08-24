using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Commands.Databases;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.Databases
{
    public class AddPlayerDbRecordHandler : IRequestHandler<AddPlayerDbRecordCommand, IActionResult>
    {
        public async Task<IActionResult> Handle(AddPlayerDbRecordCommand request, CancellationToken cancellationToken)
        {
            Player new_player = new()
            {
                Id = Guid.NewGuid(),
                Username = request.NewPlayer.Username,
                Email = request.NewPlayer.Email,
                Birthdate = request.NewPlayer.Birthdate
            };
            new_player.HashPass(request.NewPlayer.Password);

            //dodawanie nowego gracza
            await request.CasinoGodsDbContext.Players.AddAsync(new_player, cancellationToken);
            //

            var gameslist = await request.CasinoGodsDbContext.Games.ToListAsync();
            foreach (var gameNameFromTable in gameslist)
            {
                GamePlayerTable gamePlusPlayer = new()
                {
                    GameName = gameNameFromTable,
                    Player = new_player
                };
                await request.CasinoGodsDbContext.GamePlayers.AddAsync(gamePlusPlayer, cancellationToken);
            }          
            await request.CasinoGodsDbContext.SaveChangesAsync(cancellationToken);
            return new OkObjectResult(new_player);
        }
    }
}
