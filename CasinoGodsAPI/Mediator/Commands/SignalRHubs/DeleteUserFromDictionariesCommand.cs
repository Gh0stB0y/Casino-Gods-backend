using CasinoGodsAPI.Models.SignalRHubModels;
using MediatR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record DeleteUserFromDictionariesCommand(ClaimsFields ClaimsFields) : IRequest;
}
