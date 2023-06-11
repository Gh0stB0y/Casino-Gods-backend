using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.ExceptionServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Tasks;
namespace CasinoGodsAPI.TablesModel
{

    public interface LobbyHub
    {
        public Task<string> EnterTable(int tableNumber);
        public Task<string> LeaveLobby();
        public Task<string> GetTablesData();
        
    }


    public class BlackJackLobby : Hub, LobbyHub
    {
        public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public async Task<string> GetTablesData() { return "chuj"; }

        public async Task FirstMessage(string message) {
            Console.WriteLine("funkcja");
            await Clients.All.SendAsync("FirstMessage", message); 
        }

    }

    public class RouletteLobby : Hub, LobbyHub
    {
        public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public async Task<string> GetTablesData() { return "chuj"; }
    }

    public class DragonTigerLobby : Hub, LobbyHub
    {
        public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public async Task<string> GetTablesData() { return "chuj"; }
    }

    public class WarLobby : Hub, LobbyHub
    {
        public async Task<string> EnterTable(int tableNumber) { return "chuj"; }
        public async Task<string> LeaveLobby() { return "chuj"; }
        public async Task<string> GetTablesData() { return "chuj"; }
    }

}
