using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Reflection;

namespace CasinoGodsAPI.Services
{
    public class LobbyService : BackgroundService
    {
        private readonly Dictionary<Type, Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>> _hubContexts;
        private readonly ILogger<LobbyService> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        //public static List<LobbyTableData>ActiveTablesList { get; set; }

        public LobbyService(IServiceProvider serviceProvider, ILogger<LobbyService> logger, IConfiguration configuration, IConnectionMultiplexer redis) {

            _hubContexts = GetHubContexts(serviceProvider);
            _logger = logger;
        }
        private Dictionary<Type, Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>> GetHubContexts(IServiceProvider serviceProvider)
        {
            var hubContexts = new Dictionary<Type, Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>>();
            var hubTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsSubclassOf(typeof(LobbyHub)));
            foreach (var hubType in hubTypes)
            {
                var hubContextType = typeof(Microsoft.AspNetCore.SignalR.IHubContext<>).MakeGenericType(hubType);
                var hubContext = serviceProvider.GetRequiredService(hubContextType) as Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>;
                hubContexts.Add(hubType, hubContext);
            }
            return hubContexts;
        }

        //FUNKCJE PODSTAWOWE
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token));
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background service stopped.");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // akcje do wykonania przy cancellu
                    break;
                }

                if (LobbyHub.ActiveTables != null)
                {
                    foreach (var hubContext in _hubContexts)
                    {
                        try
                        {
                            string typ = hubContext.Key.Name.Replace("Lobby", "");
                            if (typ == "DragonTiger") typ = "Dragon Tiger";
                            List<ActiveTablesDB> list =new();
                            List<LobbyTableDataDTO> listToSend = new List<LobbyTableDataDTO>();
                            list = LobbyHub.ActiveTables.Where(g => g.Game == typ).ToList();
                            foreach (var table in list)
                            {
                                var tableObj = new LobbyTableDataDTO(table);
                                tableObj.currentSeats = LobbyHub.GetCurrentPlayers(tableObj.Id);
                                listToSend.Add(tableObj);
                            }
                            await hubContext.Value.Clients.All.SendAsync("TablesData", listToSend);
                        }
                        catch (Exception ex) { Console.WriteLine(ex); }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }
        public async Task StartBackgroundService() //reczne wlaczenie
        {
            if (_cancellationTokenSource != null) _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            await StartAsync(_cancellationTokenSource.Token);
        }
        public async Task StopBackgroundService()
        {
            _cancellationTokenSource?.Cancel();
            await StopAsync(_cancellationTokenSource.Token);
        } //reczne wylaczenie
    }   
}
