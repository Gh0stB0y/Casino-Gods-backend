using CasinoGodsAPI.Data_Containers;
using CasinoGodsAPI.Mediator.Commands.SignalRHubs;
using MediatR;

namespace CasinoGodsAPI.Mediator.Handlers.SignalRHubs
{
    public class TakeASeatHandler : IRequestHandler<TakeASeatCommand,string>
    {
        public Task<string> Handle(TakeASeatCommand request, CancellationToken cancellationToken)
        {
            var table = ActiveTablesData.ManagedTablesObj[request.TableId];
            if (table.UsersAtSeat[request.SeatId] == "") 
            {
                var username = ActiveTablesData.UserContextDictionary.FirstOrDefault(u => u.Value == request.Context).Key;
                if (username == null)
                {
                    return Task.FromResult("User context not found ");
                }
                else
                {
                    table.UsersAtSeat[request.SeatId] = username;

                    if(table.SeatsTakenByUser.TryGetValue(username, out var seatsTakenByUser))
                    {
                        table.SeatsTakenByUser.TryUpdate(username, seatsTakenByUser+1, seatsTakenByUser);
                    }
                    else
                    {
                        table.SeatsTakenByUser.TryAdd(username, 1);
                    }
                    return Task.FromResult("Ok");
                }                
            }
            else 
            {
                return Task.FromResult("Seat is already taken");
            }
        }
    }
}
