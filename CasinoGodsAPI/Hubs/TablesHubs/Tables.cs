

using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class Tables : Hub
    {
        protected readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt, redisGuestBankrolls;

        public Tables(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _configuration = configuration;
            _redis = redis;
            redisDbLogin = _redis.GetDatabase(0);
            redisDbJwt = _redis.GetDatabase(1);
            redisGuestBankrolls = _redis.GetDatabase(2);
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var query = Context.GetHttpContext().Request.Query;
            string paramJWT = query["param1"].ToString();
            string paramUName = query["param2"].ToString();
            string TableId = query["param3"].ToString();

            
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(paramJWT, paramUName, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") await Clients.Caller.SendAsync("Disconnect", newJWT);
            
            //DODANIE CLAIMOW DO UZYTKOWNIKA
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(newJWT);
            string UserRole = jwtSecurityToken.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value;
            string bankroll;
            if (UserRole == "Guest") bankroll = await redisGuestBankrolls.StringGetAsync(paramUName);
            else bankroll = _casinoGodsDbContext.Players.Where(c => c.username == paramUName).Select(p => p.bankroll).FirstOrDefaultAsync().Result.ToString();
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var Claims = new List<Claim>()
            { new Claim("ConnectionID", Context.ConnectionId),
              new Claim("Username", paramUName),
              new Claim("Jwt", newJWT),
              new Claim("Role", UserRole),
              new Claim("Initial bankroll", bankroll),
              new Claim("Current bankroll", bankroll)
            };
            claimsIdentity.AddClaims(Claims);
            //
            await Groups.AddToGroupAsync(Context.ConnectionId, TableId);
            string report = claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Username").Value + " entered the Table";
            await Clients.Group(TableId).SendAsync("ChatReports", report);
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var bankroll = int.Parse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Current bankroll").Value);
            var profit = bankroll - int.Parse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Initial bankroll").Value);
            var role = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Role").Value;
            if (username == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND");
            else
            {
                if (role != "Guest")
                {
                    var player = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(p => p.username == username);
                    if (player == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND IN DATABASE");
                    else { player.bankroll = bankroll; player.profit += profit; await _casinoGodsDbContext.SaveChangesAsync();}
                }
            }
            string report = username + " left the Table";
            await Clients.Others.SendAsync("ChatReports", report);
        }
        public async Task ChatMessages(string username, string message, string jwt) { await Clients.All.SendAsync("ChatMessages", username, message); }
    }
}
