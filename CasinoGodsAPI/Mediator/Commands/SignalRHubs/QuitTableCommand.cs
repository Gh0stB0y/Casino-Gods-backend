using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record QuitTableCommand(string Username , ClaimsIdentity ClaimsIdentity , 
                                    IHubCallerClients Clients , string Jwt ,
                                      IGroupManager Groups , HubCallerContext CallerContext):IRequest;

}
