using CasinoGodsAPI.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class RecoveryEmailHandler : IRequestHandler<RecoveryEmailCommand, IActionResult>
    {      
        private readonly CasinoGodsDbContext _casinoGodsDbContext;

        public RecoveryEmailHandler(CasinoGodsDbContext casinoGodsDbContext)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
        }
        public async Task<IActionResult> Handle(RecoveryEmailCommand request, CancellationToken cancellationToken)
        {           
            var playerToRecover = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Email == request.Email.emailRec, cancellationToken: cancellationToken);
            if (playerToRecover != null)
            {
                string newPass = Player.GetRandomPassword(10);
                playerToRecover.Password = newPass;

                await _casinoGodsDbContext.SaveChangesAsync(cancellationToken);
                Player.SendRecoveryEmail(playerToRecover.Email, playerToRecover.Password);
                return new OkResult();
            }
            else
            {
                return new BadRequestObjectResult("Email not found");
            }
        }
    }
}
