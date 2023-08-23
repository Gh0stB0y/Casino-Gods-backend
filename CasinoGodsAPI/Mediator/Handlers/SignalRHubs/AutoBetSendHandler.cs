using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using MediatR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class AutoBetSendHandler : IRequestHandler<AutoBetSendCommand>
    {
        private readonly IMediator _mediator;

        public AutoBetSendHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(AutoBetSendCommand request, CancellationToken cancellationToken)
        {
            if (ActiveTablesData.ManagedTablesObj[request.TableId].ClosedBetsToken == request.ClosedBetsToken)
            {
                if (!ActiveTablesData.ManagedTablesObj[request.TableId].IsBettingListVoid(request.Bets) 
                    && !ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.ContainsKey(request.Username)) 
                    //bets are closed 
                {
                    await _mediator.Send(new UpdateJwtCommand(request.ClaimsIdentity, request.Clients, request.Jwt));
                    ActiveTablesData.ManagedTablesObj[request.TableId].UserInitialBetsDictionary[request.Username] = request.Bets;
                    Console.WriteLine(request.Username + "List automatically uploaded");
                }
                else if (!ActiveTablesData.ManagedTablesObj[request.TableId].IsBettingListVoid(request.Bets) && 
                            ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.ContainsKey(request.Username))
                {
                    Console.WriteLine("Bets already placed");
                }
                else
                {
                    Console.WriteLine("Empty List");
                    ActiveTablesData.ManagedTablesObj[request.TableId].UserInitialBetsDictionary.TryAdd(request.Username, Enumerable.Repeat(0, 157).ToList());
                }
            }
            else { Console.WriteLine("Wrong ClosedBets Token"); }
        }
    }
}
