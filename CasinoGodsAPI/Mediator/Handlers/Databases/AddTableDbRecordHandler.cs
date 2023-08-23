using CasinoGodsAPI.Data;
using CasinoGodsAPI.Mediator.Commands.Databases;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CasinoGodsAPI.Mediator.Handlers.Databases
{
    public class AddTableDbRecordHandler : IRequestHandler<AddTableDbRecordCommand, Tables>
    {
        private readonly ILogger _logger;
        public AddTableDbRecordHandler(ILogger<AddTableDbRecordHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Tables> Handle(AddTableDbRecordCommand request, CancellationToken cancellationToken)
        {
            var dbContext = request.DbContext;
            var tableInDb = request.TableInDb;
            var objToAdd = request.ObjToAdd;

            if (objToAdd != null && tableInDb != null && dbContext != null)
            {
                var objFromDb = await request.TableInDb.SingleOrDefaultAsync(o => o == request.ObjToAdd, cancellationToken);
                if (objFromDb == null)
                {
                    try
                    {
                        await tableInDb.AddAsync(objToAdd, cancellationToken);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Object added to Table");
                        return objToAdd;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Obj with given parameters already exists");
                        return null;
                    }
                }
                else
                {
                    _logger.LogError("Obj with given parameters already exists");
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
