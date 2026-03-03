using QuestFlag.Infrastructure.Domain.Events;

namespace QuestFlag.Infrastructure.Domain.Contracts;

public interface IUploadEventPublisher
{
    Task PublishUploadCompletedAsync(UploadCompletedEvent @event, CancellationToken ct = default);
}
