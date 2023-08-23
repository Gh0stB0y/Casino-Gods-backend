using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record ManualBetSendCommand(string Username , string TableId , List<int> Bets ,
                                        string Jwt , ClaimsIdentity ClaimsIdentity , IHubCallerClients Clients):IRequest;
}
