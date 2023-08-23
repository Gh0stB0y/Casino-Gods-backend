using CasinoGodsAPI.Mediator.Commands.Databases;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.Databases
{
    public class DeleteTableDbRecordHandler : IRequestHandler<DeleteTableDbRecordCommand, Tables>
    {
        private readonly ILogger _logger;

        public DeleteTableDbRecordHandler(ILogger<DeleteTableDbRecordHandler> logger)
        {
            _logger = logger;
        }
        public async Task<Tables> Handle(DeleteTableDbRecordCommand request, CancellationToken cancellationToken)
        {
            var dbContext = request.DbContext;
            var tableInDb = request.TableInDb;
            var objToDelete = request.ObjToDelete;
            if (objToDelete != null && tableInDb != null && dbContext != null)
            {
                var objFromDb = await tableInDb.SingleOrDefaultAsync(o => o == objToDelete, cancellationToken);
                if (objFromDb == null)
                {
                    _logger.LogError("Object not found");
                    return null;
                }
                else
                {
                    tableInDb.Remove(objFromDb);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return objFromDb;
                }

            }
            else
            {
                _logger.LogError("One of objects passed to handler is null");
                return null;
            }

        }
    }
}
