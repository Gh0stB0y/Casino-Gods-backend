using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Commands.Databases
{
    public record DeleteTableDbRecordCommand(DbContext DbContext,DbSet<Tables> TableInDb, Tables ObjToDelete) : IRequest<Tables>;
}
