using MediatR;
using QuestFlag.Communication.Application.Common.DTOs;
using QuestFlag.Communication.Application.Features.Messages.Queries;
using QuestFlag.Communication.Domain.Interfaces;

namespace QuestFlag.Communication.Application.Features.Messages.Handlers;

public class GetMessageStatusQueryHandler : IRequestHandler<GetMessageStatusQuery, MessageStatusDto?>
{
    private readonly ICommunicationLogRepository _repository;

    public GetMessageStatusQueryHandler(ICommunicationLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<MessageStatusDto?> Handle(GetMessageStatusQuery request, CancellationToken cancellationToken)
    {
        var log = await _repository.GetByTransactionIdAsync(request.TransactionId);
        if (log == null) return null;

        return new MessageStatusDto(log.TransactionId, log.Status, log.UpdatedAt);
    }
}
