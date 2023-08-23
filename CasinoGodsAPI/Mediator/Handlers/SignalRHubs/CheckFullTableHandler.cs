using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Queries.BackgroundServices;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class CheckFullTableHandler : IRequestHandler<CheckFullTableQuery, bool>
    {
        public async Task<bool> Handle(CheckFullTableQuery request, CancellationToken cancellationToken)
        {
            int CurrentUsers = ActiveTablesData.UserCountAtTablesDictionary[request.Id];
            int MaxUsers = ActiveTablesData.ActiveTables.SingleOrDefault(t => t.TableInstanceId.ToString() == request.Id).Maxseats;
            if (MaxUsers != default)
            {
                if (CurrentUsers < MaxUsers) return await Task.FromResult(true);
                else return await Task.FromResult(false);
            }
            throw new HubException("Data error");
        }
    }
}
