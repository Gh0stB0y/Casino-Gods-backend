using CasinoGodsAPI.Data;
using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using CasinoGodsAPI.Models.DatabaseModels;
using CasinoGodsAPI.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class AddBasicTablesHandler : IRequestHandler<AddBasicTablesCommand>
    {
        public async Task Handle(AddBasicTablesCommand request, CancellationToken cancellationToken)
        {
            ActiveTablesData.UserCountAtTablesDictionary.Clear();
            var NewTables = await request.CasinoGodsDbContext.Tables.ToListAsync(cancellationToken: cancellationToken);
            var list = new List<ActiveTablesDB>();

            foreach (var table in NewTables)
            {
                var TableInstance = new ActiveTablesDB();   
                TableInstance.AddProperties(table,Guid.NewGuid());
                list.Add(TableInstance);
            }

            ActiveTablesData.ActiveTables = list;

            foreach (var activeTable in list) 
            { 
                await request.CasinoGodsDbContext.ActiveTables.AddAsync(activeTable, cancellationToken);
            }
            
            foreach (var table in ActiveTablesData.ActiveTables) 
            { 
                LobbyService.AddTable(table.TableInstanceId.ToString(), table.Game);
            }
            await request.CasinoGodsDbContext.SaveChangesAsync(cancellationToken);
            return;
        }
    }
}
