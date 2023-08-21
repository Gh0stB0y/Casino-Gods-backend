using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasinoGodsAPI.Commands.Databases
{
    //public record AddDbRecordCommand<T>(DbContext DbContext,DbSet<T> TableInDb, T ObjToAdd) : IRequest<T> where T : class;
    public record AddTableDbRecordCommand(DbContext DbContext, DbSet<Tables> TableInDb, Tables ObjToAdd) : IRequest<Tables>;
}

