using MediatR;

namespace CasinoGodsAPI.Mediator.Commands.SignalRHubs
{
    public record AddTableInstanceCommand(string TableId):IRequest;        
}
