using MediatR;
using QuestFlag.Communication.Application.Features.Conversations.Commands;
using QuestFlag.Communication.Domain.Entities;
using QuestFlag.Communication.Domain.Enums;
using QuestFlag.Communication.Domain.Contracts;
using QuestFlag.Communication.Domain.ValueObjects;

namespace QuestFlag.Communication.Application.Features.Conversations.Handlers;

public class ConversationCommandHandler : 
    IRequestHandler<ProcessInboundWebhookCommand, Unit>,
    IRequestHandler<CloseConversationCommand, Unit>
{
    private readonly IConversationThreadRepository _repository;

    public ConversationCommandHandler(IConversationThreadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(ProcessInboundWebhookCommand request, CancellationToken cancellationToken)
    {
        var thread = await _repository.GetActiveByParticipantAsync(request.TenantId.ToString(), request.Recipient);
        
        if (thread == null)
        {
            thread = new ConversationThread
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId.ToString(),
                Status = ConversationStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow,
                Messages = new List<Message>()
            };
            await _repository.AddAsync(thread);
        }

        thread.Messages.Add(new Message("User", request.Content, DateTime.UtcNow));
        thread.Status = ConversationStatus.ACTIVE;
        
        await _repository.UpdateAsync(thread);
        
        return Unit.Value;
    }

    public async Task<Unit> Handle(CloseConversationCommand request, CancellationToken cancellationToken)
    {
        await _repository.ArchiveAsync(request.ConversationId);
        return Unit.Value;
    }
}
