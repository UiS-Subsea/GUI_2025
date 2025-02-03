using System.Threading.Channels;

namespace Backend.Infrastructure
{

    public class CommandQueueService<T>
    {
        private readonly Channel<T> _channel;

        public CommandQueueService()
        {
            // Unbounded channel for fastest processing
            _channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = true,  // Optimized for one consumer
                SingleWriter = false  // Allow multiple producers
            });
        }

        // Enqueue a command (Producer)
        public async Task<bool> EnqueueAsync(T command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _channel.Writer.WaitToWriteAsync(cancellationToken))
            {
                await _channel.Writer.WriteAsync(command, cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Queue enqueue error: {ex.Message}");
            return false;
        }
    }

        // Dequeue commands (Consumer)
        public async Task<T?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _channel.Reader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Queue dequeue canceled.");
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Queue dequeue error: {ex.Message}");
                return default;
            }
        }

        public ChannelReader<T> Reader => _channel.Reader;
        public ChannelWriter<T> Writer => _channel.Writer;
    }
}
