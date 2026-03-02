using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Communication.Application.Common.DTOs;
using QuestFlag.Communication.Application.Features.Messages.Commands;
using QuestFlag.Communication.Application.Features.Messages.Queries;

namespace QuestFlag.Communication.ApiCore.Controllers;

[ApiController]
[Route("api/comm/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var transactionId = await _mediator.Send(new SendMessageCommand(dto));
        return Accepted(new { transactionId });
    }

    [HttpGet("{transactionId}/status")]
    public async Task<IActionResult> GetStatus(string transactionId)
    {
        var status = await _mediator.Send(new GetMessageStatusQuery(transactionId));
        return status != null ? Ok(status) : NotFound();
    }
}
