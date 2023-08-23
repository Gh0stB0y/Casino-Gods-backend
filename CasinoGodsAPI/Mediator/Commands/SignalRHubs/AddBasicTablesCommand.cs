using CasinoGodsAPI.Data;
using MediatR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record AddBasicTablesCommand(CasinoGodsDbContext CasinoGodsDbContext):IRequest;
}
