using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Models.SignalRHubModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class SaveBankrollProfitToDbHandler : IRequestHandler<SaveBankrollProfitToDbCommand>
    {
        private readonly ILogger<SaveBankrollProfitToDbHandler> _logger;

        public SaveBankrollProfitToDbHandler(ILogger<SaveBankrollProfitToDbHandler> logger)
        {
            _logger = logger;
        }
        public async Task Handle(SaveBankrollProfitToDbCommand request, CancellationToken cancellationToken)
        {
            var player = await request.CasinoGodsDbContext.Players.SingleOrDefaultAsync(p => p.Username == request.ClaimsFields.Username, cancellationToken: cancellationToken);
            if (player == null)
            {
                _logger.LogCritical("CRITICAL ERROR! USERNAME NOT FOUND IN DATABASE");
            }
            else
            {
                player.Bankroll = request.ClaimsFields.Bankroll;
                player.Profit += request.ClaimsFields.Profit;
                await request.CasinoGodsDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Data saved");
            }
        }
    }
}
