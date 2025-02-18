
namespace Backend.Infrastructure.Interface
{
    public interface INetworkClient
    {
        bool IsConnected { get; }  // Check if the client is connected
        Task StartAsync(CancellationToken cancellationToken);  // Start the client
        Task SendAsync<T>(T data, CancellationToken cancellationToken);  // Send data to the ROV
    }
}
