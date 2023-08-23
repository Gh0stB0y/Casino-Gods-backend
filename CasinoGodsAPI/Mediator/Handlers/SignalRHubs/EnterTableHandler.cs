using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Mediator.Queries.BackgroundServices;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class EnterTableHandler : IRequestHandler<EnterTableCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnterTableHandler> _logger;

        public EnterTableHandler(IMediator mediator,ILogger<EnterTableHandler>logger)
        {
            _mediator = mediator;
            _logger= logger;
        }
        public async Task<bool> Handle(EnterTableCommand request, CancellationToken cancellationToken)
        {

            if (!ActiveTablesData.UserGroupDictionary.ContainsKey(request.Username))
            {
                bool TableNotFull;
                //CHECK IF TABLE IS NOT FULL
                try { TableNotFull = await _mediator.Send(new CheckFullTableQuery(request.TableId), cancellationToken); }
                catch (HubException ex)
                {
                    _logger.LogError(ex.Message);
                    return false;
                }
                //
                //PERFORM ACTION
                if (TableNotFull)
                {
                    await _mediator.Send(new AddUserToDictionariesCommand(request.TableId, request.Username));
                    await request.Groups.AddToGroupAsync(request.Context.ConnectionId, request.TableId);
                    await request.Clients.OthersInGroup(request.TableId).SendAsync("TableChatReports", request.Username + " has entered the table");
                }
                else
                    await request.Clients.Caller.SendAsync("ErrorMessages", "Table is full");
                //
                //CHECK IF ANOTHER TABLE IS NEEDED, IF YES, ADD TABLE INSTANCE
                await _mediator.Send(new AddTableInstanceCommand(request.TableId));
                //
                return TableNotFull;
            }
            else
            {
                await request.Clients.Caller.SendAsync("ErrorMessages", "This user is already playing at the table");
                _logger.LogError("This user is already playing at the table");
                return false;
            }

        }
    }
}
