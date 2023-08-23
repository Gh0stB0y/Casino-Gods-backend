using MediatR;

namespace CasinoGodsAPI.Mediator.Queries.BackgroundServices
{
    public record CheckFullTableQuery(string Id):IRequest<bool>;

}
