using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.ExceptionServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Tasks;
using CasinoGodsAPI.Models;

namespace CasinoGodsAPI.TablesModel
{

    public  class LobbyHub: Hub
    {
        /*public abstract Task<string> EnterTable(int tableNumber);
        public abstract Task<string> LeaveLobby();
        public abstract Task<List<LobbyTableData>> GetTablesData();*/
        /*public abstract Task ChatMessages(string username, string message);*/
        protected Dictionary<string, string> LobbyUsers = new Dictionary<string, string>();
        public  async Task ChatMessages(string username, string message) { await Clients.All.SendAsync("ChatMessages", username, message); }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            string report = Context.ConnectionId + " entered the chat";
            await Clients.Others.SendAsync("ChatReports", report);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Perform actions when a client disconnects
            await base.OnDisconnectedAsync(exception);
            string report = Context.ConnectionId + " left the chat";
            // Invoke a specific function or perform any necessary actions
            await Clients.Others.SendAsync("ChatReports", report);
        }
    }

    public class BacarratLobby :  LobbyHub
    {
        /*public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public Task<List<LobbyTableData>> GetTablesData() { return null; }*/   
       /* public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            string report = Context.ConnectionId + " entered the chat";
            await Clients.Others.SendAsync("ChatReports", report);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Perform actions when a client disconnects
            await base.OnDisconnectedAsync(exception);
            string report = Context.ConnectionId + " left the chat";
           // Invoke a specific function or perform any necessary actions
           await Clients.Others.SendAsync("ChatReports", report);
        }*/
    }
    public class BlackJackLobby : LobbyHub
    {
        /*public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public Task<List<LobbyTableData>> GetTablesData() { return null; }*/
       

    }

    public class RouletteLobby : LobbyHub
    {
        /* public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
         public async Task<string> LeaveLobby() { return "chuj"; }
         public Task<List<LobbyTableData>> GetTablesData() { return null; }*/

    }
    public class DragonTigerLobby : LobbyHub
    {
        /*   public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
           public async Task<string> LeaveLobby() { return "chuj"; }
           public Task<List<LobbyTableData>> GetTablesData() { return null; }*/

    }

    public class WarLobby : LobbyHub
    {
        /*public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public Task<List<LobbyTableData>> GetTablesData() { return null; }*/

    }
}
