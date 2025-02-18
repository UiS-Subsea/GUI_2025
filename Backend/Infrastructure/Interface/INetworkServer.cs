using System.Threading.Channels;

namespace Backend.Infrastructure.Interface
{
    public interface INetworkServer
    {
        bool IsConnected { get; }
        ChannelReader<byte[]> SensorData { get; }  // Expose the channel reader for incoming data

        Task StartAsync(CancellationToken cancellationToken);  // Start the server
    }
}
