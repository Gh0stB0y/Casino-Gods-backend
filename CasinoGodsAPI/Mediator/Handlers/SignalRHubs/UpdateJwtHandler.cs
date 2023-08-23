using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Services;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class UpdateJwtHandler : IRequestHandler<UpdateJwtCommand>
    {
        public async Task Handle(UpdateJwtCommand request, CancellationToken cancellationToken)
        {
            string newJWT = await LobbyService.RefreshTokenGlobal(request.OldJwt);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again")
            {
                await request.CallerClients.Caller.SendAsync("Disconnect", newJWT);
            }
            else
            {
                var JwtClaim = request.ClaimsIdentity.Claims.FirstOrDefault(j => j.Type == "Jwt");
                if (JwtClaim != null)
                {
                    var newJwtClaim = new Claim(JwtClaim.Type, newJWT);
                    request.ClaimsIdentity.RemoveClaim(JwtClaim);
                    request.ClaimsIdentity.AddClaim(newJwtClaim);
                }

                await request.CallerClients.Caller.SendAsync("JwtUpdate", newJWT, cancellationToken: cancellationToken);
            }
            
        }
    }
}
