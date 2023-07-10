using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.ExceptionServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Tasks;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Data;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Hubs;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using CasinoGodsAPI.TablesModel;
using CasinoGodsAPI.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CasinoGodsAPI.TablesModel
{
    public  class LobbyHub: Hub
    {
        protected readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt, redisGuestBankrolls;
        public static List<ActiveTablesDatabase>ActiveTables;
        public LobbyHub(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
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
            
            string newJWT=await GlobalFunctions.RefreshTokenGlobal(paramJWT,paramUName, redisDbLogin, redisDbJwt, _configuration); 
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again")await Clients.Caller.SendAsync("Disconnect",newJWT);

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
            string report = claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Username").Value + " entered the chat";
            await Clients.Others.SendAsync("ChatReports", report);
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var bankroll = int.Parse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Current bankroll").Value);
            var profit = bankroll- int.Parse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Initial bankroll").Value);
            var role= claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Role").Value;
            if (username == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND");
            else
            {
                if (role != "Guest")
                {
                    var player = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(p => p.username == username);
                    if (player == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND IN DATABASE");
                    else { player.bankroll = bankroll; player.profit += profit; await _casinoGodsDbContext.SaveChangesAsync(); }
                }
            }
            string report = username + " left the chat";
            await Clients.Others.SendAsync("ChatReports", report);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task ChatMessages(string username, string message) {await Clients.All.SendAsync("ChatMessages", username, message);}
        public async Task GetTableData() { 

            var AllActiveTables=await _casinoGodsDbContext.ActiveTables.ToListAsync();
            if (AllActiveTables.Count == 0) AllActiveTables=await AddBasicTables();
            ActiveTables = AllActiveTables;
            string type=this.GetType().Name.Replace("Lobby","");
            var list = AllActiveTables.Where(g => g.Game == type);
            var listToSend = new List<LobbyTableDataDTO>();
            foreach (var table in list) listToSend.Add(new LobbyTableDataDTO(table));
            
            await Clients.Caller.SendAsync("TablesData", listToSend);
        }

        public async Task<List<ActiveTablesDatabase>> AddBasicTables()
        {
            string Path = "Tables1";
            var NewTables = await _casinoGodsDbContext.TablesList.ToListAsync();
            var list=new List<ActiveTablesDatabase>();
            foreach (var table in NewTables) 
            {
                list.Add(new ActiveTablesDatabase
                {
                    TableInstanceId = Guid.NewGuid(),
                    TablePath = "/" + table.CKGame + Path,
                    Name = table.CKname,
                    Game = table.CKGame,
                    minBet = table.minBet,
                    maxBet = table.maxBet,
                    betTime = table.betTime,
                    maxseats = table.maxseats,
                    actionTime = table.actionTime,
                    sidebet1 = table.sidebet1,
                    sidebet2 = table.sidebet2,
                    decks = table.decks
                });
            }
            foreach(var activeTable in list) { await _casinoGodsDbContext.ActiveTables.AddAsync(activeTable); }
            await _casinoGodsDbContext.SaveChangesAsync();
            return list;
        }         
    }
    public class BacarratLobby :  LobbyHub
    {
        public BacarratLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis){}


    }
    public class BlackjackLobby : LobbyHub
    {
        public new static Dictionary<string, int> HubsTablesCount = new Dictionary<string, int>();
        public new static Dictionary<LobbyTableData, string> TablesAssignedToHub = new Dictionary<LobbyTableData, string>();
        public BlackjackLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {
       
        }

    }
    public class DragonTigerLobby : LobbyHub
    {
        public new static Dictionary<string, int> HubsTablesCount = new Dictionary<string, int>();
        public new static Dictionary<LobbyTableData, string> TablesAssignedToHub = new Dictionary<LobbyTableData, string>();

        public DragonTigerLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {
         
            
        }

    }
    public class RouletteLobby : LobbyHub
    {
        public new static Dictionary<string, int> HubsTablesCount = new Dictionary<string, int>();
        public new static Dictionary<LobbyTableData, string> TablesAssignedToHub = new Dictionary<LobbyTableData, string>();

        public RouletteLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {
      
        }

    }
    public class WarLobby : LobbyHub
    {
        public new static Dictionary<string, int> HubsTablesCount = new Dictionary<string, int>();
        public new static Dictionary<LobbyTableData, string> TablesAssignedToHub = new Dictionary<LobbyTableData, string>();

        public WarLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {

        }

    }
}
