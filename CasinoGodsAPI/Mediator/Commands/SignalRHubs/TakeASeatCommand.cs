using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record TakeASeatCommand(int SeatId,string TableId, HubCallerContext Context):IRequest<string>;
}
