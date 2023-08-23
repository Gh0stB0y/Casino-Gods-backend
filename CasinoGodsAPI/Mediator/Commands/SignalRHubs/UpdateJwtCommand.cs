using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record UpdateJwtCommand(ClaimsIdentity ClaimsIdentity, IHubCallerClients CallerClients, string OldJwt):IRequest;
}
