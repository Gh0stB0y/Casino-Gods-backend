using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Queries.BackgroundServices;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.TablesModel;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CasinoGodsAPI.Mediator.Handlers.BackgroundServices
{
    public class GetActiveTablesListHandler : IRequestHandler<GetActiveTablesListQuery, List<LobbyTableDataDTO>>
    {
        private readonly ILogger _logger;

        public GetActiveTablesListHandler(ILogger<GetActiveTablesListHandler> logger)
        {
            _logger = logger;
        }
        public async Task<List<LobbyTableDataDTO>> Handle(GetActiveTablesListQuery request, CancellationToken cancellationToken)
        {
            if (ActiveTablesData.ActiveTables != null)
            {
                
                try
                {
                    string typ = request.HubContextType.Name.Replace("Lobby", "");
                    if (typ == "DragonTiger") typ = "Dragon Tiger";

               
                    List<LobbyTableDataDTO> listToSend = new List<LobbyTableDataDTO>();

                    List<ActiveTablesDB> list = ActiveTablesData.ActiveTables.Where(g => g.Game == typ).ToList();

                    foreach (var table in list)
                    {
                        var tableObj = new LobbyTableDataDTO(table);
                        tableObj.currentSeats = ActiveTablesData.UserCountAtTablesDictionary[tableObj.Id];
                        listToSend.Add(tableObj);
                    }
                    return listToSend;
                }
                catch (Exception ex) 
                { 
                    _logger.LogError(ex.Message);
                    return null;
                }
            }
            else
            {
                _logger.LogError("ActiveTables is empty");
                return null;
            }
        }
    }
}
