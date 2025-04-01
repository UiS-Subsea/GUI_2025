
namespace Backend.Infrastructure
{
    public class PythonProcessService : BackgroundService
    {
        private readonly PythonProcessManager _pythonProcessManager;

        public PythonProcessService(PythonProcessManager pythonProcessManager)
        {
            _pythonProcessManager = pythonProcessManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start the Python process initially
            _pythonProcessManager.StartPythonProcess();

            while (!stoppingToken.IsCancellationRequested)
            {
                // Check if the Python process is still running
                if (!_pythonProcessManager.IsPythonProcessRunning())
                {
                    // If it's not running, restart the process
                    Console.WriteLine("Python process stopped, restarting...");
                    _pythonProcessManager.StartPythonProcess();
                }

                // Wait for a while before checking again
                await Task.Delay(5000, stoppingToken);  // Check every 5 seconds
            }

            // Ensure the process is stopped properly when the service is stopping
            _pythonProcessManager.StopPythonProcess();
        }
    }
}
