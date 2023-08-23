using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Services;
using MediatR;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class AddUserToDictionariesHandler : IRequestHandler<AddUserToDictionariesCommand>
    {

        public async Task Handle(AddUserToDictionariesCommand request, CancellationToken cancellationToken)
        {
            if (ActiveTablesData.UserCountAtTablesDictionary[request.Id] < 1 && !ActiveTablesData.ManagedTablesObj[request.Id].IsActive)
                LobbyService.MakeTableActive(request.Id);

            ActiveTablesData.UserGroupDictionary.TryAdd(request.Username, request.Id);
            ActiveTablesData.UserCountAtTablesDictionary[request.Id]++;
            ActiveTablesData.ManagedTablesObj[request.Id].AllBetsPlaced.TryAdd(request.Username, false);
            return;
        }
    }
}
