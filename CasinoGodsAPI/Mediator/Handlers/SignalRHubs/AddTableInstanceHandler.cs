using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using MediatR;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class AddTableInstanceHandler : IRequestHandler<AddTableInstanceCommand>
    {
        public Task Handle(AddTableInstanceCommand request, CancellationToken cancellationToken)
        {
            var Table = ActiveTablesData.ActiveTables.SingleOrDefault(i => i.TableInstanceId.ToString() == request.TableId);
            var SameTypeInstances = ActiveTablesData.ActiveTables.Where(t => t.Game == Table.Game && t.Name == Table.Name).ToList();
            if (SameTypeInstances.Where(t => t.Maxseats > ActiveTablesData.UserCountAtTablesDictionary[t.TableInstanceId.ToString()]).ToList().Count < 1)
                ActiveTablesData.AddNewTableConditions(request.TableId);
            return Task.CompletedTask;
        }
    }
}
