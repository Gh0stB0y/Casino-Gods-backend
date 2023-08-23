using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class PlacingBetHandler : IRequestHandler<PlacingBetCommand>
    {
        public async Task Handle(PlacingBetCommand request, CancellationToken cancellationToken)
        {
            ActiveTablesData.ManagedTablesObj[request.TableId].UserInitialBetsDictionary[request.Username] = request.Bets;
            ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.TryAdd(request.Username, true);

            if (ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.Count >= ActiveTablesData.UserCountAtTablesDictionary[request.TableId])
            {
                ActiveTablesData.ManagedTablesObj[request.TableId].cancellationTokenSource.Cancel();
                ActiveTablesData.ManagedTablesObj[request.TableId].BetsClosed = true;
            }
            else
            {
                int playersremaining = ActiveTablesData.UserCountAtTablesDictionary[request.TableId] - ActiveTablesData.ManagedTablesObj[request.TableId].AllBetsPlaced.Count;
                await request.Clients.Caller.SendAsync($"TableChatReports", "Your bets are sent. Waiting for " + playersremaining + " player(s) to start game immediately", cancellationToken: cancellationToken);
            }
            
        }
    }
}
