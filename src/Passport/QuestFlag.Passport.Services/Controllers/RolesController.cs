using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Passport.Application.Features.Roles.Commands;
using QuestFlag.Passport.Application.Features.Roles.Queries;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TenantAdmin")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _mediator.Send(new GetRolesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(System.Guid id, [FromBody] UpdateRoleCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(System.Guid id)
    {
        await _mediator.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}
