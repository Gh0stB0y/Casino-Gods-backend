using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class AddInitialClaimsHandler : IRequestHandler<AddInitialClaimsCommand, List<Claim>>
    {
        private readonly ILogger<AddInitialClaimsHandler> _logger;

        public AddInitialClaimsHandler(ILogger<AddInitialClaimsHandler> logger)
        {
            _logger = logger;
        }
        public async Task<List<Claim>> Handle(AddInitialClaimsCommand request, CancellationToken cancellationToken)
        {
            string bankroll;
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(request.Jwt);
            string UserRole = jwtSecurityToken.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;

            if (UserRole == "Guest") bankroll = 1000.ToString();
            else
            {
                var obj = await request.CasinoGodsDbContext.Players.Where(c => c.Username == request.UName)
                                .Select(p => p.Bankroll).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (obj == null)
                {
                    _logger.LogCritical("Bankroll of user not found");
                    return null;
                }
                else
                {
                    bankroll = obj.ToString();
                }
            }
            List<Claim> Claims = new()
            { 
                new Claim("ConnectionID", request.ConnectionId),
                new Claim("Username", request.UName),
                new Claim("Jwt", request.Jwt),
                new Claim("Role", UserRole),
                new Claim("Initial bankroll", bankroll),
                new Claim("Current bankroll", bankroll)
            };
            return Claims;
        }
    }
}
