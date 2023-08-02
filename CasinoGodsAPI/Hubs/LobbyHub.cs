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

using System;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;

namespace CasinoGodsAPI.TablesModel
{
    public  class LobbyHub: Hub
    {
        protected readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase redisDbLogin, redisDbJwt, redisGuestBankrolls;


        public static List<ActiveTablesDatabase>ActiveTables;
        public static ConcurrentDictionary<string, ManageTableClass> ActiveTablesObj=new ConcurrentDictionary<string, ManageTableClass>();
        protected static Dictionary<string, int> UserCountAtTablesDictionary = new Dictionary<string, int>();
        protected static Dictionary<string, string> UserGroupDictionary = new Dictionary<string, string>();

        /*public static ConcurrentDictionary<string,bool> ExistingTableThreads = new ConcurrentDictionary<string, bool>();
        public static ConcurrentDictionary<string, ManualResetEventSlim> IsTableActive = new ConcurrentDictionary<string, ManualResetEventSlim>();*/
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
                    var player = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(p => p.username == username);
                    if (player == null) Console.WriteLine("CRITICAL ERROR! USERNAME NOT FOUND IN DATABASE");
                    else { player.bankroll = bankroll; player.profit += profit; await _casinoGodsDbContext.SaveChangesAsync(); }
                }
            }
            string tableId;
            bool result = UserGroupDictionary.TryGetValue(username, out tableId);
            if (result)
            {
                UserGroupDictionary.Remove(username);
                UserCountAtTablesDictionary[tableId]--;
                if (UserCountAtTablesDictionary[tableId] < 1) DeleteTableInstance(tableId);
            }
            else Console.WriteLine("Disconnected user was not seating at any table");
            string report = username + " has left the chat";
            await Clients.Others.SendAsync("ChatReports", report);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task ChatMessages(string username, string message) {await Clients.Others.SendAsync("ChatMessages", username, message);}
        public async Task TableChatMessages(string username,string message)
        {  
            string tableId = UserGroupDictionary[username];
            await Clients.Group(tableId).SendAsync("TableChatMessages", username, message);
        }
        public async Task GetTableData() { 

            var AllActiveTables=await _casinoGodsDbContext.ActiveTables.ToListAsync();
            if (AllActiveTables.Count == 0) { 
                UserCountAtTablesDictionary.Clear(); AllActiveTables = await AddBasicTables(); 

                foreach (var table in AllActiveTables) UserCountAtTablesDictionary.Add(table.TableInstanceId.ToString(), 0);
                ActiveTables = AllActiveTables;
                foreach(var table in ActiveTables)
                {
                    ActiveTablesObj.TryAdd(table.TableInstanceId.ToString(), new ManageTableClass(table.TableInstanceId.ToString()));
                }
            }
            else
            {
                AllActiveTables = ActiveTables;
            }
            
            string type=GetType().Name.Replace("Lobby","");
            var list = AllActiveTables.Where(g => g.Game == type);
            var listToSend = new List<LobbyTableDataDTO>();
            foreach (var table in list) 
            {
                var tableObj = new LobbyTableDataDTO(table);
                tableObj.currentSeats = UserCountAtTablesDictionary[table.TableInstanceId.ToString()];
                listToSend.Add(tableObj); 
            }          
            await Clients.Caller.SendAsync("TablesData", listToSend);
        } 
        public async Task<bool> CheckFullTable(string TableId)
        {
            int CurrentUsers = UserCountAtTablesDictionary[TableId];
            int MaxUsers =  ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId).maxseats;    
            if (MaxUsers != default)
            {
                if (CurrentUsers < MaxUsers) return true; 
                else return false;
            }
            throw new HubException("Data error");
        }
        public async Task<List<ActiveTablesDatabase>> AddBasicTables()
        {
            var NewTables = await _casinoGodsDbContext.TablesList.ToListAsync();
            var list=new List<ActiveTablesDatabase>();
            foreach (var table in NewTables) 
            {
                var TableInstance = new ActiveTablesDatabase() { 
                
                    TableInstanceId = Guid.NewGuid(),
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
                };
                list.Add(TableInstance);
                /*ExistingTableThreads.TryAdd(TableInstance.TableInstanceId.ToString(), true);*/
            }
            foreach (var activeTable in list) { await _casinoGodsDbContext.ActiveTables.AddAsync(activeTable); }
            await _casinoGodsDbContext.SaveChangesAsync();
            return list;
        }   
        public async Task<bool> EnterTable(string TableId, string jwt)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); return false; }
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            if (!UserGroupDictionary.ContainsKey(username))
            {
                bool TableNotFull;
                try { TableNotFull = await CheckFullTable(TableId); }
                catch (HubException ex) { Console.WriteLine(ex.Message); return false; }
                if (TableNotFull)
                {
                    /*if (UserCountAtTablesDictionary[TableId] < 1 && IsTableActive.ContainsKey(TableId)) { MakeTableActive(TableId); }
                    if (UserCountAtTablesDictionary[TableId] < 1 && !IsTableActive.ContainsKey(TableId))
                    {
                        await ManageTable(TableId, true);
                    }*/
                    UserGroupDictionary.Add(username, TableId);
                    UserCountAtTablesDictionary[TableId]++;
                    await Groups.AddToGroupAsync(Context.ConnectionId, TableId);
                    string tableReport = username + " has entered the table";
                    await Clients.OthersInGroup(TableId).SendAsync("TableChatReports", tableReport);

                    
                }
                else { await Clients.Caller.SendAsync("ErrorMessages", "Table is full"); }

                var Table = ActiveTables.SingleOrDefault(i => i.TableInstanceId.ToString() == TableId);
                var SameTypeInstances = ActiveTables.Where(t => t.Game == Table.Game && t.Name == Table.Name).ToList();
                if (SameTypeInstances.Where(t => t.maxseats > UserCountAtTablesDictionary[t.TableInstanceId.ToString()]).ToList().Count<1) AddNewTableInstance(TableId);
                return TableNotFull;
            }
            else
            {
                await Clients.Caller.SendAsync("ErrorMessages", "This user is already playing at the table");
                return false;
            }
        }
        public virtual async Task AdditionalSetup(string username, string TableId) { }
        private void AddNewTableInstance(string TableId)
        {
            var ParentTable = ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId);
            if (ParentTable != null)
            {
                var ChildTable = new ActiveTablesDatabase()
                {
                    TableInstanceId = Guid.NewGuid(),
                    Name = ParentTable.Name,
                    Game = ParentTable.Game,
                    minBet = ParentTable.minBet,
                    maxBet = ParentTable.maxBet,
                    betTime = ParentTable.betTime,
                    maxseats = ParentTable.maxseats,
                    actionTime = ParentTable.actionTime,
                    sidebet1 = ParentTable.sidebet1,
                    sidebet2 = ParentTable.sidebet2,
                    decks = ParentTable.decks
                };
                UserCountAtTablesDictionary.Add(ChildTable.TableInstanceId.ToString(), 0);
                ActiveTables.Add(ChildTable);

                /*ExistingTableThreads.TryAdd(ChildTable.TableInstanceId.ToString(), true);
                ManageTable(ChildTable.TableInstanceId.ToString(),false);*/
            }
            else Console.WriteLine("Parent Table not found");
        }
        private void DeleteTableInstance(string TableId)
        {
            var Table = ActiveTables.SingleOrDefault(i => i.TableInstanceId.ToString() == TableId);
            if (Table != null)
            {
                bool AllTablesFull = true;
                var SameTypeInstances = ActiveTables.Where(t => t.Game == Table.Game && t.Name == Table.Name).ToList();
                int count = 0;
                foreach (var Instance in SameTypeInstances) {
                    if (UserCountAtTablesDictionary[Instance.TableInstanceId.ToString()] < Instance.maxseats) count++;
                }
                if(count>1)AllTablesFull = false;
                if (SameTypeInstances.Count > 1 && AllTablesFull == false)
                {
                    UserCountAtTablesDictionary.Remove(TableId);
                    ActiveTables.Remove(Table);

                    /*ExistingTableThreads[TableId] = false;*/

                    var UsersOfDeletedTable = UserGroupDictionary.Where(u => u.Value == TableId).ToList();
                    if (UsersOfDeletedTable.Count > 0)
                    {
                        foreach (var User in UsersOfDeletedTable)
                        {
                            UserGroupDictionary.Remove(User.Key);
                        }
                    }
                }
                else { }//MakeTableInactive(TableId);
            }
        }
        public async Task QuitTable(string jwt)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); return; }

            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;

            string tableId = UserGroupDictionary[username];
            UserGroupDictionary.Remove(username);
            UserCountAtTablesDictionary[tableId]--;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, tableId);
            if(UserCountAtTablesDictionary[tableId]<1) DeleteTableInstance(tableId);
            string tableReport = username + " has left the table";
            await Clients.OthersInGroup(tableId).SendAsync("TableChatReports", tableReport);
            await Clients.Caller.SendAsync("JwtUpdate", newJWT);
        }
        public static int GetCurrentPlayers(string id)
        {
            return UserCountAtTablesDictionary[id];
        }
        public async Task PlaceBet(string jwt, string TableId,List<int>bets)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") await Clients.Caller.SendAsync("Disconnect", newJWT);
        }
        public async Task TakeASeat(string jwt,int seatID)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") await Clients.Caller.SendAsync("Disconnect", newJWT);
        }
        /*protected async Task ManageTable(string TableId,bool ActiveImmediately)
        {
            if (!IsTableActive.ContainsKey(TableId)) IsTableActive[TableId] = new ManualResetEventSlim(ActiveImmediately); // Initialize as active

            ActiveTablesObj.TryAdd(TableId, new ManageTableClass(TableId));            
            if (ActiveTablesObj.ContainsKey(TableId))
            {
                while (ExistingTableThreads[TableId]) //MAIN LOOP
                {
                    IsTableActive[TableId].Wait();
                    foreach (var seat in ActiveTablesObj[TableId].UsersAtSeat) Console.WriteLine("Index: " + seat.Key + ", Value: " + seat.Value);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await Clients.Group(TableId).SendAsync("DUPA");


                    //await Task.Run(async () => await BettingPhase(ActiveTablesObj[TableId]));
                    //Task newTask = Task.Run(async ()=>await BettingPhase(ActiveTablesObj[TableId]));
                    //await BettingPhase(ActiveTablesObj[TableId]);
                    //await PlayingPhase(ActiveTablesObj[TableId]);
                    //await ResultsPhase(ActiveTablesObj[TableId]);
                }
                //
                if (!ExistingTableThreads[TableId]) //CLEANING
                {
                    // akcje do wykonania przy cancellu
                    Console.WriteLine("Table Thread is deleted");
                    ExistingTableThreads.TryRemove(TableId, out _);
                    IsTableActive.TryRemove(TableId, out _);
                    ActiveTablesObj.TryRemove(TableId, out _);
                }
            }
        }*/
        /*public void MakeTableInactive(string TableId)
        {
            if (IsTableActive.ContainsKey(TableId))
            {
                IsTableActive[TableId].Reset();
            }
        }
        public void MakeTableActive(string TableId)
        {
            if (IsTableActive.ContainsKey(TableId))
            {
                IsTableActive[TableId].Set();
            }
        }*/
        protected virtual async Task BettingPhase(ManageTableClass TableObj) { }
        protected virtual async Task PlayingPhase(ManageTableClass TableObj) { }
        protected virtual async Task ResultsPhase(ManageTableClass TableObj) { }
        public async Task<bool> IsBettingEnabled() { return true; }
        public async Task ToggleBetting(bool enabled,string TableId) { await Clients.Group(TableId).SendAsync("ToggleBetting", enabled); }
        public async Task SendBets(string jwt,List<int> Bets)
        {
            string newJWT = await GlobalFunctions.RefreshTokenGlobal(jwt, redisDbLogin, redisDbJwt, _configuration);
            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again") { await Clients.Caller.SendAsync("Disconnect", newJWT); }
            else
            {
                await Clients.Caller.SendAsync("JwtUpdate", newJWT);
                var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
                var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
                string TableId = UserGroupDictionary[username];

                List<int>newBets= await ConvertBettingLists(Bets);
                int index = ActiveTablesObj[TableId].UsersAtSeat.FirstOrDefault(u => u.Value == username).Key;
                ActiveTablesObj[TableId].BetAtSeat[index]=newBets;
                ActiveTablesObj[TableId].SeatActiveInCurrentGame[index] = true;
            }
        }
        public virtual async Task<List<int>> ConvertBettingLists(List<int> Bets) { return Bets; }
    }
    public class BacarratLobby :  LobbyHub
    {

        public BacarratLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
        protected override async Task BettingPhase(ManageTableClass TableObj) { Console.WriteLine("Bacarrat faza1"); }
        protected override async Task PlayingPhase(ManageTableClass TableObj) { Console.WriteLine("Bacarrat faza2"); }
        protected override async Task ResultsPhase(ManageTableClass TableObj) 
        { 
            Console.WriteLine("Bacarrat faza3");
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
    public class BlackjackLobby : LobbyHub
    {
        public BlackjackLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
        protected override async Task BettingPhase(ManageTableClass TableObj) { Console.WriteLine("Blackjack faza1"); }
        protected override async Task PlayingPhase(ManageTableClass TableObj) { Console.WriteLine("Blackjack faza2"); }
        protected override async Task ResultsPhase(ManageTableClass TableObj) 
        { 
            Console.WriteLine("Blackjack faza3");
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
    public class DragonTigerLobby : LobbyHub
    {
        public DragonTigerLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
        protected override async Task BettingPhase(ManageTableClass TableObj) { Console.WriteLine("Dragon Tiger faza1"); }
        protected override async Task PlayingPhase(ManageTableClass TableObj) { Console.WriteLine("Dragon Tiger faza2"); }
        protected override async Task ResultsPhase(ManageTableClass TableObj)
        { 
            Console.WriteLine("Dragon Tiger faza3"); 
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
    public class RouletteLobby : LobbyHub
    {
        public RouletteLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
        public override async Task AdditionalSetup(string username, string TableId)
        {
       
            var SeatToBeTaken = ActiveTablesObj[TableId].UsersAtSeat.FirstOrDefault(u => u.Value == "Empty Seat").Key;
            if (SeatToBeTaken >=0)
            {
                ActiveTablesObj[TableId].UsersAtSeat[SeatToBeTaken] = username;
                ActiveTablesObj[TableId].SeatsTakenByUser.TryAdd(username, 1);
                ActiveTablesObj[TableId].SeatActiveInCurrentGame[SeatToBeTaken] = true;
            }
            else await Clients.Caller.SendAsync("ErrorMessages", "No empty seat on given table.");
        }
        public override async Task<List<int>> ConvertBettingLists(List<int> Bets)
        {
            var list = new List<int>();
            return list;
        }
        protected override async Task BettingPhase(ManageTableClass TableObj) 
        {
            Console.WriteLine("dupa");
            await ChatMessages("user", "message");
            Console.WriteLine("CHUJ");
        }
        protected override async Task PlayingPhase(ManageTableClass TableObj) 
        { 
            Console.WriteLine("Roulette faza2"); 
        }
        protected override async Task ResultsPhase(ManageTableClass TableObj) 
        { 
            Console.WriteLine("Roulette faza3");
        }
    }
    public class WarLobby : LobbyHub
    {
        public WarLobby(CasinoGodsDbContext casinoGodsDbContext, IConfiguration configuration, IConnectionMultiplexer redis)
        : base(casinoGodsDbContext, configuration, redis) {}
        protected override async Task BettingPhase(ManageTableClass TableObj) { Console.WriteLine("War faza1"); }
        protected override async Task PlayingPhase(ManageTableClass TableObj) { Console.WriteLine("War faza2"); }
        protected override async Task ResultsPhase(ManageTableClass TableObj) 
        {
            Console.WriteLine("War faza3");
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
