using System.Threading.Channels;

namespace Backend.Infrastructure.Interface
{
    public interface ICommandQueueService<T>
    {
        Task<bool> EnqueueAsync(T command, CancellationToken cancellationToken = default);
        Task<T?> DequeueAsync(CancellationToken cancellationToken = default);
        ChannelReader<T> Reader { get; }
        ChannelWriter<T> Writer { get; }
    }
}