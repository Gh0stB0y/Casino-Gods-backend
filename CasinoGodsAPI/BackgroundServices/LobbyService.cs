using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Reflection;

namespace CasinoGodsAPI.Services
{
    public class LobbyService : BackgroundService
    {
        public static Dictionary<Type, IHubContext<Hub>> _hubContexts;
        private readonly ILogger<LobbyService> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private static IServiceProvider _serviceProvider;
        private static IConfiguration _configuration;
        private static IConnectionMultiplexer _redis;

        public static ConcurrentDictionary<string, ManageTableClass> ManagedTablesObj = new ConcurrentDictionary<string, ManageTableClass>();
        public static ConcurrentDictionary<string, int> UserCountAtTablesDictionary = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, string> UserGroupDictionary = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, HubCallerContext> UserContextDictionary = new ConcurrentDictionary<string, HubCallerContext>();


        public LobbyService(IServiceProvider serviceProvider, ILogger<LobbyService> logger, IConfiguration configuration, IConnectionMultiplexer redis)
        {

            _serviceProvider = serviceProvider;
            _hubContexts = GetHubContexts(serviceProvider);
            _logger = logger;
            _configuration = configuration;
            _redis = redis;
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

        //CUSTOM FUNKCJE
        private Dictionary<Type, IHubContext<Hub>> GetHubContexts(IServiceProvider serviceProvider)
        {
            var hubContexts = new Dictionary<Type, IHubContext<Hub>>();
            var hubTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsSubclassOf(typeof(LobbyHub)));

            foreach (var hubType in hubTypes)
            {
                var hubContextType = typeof(IHubContext<>).MakeGenericType(hubType);
                var hubContext = serviceProvider.GetRequiredService(hubContextType) as IHubContext<Hub>;
                hubContexts.Add(hubType, hubContext);
            }
            return hubContexts;
        }
        //


        //OBSLUGA STOLOW
        public static void MakeTableActive(string TableId)
        {
            if (!ManagedTablesObj[TableId].IsActive && !ManagedTablesObj[TableId].GameInProgress)
            {
                ManagedTablesObj[TableId].IsActive = true;
                Task.Run(() => ManagedTablesObj[TableId].StartGame());
            }
        }
        public static void MakeTableInactive(string TableId)
        {
            ManagedTablesObj[TableId].IsActive = false;
        }
        public static void AddTable(string TableId, string Game)
        {
            UserCountAtTablesDictionary.TryAdd(TableId, 0);
            ManagedTablesObj.TryAdd(TableId, new ManageTableClass(TableId, Game, _serviceProvider, _configuration, _redis));
        }
        public static void DeleteTable(string TableId)
        {
            UserCountAtTablesDictionary.TryRemove(TableId, out _);
            ManagedTablesObj.TryRemove(TableId, out _);
        }
        //
    }   
}
