using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Passport.Application.Features.Agents.Commands;
using QuestFlag.Passport.Application.Features.Agents.Queries;
using QuestFlag.Passport.Application.Common.DTOs;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PassportAdmin")]
public class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAgents()
    {
        var result = await _mediator.Send(new GetAgentsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentCommand command)
    {
        var clientId = await _mediator.Send(command);
        return Ok(new { clientId });
    }

    [HttpPut("{clientId}")]
    public async Task<IActionResult> UpdateAgent(string clientId, [FromBody] UpdateAgentCommand command)
    {
        if (clientId != command.ClientId) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteAgent(string clientId)
    {
        await _mediator.Send(new DeleteAgentCommand(clientId));
        return NoContent();
    }
}
