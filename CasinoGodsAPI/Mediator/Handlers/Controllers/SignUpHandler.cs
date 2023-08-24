using CasinoGodsAPI.Data;
using CasinoGodsAPI.Models.DatabaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CasinoGodsAPI.Controllers;
using Microsoft.AspNet.SignalR.Messaging;
using CasinoGodsAPI.DTOs;
using CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController;
using System.ComponentModel.DataAnnotations;
using CasinoGodsAPI.Validation;
using CasinoGodsAPI.Mediator.Commands.Databases;

namespace CasinoGodsAPI.Mediator.Handlers.Controllers
{
    public class SignUpHandler : IRequestHandler<SignUpCommand, IActionResult>
    {
        private readonly CasinoGodsDbContext _casinoGodsDbContext;
        private readonly IMediator _mediator;

        public SignUpHandler(CasinoGodsDbContext casinoGodsDbContext, IMediator mediator)
        {
            _casinoGodsDbContext = casinoGodsDbContext;
            _mediator = mediator;
        }
        public async Task<IActionResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            SignUpValidator validator = new (_casinoGodsDbContext);
            var CredentialsValid =await validator.ValidateAsync(request.NewPlayer, cancellationToken);
            if(CredentialsValid.IsValid)
                return await _mediator.Send(new AddPlayerDbRecordCommand(request.NewPlayer, _casinoGodsDbContext), cancellationToken);
            else
                return new BadRequestObjectResult(CredentialsValid.Errors.First());     
        }
    }
}
