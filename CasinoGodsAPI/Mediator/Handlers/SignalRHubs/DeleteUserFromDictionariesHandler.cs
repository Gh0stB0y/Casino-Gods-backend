using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Models.SignalRHubModels;
using CasinoGodsAPI.Services;
using MediatR;
using System.Text.RegularExpressions;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class DeleteUserFromDictionaries : IRequestHandler<DeleteUserFromDictionariesCommand>
    {
        private readonly ILogger<DeleteUserFromDictionaries> _logger;

        public DeleteUserFromDictionaries(ILogger<DeleteUserFromDictionaries> logger)
        {
            _logger = logger;
        }
        public async Task Handle(DeleteUserFromDictionariesCommand request, CancellationToken cancellationToken)
        {
            bool result = ActiveTablesData.UserGroupDictionary.TryGetValue(request.ClaimsFields.Username, out string tableId);
            if (result)
            {
                ActiveTablesData.UserGroupDictionary.TryRemove(request.ClaimsFields.Username, out _);
                ActiveTablesData.UserCountAtTablesDictionary[tableId]--;

                if (ActiveTablesData.UserCountAtTablesDictionary[tableId] < 1)
                {
                    LobbyService.MakeTableInactive(tableId);
                    ActiveTablesData.DeleteTableConditions(tableId);
                }
                _logger.LogInformation("Disconnected user was seating at the table");
            }
            else
            {
               _logger.LogInformation("Disconnected user was not seating at any table");                
            }

            ActiveTablesData.UserContextDictionary.TryRemove(request.ClaimsFields.Username, out _);

            return;
        }
    }
}
