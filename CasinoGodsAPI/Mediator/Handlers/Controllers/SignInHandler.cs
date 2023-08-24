using CasinoGodsAPI.Data;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class SignInHandler : IRequestHandler<SignInCommand, IActionResult>
    {
        private readonly IMediator _mediator;
        private readonly CasinoGodsDbContext _casinoGodsDbContext;

        public SignInHandler(CasinoGodsDbContext casinoGodsDbContext,IMediator mediator)
        {
            _mediator = mediator;
            _casinoGodsDbContext = casinoGodsDbContext;
        }
        public async Task<IActionResult> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            Player loggedPlayer = await _casinoGodsDbContext.Players.SingleOrDefaultAsync(play => play.Username == request.PlayerCredientials.username);

            if (loggedPlayer == null) return new BadRequestObjectResult("Username not found");
            else
            {
                if (!CheckPassword(request.PlayerCredientials.password, loggedPlayer.PassHash, loggedPlayer.PassSalt))
                    return new UnauthorizedObjectResult("Password not correct");
                else
                    return await _mediator.Send(new CorrectSignInCommand(loggedPlayer, request.PlayerCredientials.username), cancellationToken);               
            }
        }
        private bool CheckPassword(string password, byte[] passHash, byte[] passSalt)
        {
            using (var hmac = new HMACSHA512(passSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passHash);

            }
        }
    }
}
