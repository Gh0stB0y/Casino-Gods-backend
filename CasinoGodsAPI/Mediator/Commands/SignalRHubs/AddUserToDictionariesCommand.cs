using MediatR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record AddUserToDictionariesCommand(string Id,string Username):IRequest;

}
