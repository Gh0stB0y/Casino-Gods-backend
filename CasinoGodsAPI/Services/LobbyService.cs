using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Databases;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Queries.BackgroundServices;
using CasinoGodsAPI.Models;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.TablesModel;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
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
        private readonly IMediator _mediator;
        private static IDatabase _redisDbLogin;
        private static IDatabase _redisDbJwt;

        public LobbyService(IServiceProvider serviceProvider, ILogger<LobbyService> logger, IConfiguration configuration, IConnectionMultiplexer redis,
                                IMediator mediator)
        {
            _serviceProvider = serviceProvider;
            _hubContexts = GetHubContexts(serviceProvider);
            _logger = logger;
            _configuration = configuration;
            _redis = redis;
            _mediator =mediator;
            _redisDbLogin = _redis.GetDatabase(0);
            _redisDbJwt = _redis.GetDatabase(1);
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

                if (ActiveTablesData.ActiveTables != null)
                {                  
                    foreach (var hubContext in _hubContexts)
                    {
                        //List<LobbyTableDataDTO> listToSend = await _mediator.Send(new GetActiveTablesListQuery(hubContext.Key, hubContext.Value), stoppingToken);
                        List<LobbyTableDataDTO> listToSend = await _mediator.Send(new GetActiveTablesListQuery(hubContext.Key), stoppingToken);
                        if (listToSend != null) await hubContext.Value.Clients.All.SendAsync("TablesData", listToSend, cancellationToken: stoppingToken);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }
        public async Task StartBackgroundService() //reczne wlaczenie
        {
            _cancellationTokenSource?.Cancel();
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
            if (!ActiveTablesData.ManagedTablesObj[TableId].IsActive && !ActiveTablesData.ManagedTablesObj[TableId].GameInProgress)
            {
                ActiveTablesData.ManagedTablesObj[TableId].IsActive = true;
                Task.Run(() => ActiveTablesData.ManagedTablesObj[TableId].StartGame());
            }
        }
        public static void MakeTableInactive(string TableId)
        {
            ActiveTablesData.ManagedTablesObj[TableId].IsActive = false;
        }
        public static void AddTable(string TableId, string Game)
        {
            ActiveTablesData.UserCountAtTablesDictionary.TryAdd(TableId, 0);
            ActiveTablesData.ManagedTablesObj.TryAdd(TableId, new PlayingTable(TableId, Game, _serviceProvider));
        }
        public static void DeleteTable(string TableId)
        {
            ActiveTablesData.UserCountAtTablesDictionary.TryRemove(TableId, out _);
            ActiveTablesData.ManagedTablesObj.TryRemove(TableId, out _);
        }
        //

        //UPDATE AND SEARCH OF JWT TOKEN
        public static async Task<string> RefreshTokenGlobal(string jwt)
        {
            var ActivePlayerCheck = await _redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return "Session expired, log in again";
            else
            {
                string username = ActivePlayerCheck.ToString();

                await _redisDbJwt.KeyDeleteAsync(jwt);
                await _redisDbLogin.KeyDeleteAsync(username);
                var ActivePlayer = new RedisActivePlayer(username, jwt, _configuration);
                await _redisDbLogin.StringSetAsync(ActivePlayer.Name, ActivePlayer.Jwt, new TimeSpan(0, 0, 5, 0));
                await _redisDbJwt.StringSetAsync(ActivePlayer.Jwt, ActivePlayer.Name, new TimeSpan(0, 0, 5, 0));

                return ActivePlayer.Jwt;
            }
        }
        public static async Task<string> RefreshTokenGlobal(string jwt, string UName)
        {
            var ActivePlayerCheck = await _redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return "Session expired, log in again";
            else
            {
                string username = ActivePlayerCheck.ToString();
                if (UName == username)
                {
                    await _redisDbJwt.KeyDeleteAsync(jwt);
                    await _redisDbLogin.KeyDeleteAsync(username);
                    var ActivePlayer = new RedisActivePlayer(username, jwt, _configuration);
                    Console.WriteLine("JWT ZAPISANE DO REDISA" + ActivePlayer.Jwt);
                    await _redisDbLogin.StringSetAsync(ActivePlayer.Name, ActivePlayer.Jwt, new TimeSpan(0, 0, 5, 0));
                    await _redisDbJwt.StringSetAsync(ActivePlayer.Jwt, ActivePlayer.Name, new TimeSpan(0, 0, 5, 0));
                    return ActivePlayer.Jwt;
                }
                else return "Redis data error, log in again";
            }
        }
        public static async Task<bool> LookForTokenGlobal(string UName)
        {
            var ActivePlayerCheck = await _redisDbLogin.StringGetAsync(UName);
            return !ActivePlayerCheck.IsNull;
        }
        public static async Task<bool> LookForJWTGlobal(string jwt)
        {
            var ActivePlayerCheck = await _redisDbJwt.StringGetAsync(jwt);
            if (ActivePlayerCheck.IsNull) return false;
            else return true;
        }
    }   
}
