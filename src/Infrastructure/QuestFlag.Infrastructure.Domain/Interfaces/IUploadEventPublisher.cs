using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Infrastructure.Domain.Events;

namespace QuestFlag.Infrastructure.Domain.Interfaces;

public interface IUploadEventPublisher
{
    Task PublishUploadCompletedAsync(UploadCompletedEvent @event, CancellationToken ct = default);
}
