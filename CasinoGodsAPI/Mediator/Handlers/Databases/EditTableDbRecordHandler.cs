using CasinoGodsAPI.Mediator.Commands.Databases;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Mediator.Handlers.Databases
{
    public class EditTableDbRecordHandler : IRequestHandler<EditTableDbRecordCommand, Tables>
    {
        private readonly ILogger _logger;

        public EditTableDbRecordHandler(ILogger<EditTableDbRecordHandler> logger)
        {
            _logger = logger;
        }
        public async Task<Tables> Handle(EditTableDbRecordCommand request, CancellationToken cancellationToken)
        {
            var dbContext = request.DbContext;
            var tableInDb = request.TableInDb;
            var oldObj = request.OldObj;
            var newObj = request.NewObj;
            if (oldObj != null && tableInDb != null && dbContext != null && newObj != null)
            {
                var objFromDb = await tableInDb.SingleOrDefaultAsync(o => o == oldObj);
                if (objFromDb != null)
                {
                    objFromDb = newObj;
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Object updated");
                    return newObj;
                }
                else
                {
                    _logger.LogError("Old object not found");
                    return null;
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
