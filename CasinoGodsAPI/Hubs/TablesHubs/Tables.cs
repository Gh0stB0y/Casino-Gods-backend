

using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace CasinoGodsAPI.Hubs.TablesHubs
{
    public class Tables : Hub
    {
        protected readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt, redisGuestBankrolls;
        protected static Dictionary<string, string> UserGroupDictionary=new Dictionary<string, string>();
        
        public Tables(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _configuration = configuration;
            _redis = redis;
            redisDbLogin = _redis.GetDatabase(0);
            redisDbJwt = _redis.GetDatabase(1);
            redisGuestBankrolls = _redis.GetDatabase(2);;
        }
        public override async Task OnConnectedAsync()
        {
            

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
                {   new Claim("ConnectionID", Context.ConnectionId),
                    new Claim("Username", paramUName),
                    new Claim("Jwt", newJWT),
                    new Claim("Role", UserRole),
                    new Claim("Initial bankroll", bankroll),
                    new Claim("Current bankroll", bankroll)
                };
            claimsIdentity.AddClaims(Claims);
            //

           // if (LobbyHub.ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId).maxseats > LobbyHub.UserCountAtTablesDictionary[TableId])
            {
                await base.OnConnectedAsync();
               
                //DODAWANIE DO CZLONKA DO STOLOW, DODAWANIE UZYTKOWNIKA DO LICZNIKA NA STOLE
                
                //
                string report = claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Username").Value + " entered the Table";
                await Clients.Group(TableId).SendAsync("ChatReports", report);
                await Clients.Caller.SendAsync("JwtUpdate", newJWT);
                Console.WriteLine("max seats: " + LobbyHub.ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId).maxseats);
                //Console.WriteLine("UserCountDictionary: " + LobbyHub.UserCountAtTablesDictionary[TableId]);
            }
           // else { await Clients.Caller.SendAsync("Messages", newJWT);}
         
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
            string table = UserGroupDictionary[username];

            await Clients.OthersInGroup(table).SendAsync("ChatReports", report); 
            UserGroupDictionary.Remove(username);
            //LobbyHub.UserCountAtTablesDictionary[table] -= 1;
        }
        public async Task ChatMessages(string username, string message) {
           string table = UserGroupDictionary[username];
           await Clients.Group(table).SendAsync("ChatMessages", username, message); 
        }
    }
}
