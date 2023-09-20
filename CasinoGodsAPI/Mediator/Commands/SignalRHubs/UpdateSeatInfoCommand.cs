using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record UpdateSeatInfoCommand(string TableId, int SeatId, IHubCallerClients Clients) : IRequest;
}
