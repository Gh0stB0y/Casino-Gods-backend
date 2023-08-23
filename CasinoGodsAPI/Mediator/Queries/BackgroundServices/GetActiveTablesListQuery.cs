using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Queries.BackgroundServices
{
    //public record GetActiveTablesListQuery(Type HubContextType, IHubContext<Hub>HubContext) :IRequest<List<LobbyTableDataDTO>>;
    public record GetActiveTablesListQuery(Type HubContextType) : IRequest<List<LobbyTableDataDTO>>;
}
