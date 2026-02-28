using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Passport.Application.Features.Tenants.Commands;
using QuestFlag.Passport.Application.Features.Tenants.Queries;
using QuestFlag.Passport.Application.Features.Users.Commands;
using QuestFlag.Passport.Application.Features.Users.Queries;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTenants()
    {
        var result = await _mediator.Send(new GetTenantsQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    [HttpGet("{tenantId}/users")]
    public async Task<IActionResult> GetUsers(System.Guid tenantId)
    {
        var result = await _mediator.Send(new GetUsersByTenantQuery(tenantId));
        return Ok(result);
    }

    [HttpPost("{tenantId}/users")]
    [Authorize(Policy = "TenantAdmin")]
    public async Task<IActionResult> CreateUser(System.Guid tenantId, [FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(
            tenantId, 
            request.Username, 
            request.Email, 
            request.Password, 
            request.DisplayName, 
            request.RoleName);
            
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }
}

public record CreateUserRequest(string Username, string Email, string Password, string DisplayName, string RoleName);
