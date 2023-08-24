using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Databases
{
    public record AddPlayerDbRecordCommand(SignUpDTO NewPlayer,CasinoGodsDbContext CasinoGodsDbContext):IRequest<IActionResult>;
}
