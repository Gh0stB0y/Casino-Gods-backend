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
using CasinoGodsAPI.Data_Containers;
using MediatR;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Models.SignalRHubModels;
using CasinoGodsAPI.Mediator.Queries.BackgroundServices;

namespace CasinoGodsAPI.TablesModel
{
    public  class LobbyHub: Hub
    {
        protected readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IMediator _mediator;
        private readonly ILogger<LobbyHub> _logger;

        public LobbyHub(CasinoGodsDbContext CasinoGodsDbContext, IMediator mediator, ILogger<LobbyHub> logger)
        {
            _casinoGodsDbContext = CasinoGodsDbContext;
            _mediator = mediator;
            _logger = logger;
        }        
        public override async Task OnConnectedAsync()
        {
            //await base.OnConnectedAsync();
            var query = Context.GetHttpContext().Request.Query;
            string paramJWT = query["param1"].ToString();
            string paramUName = query["param2"].ToString();            
            //string newJWT=await RefreshToken.RefreshTokenGlobal(paramJWT,paramUName, redisDbLogin, redisDbJwt, _configuration);
            string newJWT = await LobbyService.RefreshTokenGlobal(paramJWT, paramUName);

            if (newJWT == null || newJWT == "Session expired, log in again" || newJWT == "Redis data error, log in again")
            { 
                await Clients.Caller.SendAsync("Disconnect", newJWT); 
            }
            else 
            {
                var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
                //ADD CLAIMS 
                claimsIdentity.AddClaims(await _mediator.Send(new AddInitialClaimsCommand(_casinoGodsDbContext, Context.ConnectionId, newJWT, paramUName)));
                //SEND
                ActiveTablesData.UserContextDictionary.TryAdd(paramUName, Context);
                await Clients.Others.SendAsync("ChatReports", paramUName + " has entered the chat");
                await Clients.Caller.SendAsync("JwtUpdate", newJWT);
                await base.OnConnectedAsync();
                //
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            ClaimsFields claimsFields = new(claimsIdentity);

            if (claimsFields.Username == null) _logger.LogCritical("CRITICAL ERROR! USERNAME NOT FOUND");
            else
            {
                if (claimsFields.Role != "Guest")
                {                                     
                    await _mediator.Send(new SaveBankrollProfitToDbCommand(_casinoGodsDbContext, claimsFields));//SET PLAYER BANKROLL AND PROFIT TO DATABASE   
                }
                
                await _mediator.Send(new DeleteUserFromDictionariesCommand(claimsFields));//DELETE USER DATA FROM DICTIONARIES RELATED TO TABLES
                //SEND
                await Clients.Others.SendAsync("ChatReports", claimsFields.Username + " has left the chat");
                await base.OnDisconnectedAsync(exception);
            }
        }

        //SENDING CHAT MESSAGES
        public async Task ChatMessages(string username, string message) 
        {
            await Clients.Others.SendAsync("ChatMessages", username, message);
        }
        public async Task TableChatMessages(string username,string message)
        {  
            string tableId = ActiveTablesData.UserGroupDictionary[username];
            await Clients.Group(tableId).SendAsync("TableChatMessages", username, message);
        }
        //

        //ENTER AND QUIT TABLE
        public async Task<bool> EnterTable(string TableId, string jwt)
        {
            
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            if (username == null)
            { 
                _logger.LogCritical("CRITICAL ERROR! USERNAME CLAIM NOT FOUND");
                throw new HubException("CRITICAL ERROR! USERNAME CLAIM NOT FOUND");
            }
            else
            {
                await _mediator.Send(new UpdateJwtCommand(claimsIdentity, Clients, jwt));
                return await _mediator.Send(new EnterTableCommand(TableId, username,Clients,Groups,Context));               
            }

        }
        public async Task QuitTable(string jwt)
        {
            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            var username = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            if (username == null)
            {
                _logger.LogCritical("CRITICAL ERROR! USERNAME CLAIM NOT FOUND");
                throw new HubException("CRITICAL ERROR! USERNAME CLAIM NOT FOUND");
            }
            else
            {
                await _mediator.Send(new QuitTableCommand(username, claimsIdentity , Clients , jwt, Groups, Context));
            }
        }
        //
        
        //QUERIES
        public async Task GetTableData() 
        {
            var AllActiveTables=await _casinoGodsDbContext.ActiveTables.ToListAsync();
            if (AllActiveTables.Count == 0) 
                await _mediator.Send(new AddBasicTablesCommand(_casinoGodsDbContext));
            else AllActiveTables = ActiveTablesData.ActiveTables;

            var listToSend = await _mediator.Send(new GetActiveTablesListQuery(GetType()));
            await Clients.Caller.SendAsync("TablesData", listToSend);
        } 
        public Task<bool> CheckFullTable(string TableId)
        {
            int CurrentUsers = ActiveTablesData.UserCountAtTablesDictionary[TableId];
            int MaxUsers = ActiveTablesData.ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == TableId).Maxseats;    
            if (MaxUsers != default)
            {
                if (CurrentUsers < MaxUsers) return Task.FromResult(true); 
                else return Task.FromResult(false);
            }
            else 
                throw new HubException("Data error");
        }        
        //        

