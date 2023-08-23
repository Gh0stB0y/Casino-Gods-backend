using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using MediatR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class ManualBetSendHandler : IRequestHandler<ManualBetSendCommand>
    {
        private readonly IMediator _mediator;

        public ManualBetSendHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(ManualBetSendCommand request, CancellationToken cancellationToken)
        {
            if (!ActiveTablesData.ManagedTablesObj[request.TableId].IsBettingListVoid(request.Bets) && 
                !ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.ContainsKey(request.Username)) 
                //bet are open, and betting list is valid
            {
                await _mediator.Send(new UpdateJwtCommand(request.ClaimsIdentity, request.Clients, request.Jwt), cancellationToken);
                await _mediator.Send(new PlacingBetCommand(request.Username, request.TableId, request.Bets, request.Clients), cancellationToken);
                Console.WriteLine(request.Username + " List uploaded");
            }
            else if (!ActiveTablesData.ManagedTablesObj[request.TableId].IsBettingListVoid(request.Bets) && 
                        ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.ContainsKey(request.Username))
            {
                Console.WriteLine("Bets already placed");
            }
            else
                Console.WriteLine("Empty List was sent to me");
        }
    }
}
