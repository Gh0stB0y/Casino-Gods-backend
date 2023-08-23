using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models.SignalRHubModels;
using MediatR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record SaveBankrollProfitToDbCommand(CasinoGodsDbContext CasinoGodsDbContext,ClaimsFields ClaimsFields):IRequest;

}
