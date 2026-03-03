using QuestFlag.Communication.Domain.Events;

namespace QuestFlag.Communication.Domain.Contracts;

public interface IUploadEventPublisher
{
    Task PublishUploadCompletedAsync(UploadCompletedEvent @event, CancellationToken ct = default);
}
