using CasinoGodsAPI.Data;
using MediatR;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record AddInitialClaimsCommand(CasinoGodsDbContext CasinoGodsDbContext,string ConnectionId,string Jwt,string UName):IRequest<List<Claim>>;
}
