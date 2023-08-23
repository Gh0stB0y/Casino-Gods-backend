using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Services;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class QuitTableHandler : IRequestHandler<QuitTableCommand>
    {
        private readonly IMediator _mediator;

        public QuitTableHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(QuitTableCommand request, CancellationToken cancellationToken)
        {
            string tableId = ActiveTablesData.UserGroupDictionary[request.Username];
            
            //UPDATE JWT
            await _mediator.Send(new UpdateJwtCommand(request.ClaimsIdentity, request.Clients, request.Jwt), cancellationToken);
            //

            //DELETE USER FROM DICTIONARIES AND GROUP
            ActiveTablesData.UserGroupDictionary.TryRemove(request.Username, out _);
            ActiveTablesData.UserCountAtTablesDictionary[tableId]--;
            await request.Groups.RemoveFromGroupAsync(request.CallerContext.ConnectionId, tableId);
            
            if (ActiveTablesData.UserCountAtTablesDictionary[tableId] < 1)
            {
                LobbyService.MakeTableInactive(tableId);
                ActiveTablesData.DeleteTableConditions(tableId);
            }
            //

            await request.Clients.OthersInGroup(tableId).SendAsync("TableChatReports", request.Username + " has left the table", cancellationToken: cancellationToken);
        }
    }
}
