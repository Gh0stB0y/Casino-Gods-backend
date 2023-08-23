using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record EnterTableCommand(string TableId , string Username, IHubCallerClients Clients, IGroupManager Groups, HubCallerContext Context) : IRequest<bool>;
}
