using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Communication.Application.Features.Conversations.Commands;

namespace QuestFlag.Communication.ApiCore.Controllers;

[ApiController]
[Route("api/comm/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("webhooks/inbound")]
    public async Task<IActionResult> InboundWebhook([FromBody] ProcessInboundWebhookCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CloseConversation(Guid id)
    {
        await _mediator.Send(new CloseConversationCommand(id));
        return NoContent();
    }
}
