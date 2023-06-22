using CasinoGodsAPI.TablesModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CasinoGodsAPI.Services
{
    public class LobbyService : BackgroundService
    {

        private readonly Dictionary<Type, Microsoft.AspNetCore.SignalR.IHubContext<Microsoft.AspNetCore.SignalR.Hub>> _hubContexts;
        private readonly ILogger<LobbyService> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        public LobbyService(IServiceProvider serviceProvider, ILogger<LobbyService> logger) {

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
                foreach(var hubContext in _hubContexts)
                {
                    await hubContext.Value.Clients.All.SendAsync("ChatMessages", "Kacper", "to kozak");
                }
                //await _hubContext.Clients.All.SendAsync("ChatMessages", "Kacper", "to kozak");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
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
        //

        //FUNKCJE CUSTOMOWE/POLECENIA DO WYKONANIA PRZY WYWOLANIU TIMERA

        //
    }

    
}
