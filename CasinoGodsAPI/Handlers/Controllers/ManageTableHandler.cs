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
            var gameObj = await _casinoGodsDbContext.GamesList.SingleOrDefaultAsync(g => g.Name == request.Obj.GameType);
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

                            /*if (await _casinoGodsDbContext.TablesList.Where(g => g.CKGame == gameObj.Name).SingleOrDefaultAsync(t => t.CKname == request.Obj.Name) == null)
                            {

                                Tables newTable = new Tables()
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
                                await _casinoGodsDbContext.TablesList.AddAsync(newTable);
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return new OkResult();
                            }
                            else return new BadRequestObjectResult("Table with given name already exists");*/
                            var addedObj= await _mediator.Send(new AddDbRecordCommand(_casinoGodsDbContext,_casinoGodsDbContext.TablesList, TableObj), cancellationToken);
                            if (addedObj != null) return new OkResult();
                            else return new BadRequestObjectResult("Table with given name already exists");

                        case "edit":

                            /*var Table = await _casinoGodsDbContext.TablesList.Where(g => g.CKGame == request.Obj.GameType).SingleOrDefaultAsync(p => p.CKname == request.Obj.Name);
                            if (Table != null)
                            {
                                Table.CKGame = gameObj.Name; Table.MinBet = request.Obj.MinBet; Table.MaxBet = request.Obj.MaxBet; Table.BetTime = request.Obj.BetTime;
                                Table.Maxseats = request.Obj.MaxSeats; Table.ActionTime = request.Obj.ActionTime; Table.Sidebet1 = request.Obj.Sidebet1; Table.Sidebet2 = request.Obj.Sidebet2;
                                Table.Decks = request.Obj.Decks;
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return new OkResult();
                            }
                            else return new BadRequestObjectResult("Table not found");*/
                            var oldObj=await _casinoGodsDbContext.TablesList.Where(g=>g.CKGame== TableObj.CKGame&&g.CKname==TableObj.CKname).SingleOrDefaultAsync();
                            if (oldObj != null)
                            {
                                var updatedObj = await _mediator.Send(new EditDbRecordCommand(_casinoGodsDbContext, _casinoGodsDbContext.TablesList, oldObj, TableObj));
                                if (updatedObj != null) return new OkObjectResult(updatedObj);
                                else return new NotFoundObjectResult("Table not found");
                            }
                            else
                            {
                                return new BadRequestObjectResult("Table with given composite key doesn't exist");
                            }
                        case "delete":

                            /*var Table2 = await _casinoGodsDbContext.TablesList.Where(g => g.CKGame == request.Obj.GameType).SingleOrDefaultAsync(play => play.CKname == request.Obj.Name);
                            if (Table2 == null) { return new BadRequestObjectResult("Table not found"); }
                            else
                            {
                                _casinoGodsDbContext.TablesList.Remove(Table2);
                                await _casinoGodsDbContext.SaveChangesAsync();
                                return new OkResult();
                            }*/
                            var deletedObj = await _mediator.Send(new DeleteDbRecordCommand(_casinoGodsDbContext,_casinoGodsDbContext.TablesList, TableObj));
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
