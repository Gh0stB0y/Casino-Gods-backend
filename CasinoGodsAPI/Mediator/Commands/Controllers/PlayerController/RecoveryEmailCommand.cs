﻿using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.PlayerController
{
    public record RecoveryEmailCommand(EmailDTO Email) : IRequest<IActionResult>;
}
