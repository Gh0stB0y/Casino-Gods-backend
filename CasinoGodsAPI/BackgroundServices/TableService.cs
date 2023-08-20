using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using System.Threading;

namespace CasinoGodsAPI.Services
{
    public class TableService:BackgroundService
    {
        public static Dictionary<Type, Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>> _hubContexts;
        private readonly ILogger<LobbyService> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private static IServiceProvider _serviceProvider;
        private static IConfiguration _configuration;
        private static IConnectionMultiplexer _redis; 
   
        public static ConcurrentDictionary<string, ManageTableClass> ManagedTablesObj = new ConcurrentDictionary<string, ManageTableClass>();
        public static ConcurrentDictionary<string, int> UserCountAtTablesDictionary = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, string> UserGroupDictionary = new ConcurrentDictionary<string, string>();        
        public static ConcurrentDictionary<string, HubCallerContext> UserContextDictionary = new ConcurrentDictionary<string, HubCallerContext>();

        public TableService(IServiceProvider serviceProvider, ILogger<LobbyService> logger, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _hubContexts = GetHubContexts(serviceProvider);
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _redis = redis;
        }
        public Dictionary<Type, Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>> GetHubContexts(IServiceProvider serviceProvider)
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
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            //Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token));
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
                foreach(var Table in ManagedTablesObj.Values) { Console.WriteLine("ManagedTablesObj --> Table: " + Table.Id+", IsActive: "+Table.IsActive);}
                foreach(var Count in UserCountAtTablesDictionary) { Console.WriteLine("UserCountAtTablesDictionary --> Table: " + Count.Key + ", Count: " + Count.Value); }
                foreach(var User in UserGroupDictionary) { Console.WriteLine("UserGroupDictionary --> User: " + User.Key + " belongs to group/table: " + User.Value); }
                foreach (var User in UserContextDictionary) { Console.WriteLine("UserContextDictionary --> User: " + User.Key + " has Context: " + User.Value.ConnectionId); }
                //LookForEmptyTables();
                await Task.Delay(TimeSpan.FromSeconds(10));
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
        
        public static void MakeTableActive(string TableId)
        {
            if (!ManagedTablesObj[TableId].IsActive && !ManagedTablesObj[TableId].GameInProgress)
            {
                ManagedTablesObj[TableId].IsActive = true;
                Task.Run(() => ManagedTablesObj[TableId].StartGame());
                //ManagedTablesObj[TableId].StartGame();
            }
        }
        public static void MakeTableInactive(string TableId)
        {
            ManagedTablesObj[TableId].IsActive = false;
        }
        public static void AddTable(string TableId, string Game)
        {
            UserCountAtTablesDictionary.TryAdd(TableId, 0);
            ManagedTablesObj.TryAdd(TableId,new ManageTableClass(TableId,Game, _serviceProvider, _configuration, _redis));
        }
        public static void DeleteTable(string TableId)
        {           
            UserCountAtTablesDictionary.TryRemove(TableId,out _);
            ManagedTablesObj.TryRemove(TableId,out _);
        }
    }
}
