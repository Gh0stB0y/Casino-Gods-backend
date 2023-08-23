using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record PlacingBetCommand(string Username , string TableId , List<int> Bets , IHubCallerClients Clients):IRequest;

}
