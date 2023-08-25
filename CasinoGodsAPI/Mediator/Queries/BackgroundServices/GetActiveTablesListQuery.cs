using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Queries.BackgroundServices
{
    public record GetActiveTablesListQuery(Type HubContextType) : IRequest<List<LobbyTableDataDTO>>;
}
