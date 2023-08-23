using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Mediator.Commands.Controllers.AdminController
{
    public record ManageTableCommand(AdminManageTableDTO Obj) : IRequest<IActionResult>;
}
