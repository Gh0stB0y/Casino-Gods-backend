using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Commands.Databases
{
    public record EditTableDbRecordCommand(DbContext DbContext,DbSet<Tables> TableInDb, Tables OldObj,Tables NewObj) : IRequest<Tables>;
}
