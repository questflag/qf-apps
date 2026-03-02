using MediatR;
using QuestFlag.Communication.Application.Features.Messages.Commands;
using QuestFlag.Communication.Domain.Entities;
using QuestFlag.Communication.Domain.Enums;
using QuestFlag.Communication.Domain.Interfaces;

namespace QuestFlag.Communication.Application.Features.Messages.Handlers;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, string>
{
    private readonly ICommunicationLogRepository _repository;
    private readonly IProviderResolver _resolver;

    public SendMessageCommandHandler(ICommunicationLogRepository repository, IProviderResolver resolver)
    {
        _repository = repository;
        _resolver = resolver;
    }

    public async Task<string> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var transactionId = Guid.NewGuid().ToString();
        
        var log = new CommunicationLog
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            Recipient = request.Message.Recipient,
            ChannelUsed = request.Message.ChannelType,
            Payload = request.Message.Payload,
            Status = MessageStatus.CREATED,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(log);

        // Logic to resolve provider and enqueue task would go here/Core layer
        // For now, just completing the command logic
        
        return transactionId;
    }
}
