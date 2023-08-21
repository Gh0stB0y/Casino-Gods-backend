using CasinoGodsAPI.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CasinoGodsAPI.Controllers;
using Microsoft.AspNet.SignalR.Messaging;
using CasinoGodsAPI.DTOs;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class SignUpHandler : IRequestHandler<SignUpCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        public SignUpHandler(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
        }
        public async Task<IActionResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            
            //warunki do spelnienia, unikalny login,unikalny mail,
            if ((await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Username == request.NewPlayer.username) == null) &&
                 await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Email == request.NewPlayer.email) == null)
            {
                string message = SignUpDTO.CheckSignUpCredentials(request.NewPlayer);
                if (message != "") return new BadRequestObjectResult(message);
                else
                {
                    Player new_player = new()
                    {
                        Id = Guid.NewGuid(),
                        Username = request.NewPlayer.username,
                        Email = request.NewPlayer.email,
                        Birthdate = request.NewPlayer.birthdate
                    };
                    new_player.HashPass(request.NewPlayer.password);

                    //dodawanie nowego gracza
                    await _casinoGodsDbContext.Players.AddAsync(new_player, cancellationToken);
                    //

                    var gameslist = await _casinoGodsDbContext.Games.ToListAsync();
                    foreach (var gameNameFromTable in gameslist)
                    {
                        GamePlayerTable gamePlusPlayer = new()
                        {
                            GameName = gameNameFromTable,
                            Player = new_player
                        };

                        await _casinoGodsDbContext.GamePlayers.AddAsync(gamePlusPlayer, cancellationToken);
                    }

                    var playerGamelist = await _casinoGodsDbContext.GamePlayers.Where(c => c.Player == new_player).ToListAsync(cancellationToken: cancellationToken);

                    await _casinoGodsDbContext.SaveChangesAsync(cancellationToken);
                    return new OkObjectResult(new_player);

                }

            }//dalsze sprawdzanie


            else if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.Username == request.NewPlayer.username) != null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.Email == request.NewPlayer.email) == null)       
                                                                        return new BadRequestObjectResult("Username already in use"); 
            
            else if ((_casinoGodsDbContext.Players.SingleOrDefault(play => play.Username == request.NewPlayer.username) == null) &&
                 _casinoGodsDbContext.Players.SingleOrDefault(play => play.Email == request.NewPlayer.email) != null)
                                                                        return new BadRequestObjectResult("Email already in use");
            else return new BadRequestObjectResult("Username and email already in use");

        }
    }
}
