using CasinoGodsAPI.Commands.Controllers.AdminController;
using CasinoGodsAPI.Commands.Databases;
using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Handlers.Controllers
{
    public class ManageTableHandler : IRequestHandler<ManageTableCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IMediator _mediator;

        public ManageTableHandler(CasinoGodsDbContext casinoGodsDbContext,IMediator mediator)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _mediator = mediator;
        }
        public async Task<IActionResult> Handle(ManageTableCommand request, CancellationToken cancellationToken)
        {
            var gameObj = await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g => g.Name == request.Obj.GameType, cancellationToken: cancellationToken);
            if (gameObj != null)
            {
                if (request.Obj.CheckTable())
                {
                    Tables TableObj = new()
                    {
                        CKname = request.Obj.Name,
                        CKGame = gameObj.Name,
                        MinBet = request.Obj.MinBet,
                        MaxBet = request.Obj.MaxBet,
                        BetTime = request.Obj.BetTime,
                        Maxseats = request.Obj.MaxSeats,
                        ActionTime = request.Obj.ActionTime,
                        Sidebet1 = request.Obj.Sidebet1,
                        Sidebet2 = request.Obj.Sidebet2,
                        Decks = request.Obj.Decks
                    };             

                    switch (request.Obj.ActionType)
                    {
                        case "add":

                            var addedObj = await _mediator.Send(new AddTableDbRecordCommand(_casinoGodsDbContext, _casinoGodsDbContext.TablesList, TableObj), cancellationToken);
                            if (addedObj != null) return new OkResult();
                            else return new BadRequestObjectResult("Table with given name already exists");

                        case "edit":

                            var oldObj=await _casinoGodsDbContext.TablesList.Where(g=>g.CKGame== TableObj.CKGame&&g.CKname==TableObj.CKname).SingleOrDefaultAsync(cancellationToken: cancellationToken);
                            if (oldObj != null)
                            {
                                var updatedObj = await _mediator.Send(new EditTableDbRecordCommand(_casinoGodsDbContext, _casinoGodsDbContext.TablesList, oldObj, TableObj), cancellationToken);
                                if (updatedObj != null) return new OkObjectResult(updatedObj);
                                else return new NotFoundObjectResult("Table not found");
                            }
                            else
                            {
                                return new BadRequestObjectResult("Table with given composite key doesn't exist");
                            }

                        case "delete":

                            var deletedObj = await _mediator.Send(new DeleteTableDbRecordCommand(_casinoGodsDbContext, _casinoGodsDbContext.TablesList, TableObj), cancellationToken);
                            if(deletedObj!=null) return new OkObjectResult(deletedObj);
                            else return new NotFoundObjectResult("Table not found");

                        default:

                            return new BadRequestObjectResult("Wrong action Type");
                    }
                }
                else return new BadRequestObjectResult("Wrong table data");
            }
            else return new BadRequestObjectResult("Wrong game type");
        }
    }
}
