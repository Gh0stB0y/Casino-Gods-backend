using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Migrations;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class UpdateSeatInfoHandler : IRequestHandler<UpdateSeatInfoCommand>
    {
        public Task Handle(UpdateSeatInfoCommand request, CancellationToken cancellationToken)
        {
            var table = ActiveTablesData.ManagedTablesObj[request.TableId];
            TableSeatDTO changedSeat = new(request.SeatId,table);
            request.Clients.Group(request.TableId).SendAsync("UpdateSeatInfo",changedSeat);
            return Task.CompletedTask;
        }
    }
}
