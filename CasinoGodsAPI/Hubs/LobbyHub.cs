using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Data;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using CasinoGodsAPI.Services;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.DTOs;

namespace CasinoGodsAPI.TablesModel
{
    public  class LobbyHub: Hub
    {
        protected readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt;

        public static List<ActiveTablesDB> ActiveTables;
        
        public LobbyHub(CasinoGodsDbContext CasinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _configuration = configuration;
            _redis = redis;
            redisDbLogin = _redis.GetDatabase(0);
            redisDbJwt = _redis.GetDatabase(1);
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
            if (UserRole == "Guest") bankroll = 1000.ToString();
            else bankroll = _casinoGodsDbContext.Players.Where(c => c.Username == paramUName).Select(p => p.Bankroll).FirstOrDefaultAsync().Result.ToString();
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
            TableService.UserContextDictionary.TryAdd(paramUName, Context);
            string report = claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Username").Value + " has entered the chat";
            await Clients.Others.SendAsync("ChatReports", report);
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            var bankroll = int.Parse(claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Current bankroll").Value);
            var profit = bankroll- int.Parse(claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Initial bankroll").Value);
            var role= claimsIdentity.Claims.SingleOrDefault(c => c.Type == "Role").Value;
            if (username == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND");
            else
            {
                if (role != "Guest")
                {
                    var player = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(p => p.Username == username);
                    if (player == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND IN DATABASE");
                    else { player.Bankroll = bankroll; player.Profit += profit; await _casinoGodsDbContext.SaveChangesAsync(); }
                }
            }
            string tableId;
            bool result = TableService.UserGroupDictionary.TryGetValue(username, out tableId);
            if (result)
            {
                TableService.UserContextDictionary.TryRemove(username, out _);
                TableService.UserGroupDictionary.TryRemove(username,out _);
                TableService.UserCountAtTablesDictionary[tableId]--;
                if (TableService.UserCountAtTablesDictionary[tableId] < 1) DeleteTableInstance(tableId);
            }
            else Console.WriteLine("Disconnected user was not seating at any table");
            TableService.UserContextDictionary.TryRemove(username, out _);
            string report = username + " has left the chat";
            await Clients.Others.SendAsync("ChatReports", report);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task ChatMessages(string username, string message) 
        {
            await Clients.Others.SendAsync("ChatMessages", username, message);
        }
        public async Task TableChatMessages(string username,string message)
        {  
            string tableId = TableService.UserGroupDictionary[username];
            await Clients.Group(tableId).SendAsync("TableChatMessages", username, message);
        }
        public async Task GetTableData() 
        {
            var AllActiveTables=await _casinoGodsDbContext.ActiveTables.ToListAsync();
            if (AllActiveTables.Count == 0) {
                TableService.UserCountAtTablesDictionary.Clear();await AddBasicTables(); 
                foreach (var table in ActiveTables) { TableService.AddTable(table.TableInstanceId.ToString(), table.Game); }
            }
            else{AllActiveTables = ActiveTables;}
            
            string type=GetType().Name.Replace("Lobby","");
            Console.WriteLine(type);
            if (type == "DragonTiger") type = "Dragon Tiger";
            var list = ActiveTables.Where(g => g.Game == type);
            var listToSend = new List<LobbyTableDataDTO>();
            foreach (var table in list) 
            {
                var tableObj = new LobbyTableDataDTO(table);
                tableObj.currentSeats = TableService.UserCountAtTablesDictionary[table.TableInstanceId.ToString()];
                listToSend.Add(tableObj); 
            }          
            await Clients.Caller.SendAsync("TablesData", listToSend);
        } 
        public async Task<bool> CheckFullTable(string TableId)
        {
            int CurrentUsers = TableService.UserCountAtTablesDictionary[TableId];
            int MaxUsers =  ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId).Maxseats;    
            if (MaxUsers != default)
            {
                if (CurrentUsers < MaxUsers) return true; 
                else return false;
            }
            throw new HubException("Data error");
        }
        public async Task AddBasicTables()
        {
            var NewTables = await _casinoGodsDbContext.TablesList.ToListAsync();
            var list=new List<ActiveTablesDB>();
            foreach (var table in NewTables) 
            {
                var TableInstance = new ActiveTablesDB() { 
                
                    TableInstanceId = Guid.NewGuid(),
                    Name = table.CKname,
                    Game = table.CKGame,
                    MinBet = table.MinBet,
                    MaxBet = table.MaxBet,
                    BetTime = table.BetTime,
                    Maxseats = table.Maxseats,
                    ActionTime = table.ActionTime,
                    Sidebet1 = table.Sidebet1,
                    Sidebet2 = table.Sidebet2,
                    Decks = table.Decks
                };
                list.Add(TableInstance);                               
            }    
            foreach (var activeTable in list) { await _casinoGodsDbContext.ActiveTables.AddAsync(activeTable); }
            ActiveTables = list;
            await _casinoGodsDbContext.SaveChangesAsync();
        }   
        public async Task<bool> EnterTable(string TableId, string jwt)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); return false; }
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var JwtClaim = claimsIdentity.FindFirst("Jwt");
            if (JwtClaim != null)
            {
                var newJwtClaim = new Claim(JwtClaim.Type, newJWT);
                claimsIdentity.RemoveClaim(JwtClaim);
                claimsIdentity.AddClaim(newJwtClaim);
            }
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            if (!TableService.UserGroupDictionary.ContainsKey(username))
            {
                bool TableNotFull;
                try { TableNotFull = await CheckFullTable(TableId); }
                catch (HubException ex) { Console.WriteLine(ex.Message); return false; }
                if (TableNotFull)
                {
                    if (TableService.UserCountAtTablesDictionary[TableId] < 1 && !TableService.ManagedTablesObj[TableId].IsActive) { TableService.MakeTableActive(TableId); }
                    TableService.UserGroupDictionary.TryAdd(username, TableId);
                    TableService.UserCountAtTablesDictionary[TableId]++;
                    TableService.ManagedTablesObj[TableId].AllBetsPlaced.TryAdd(username, false);
                    await Groups.AddToGroupAsync(Context.ConnectionId, TableId);
                    string tableReport = username + " has entered the table";
                    await Clients.OthersInGroup(TableId).SendAsync("TableChatReports", tableReport);

                }
                else { await Clients.Caller.SendAsync("ErrorMessages", "Table is full"); }

                var Table = ActiveTables.SingleOrDefault(i => i.TableInstanceId.ToString() == TableId);
                var SameTypeInstances = ActiveTables.Where(t => t.Game == Table.Game && t.Name == Table.Name).ToList();
                if (SameTypeInstances.Where(t => t.Maxseats > TableService.UserCountAtTablesDictionary[t.TableInstanceId.ToString()]).ToList().Count<1) AddNewTableInstance(TableId);
                return TableNotFull;
            }
            else
            {
                await Clients.Caller.SendAsync("ErrorMessages", "This user is already playing at the table");
                return false;
            }
        }
        private static void AddNewTableInstance(string TableId)
        {
            var ParentTable = ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId);
            if (ParentTable != null)
            {
                var ChildTable = new ActiveTablesDB()
                {
                    TableInstanceId = Guid.NewGuid(),
                    Name = ParentTable.Name,
                    Game = ParentTable.Game,
                    MinBet = ParentTable.MinBet,
                    MaxBet = ParentTable.MaxBet,
                    BetTime = ParentTable.BetTime,
                    Maxseats = ParentTable.Maxseats,
                    ActionTime = ParentTable.ActionTime,
                    Sidebet1 = ParentTable.Sidebet1,
                    Sidebet2 = ParentTable.Sidebet2,
                    Decks = ParentTable.Decks
                };
                TableService.UserCountAtTablesDictionary.TryAdd(ChildTable.TableInstanceId.ToString(), 0);
                ActiveTables.Add(ChildTable);
                TableService.AddTable(ChildTable.TableInstanceId.ToString(), ChildTable.Game);
            }
            else Console.WriteLine("Parent Table not found");
        }
        public static void DeleteTableInstance(string TableId)
        {
            var Table = ActiveTables.SingleOrDefault(i => i.TableInstanceId.ToString() == TableId);
            if (Table != null)
            {
                bool AllTablesFull = true;
                var SameTypeInstances = ActiveTables.Where(t => t.Game == Table.Game && t.Name == Table.Name).ToList();
                int count = 0;
                foreach (var Instance in SameTypeInstances) {
                    if (TableService.UserCountAtTablesDictionary[Instance.TableInstanceId.ToString()] < Instance.Maxseats) count++;
                }
                if(count>1)AllTablesFull = false;
                if (SameTypeInstances.Count > 1 && AllTablesFull == false)
                {
                    //TableService.UserCountAtTablesDictionary.TryRemove(TableId,out _);
                    ActiveTables.Remove(Table);
                    TableService.DeleteTable(TableId);

                    var UsersOfDeletedTable = TableService.UserGroupDictionary.Where(u => u.Value == TableId).ToList();
                    if (UsersOfDeletedTable.Count > 0)
                    {
                        foreach (var User in UsersOfDeletedTable)
                        {
                            TableService.UserGroupDictionary.TryRemove(User.Key,out _);
                        }
                    }
                }
                else 
                {
                    TableService.MakeTableInactive(TableId);
                }
            }
        }
        public async Task QuitTable(string jwt)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); return; }

            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var JwtClaim = claimsIdentity.FindFirst("Jwt");
            if (JwtClaim != null)
            {
                var newJwtClaim = new Claim(JwtClaim.Type, newJWT);
                claimsIdentity.RemoveClaim(JwtClaim);
                claimsIdentity.AddClaim(newJwtClaim);
            }
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;

            string tableId = TableService.UserGroupDictionary[username];
            TableService.UserGroupDictionary.TryRemove(username, out _);
            TableService.UserCountAtTablesDictionary[tableId]--;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tableId);
            //if (TableService.UserCountAtTablesDictionary[tableId] < 1){ TableService.MakeTableInactive(tableId); DeleteTableInstance(tableId);}
            string tableReport = username + " has left the table";
            await Clients.OthersInGroup(tableId).SendAsync("TableChatReports", tableReport);
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);
        }
        public static int GetCurrentPlayers(string id)
        {
            return 0;
            //return TableService.UserCountAtTablesDictionary[id];
        }
        public async Task PlaceBet(string jwt, string TableId,List<int>bets)
        {
            
        }
        public async Task TakeASeat(string jwt,int seatID)
        {
           
        }
        public async Task<bool> IsBettingEnabled() { return true; }
        public async Task ToggleBetting(bool enabled, string TableId) { await Clients.Group(TableId).SendAsync("ToggleBetting", enabled); }
        public async Task SendBets(List<int> Bets,string jwt,string ClosedBetsToken)
        {
            bool JwtValid = await GlobalFunctions.LookForJWTGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if(!JwtValid) await Clients.Caller.SendAsync("Disconnect", "Session expired, log in again");
           
            else
            {
                var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
                var UsernameClaim = claimsIdentity.FindFirst("Username");
                if(UsernameClaim == null) await Clients.Caller.SendAsync("Disconnect", "Claims Error");
                else
                {
                    string TableId = TableService.UserGroupDictionary[UsernameClaim.Value];
                    if (TableService.ManagedTablesObj[TableId].BetsClosed == false)
                    {
                        if (!TableService.ManagedTablesObj[TableId].IsBettingListVoid(Bets)&& !TableService.ManagedTablesObj[TableId].AllBetsPlaced.ContainsKey(UsernameClaim.Value)) //bet are open, and betting list is valid
                        {
                            //var bankrollAvailable = await _casinoGodsDbContext.Players.FirstOrDefaultAsync(u => u.username == UsernameClaim.Value);
                            //if (bankrollAvailable != null)
                            //{
                                //if (bankrollAvailable.bankroll >= Bets.Sum()) 
                                    Console.WriteLine(UsernameClaim.Value + " List uploaded");
                                    TableService.ManagedTablesObj[TableId].UserInitialBetsDictionary[UsernameClaim.Value] = Bets;
                                    TableService.ManagedTablesObj[TableId].AllBetsPlaced.TryAdd(UsernameClaim.Value, true);
                                    string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
                                    if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); }
                                    await Clients.Caller.SendAsync("JwtUpdate", newJWT);
                                    var JwtClaim = claimsIdentity.FindFirst("Jwt");
                                    if (JwtClaim != null)
                                    {
                                        var newJwtClaim = new Claim(JwtClaim.Type, newJWT);
                                        claimsIdentity.RemoveClaim(JwtClaim);
                                        claimsIdentity.AddClaim(newJwtClaim);
                                    }
                                    if (TableService.ManagedTablesObj[TableId].AllBetsPlaced.Count >= TableService.UserCountAtTablesDictionary[TableId])
                                    { TableService.ManagedTablesObj[TableId].cancellationTokenSource.Cancel(); TableService.ManagedTablesObj[TableId].BetsClosed = true; }
                                    else
                                    {
                                        int playersremaining = TableService.UserCountAtTablesDictionary[TableId] - TableService.ManagedTablesObj[TableId].AllBetsPlaced.Count;
                                        await Clients.Caller.SendAsync($"TableChatReports", "Your bets are sent. Waiting for " + playersremaining + " player(s) to start game immediately");
                                    }
                                
                                //else Console.WriteLine("Not enough cash");
                            //}
                            //else Console.WriteLine("Bankroll from Database error");
                        }
                        else if (!TableService.ManagedTablesObj[TableId].IsBettingListVoid(Bets) && TableService.ManagedTablesObj[TableId].AllBetsPlaced.ContainsKey(UsernameClaim.Value))
                        { Console.WriteLine("Bets already placed");}
                        else Console.WriteLine("Empty List was sent to me");                       
                    }
                    else
                    {
                        if (TableService.ManagedTablesObj[TableId].ClosedBetsToken == ClosedBetsToken)
                        {
                            if (!TableService.ManagedTablesObj[TableId].IsBettingListVoid(Bets) && !TableService.ManagedTablesObj[TableId].AllBetsPlaced.ContainsKey(UsernameClaim.Value)) //bets are closed 
                            {
                                Console.WriteLine(UsernameClaim.Value + "List automatically uploaded");

                                TableService.ManagedTablesObj[TableId].UserInitialBetsDictionary[UsernameClaim.Value] = Bets;
                                string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
                                if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); }
                                await Clients.Caller.SendAsync("JwtUpdate", newJWT);
                                var JwtClaim = claimsIdentity.FindFirst("Jwt");
                                if (JwtClaim != null)
                                {
                                    var newJwtClaim = new Claim(JwtClaim.Type, newJWT);
                                    claimsIdentity.RemoveClaim(JwtClaim);
                                    claimsIdentity.AddClaim(newJwtClaim);
                                }
                            }
                            else if (!TableService.ManagedTablesObj[TableId].IsBettingListVoid(Bets) && TableService.ManagedTablesObj[TableId].AllBetsPlaced.ContainsKey(UsernameClaim.Value))
                            { Console.WriteLine("Bets already placed"); }
                            else { Console.WriteLine("Empty List"); TableService.ManagedTablesObj[TableId].UserInitialBetsDictionary.TryAdd(UsernameClaim.Value, Enumerable.Repeat(0, 157).ToList()); }
                        }
                        else { Console.WriteLine("Wrong ClosedBets Token"); }
                    }
                }
            }
        }
    }
    public class BacarratLobby :  LobbyHub
    {
        public BacarratLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
    }
    public class BlackjackLobby : LobbyHub
    {
        public BlackjackLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
    }
    public class DragonTigerLobby : LobbyHub
    {
        public DragonTigerLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
    }
    public class RouletteLobby : LobbyHub
    {
        public RouletteLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
    }
    public class WarLobby : LobbyHub
    {
        public WarLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
    }
}
