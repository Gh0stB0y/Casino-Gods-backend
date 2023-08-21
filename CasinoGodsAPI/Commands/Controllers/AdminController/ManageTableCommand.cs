using CasinoGodsAPI.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CasinoGodsAPI.Commands.Controllers.AdminController
{
    public record ManageTableCommand(AdminManageTableDTO Obj):IRequest<IActionResult>; 
}