        //IN-GAME COMMANDS
        public async Task SendBets(List<int> Bets, string jwt, string ClosedBetsToken)
        {
            bool JwtValid = await LobbyService.LookForJWTGlobal(jwt);
            if (!JwtValid) await Clients.Caller.SendAsync("Disconnect", "Session expired, log in again");
            else
            {
                var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
                var UsernameClaim = claimsIdentity.FindFirst("Username");

                if (UsernameClaim == null) await Clients.Caller.SendAsync("Disconnect", "Claims Error");
                else
                {
                    string TableId = ActiveTablesData.UserGroupDictionary[UsernameClaim.Value];
                    if (ActiveTablesData.ManagedTablesObj[TableId].BetsClosed == false)
                    {                    
                        await _mediator.Send(new ManualBetSendCommand(UsernameClaim.Value, TableId, Bets, jwt, claimsIdentity, Clients));
                    }
                    else
                    {
                        await _mediator.Send(new AutoBetSendCommand(UsernameClaim.Value, TableId, Bets, jwt,claimsIdentity, Clients, ClosedBetsToken));
                    }
                }
            }
        }
        public async Task<bool> IsBettingEnabled() 
        { 
            return await Task.FromResult(true); 
        }
        public async Task ToggleBetting(bool IsEnabled, string TableId) 
        { 
            await Clients.Group(TableId).SendAsync("ToggleBetting", IsEnabled); 
        }
        //
    }
    public class BacarratLobby :  LobbyHub
    {
        public BacarratLobby(CasinoGodsDbContext casinoGodsDbContext,  IMediator mediator, ILogger<BacarratLobby> logger)
        : base(casinoGodsDbContext, mediator, logger) {}
    }
    public class BlackjackLobby : LobbyHub
    {
        public BlackjackLobby(CasinoGodsDbContext casinoGodsDbContext , IMediator mediator, ILogger<BlackjackLobby> logger)
        : base(casinoGodsDbContext, mediator, logger) {}
    }
    public class DragonTigerLobby : LobbyHub
    {
        public DragonTigerLobby(CasinoGodsDbContext casinoGodsDbContext, IMediator mediator, ILogger<DragonTigerLobby> logger)
        : base(casinoGodsDbContext, mediator, logger) {}
    }
    public class RouletteLobby : LobbyHub
    {
        public RouletteLobby(CasinoGodsDbContext casinoGodsDbContext, IMediator mediator, ILogger<RouletteLobby> logger)
        : base(casinoGodsDbContext, mediator, logger) {}
    }
    public class WarLobby : LobbyHub
    {
        public WarLobby(CasinoGodsDbContext casinoGodsDbContext, IMediator mediator, ILogger<LobbyHub> logger)
        : base(casinoGodsDbContext , mediator, logger) {}
    }
}
