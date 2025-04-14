using System.Diagnostics;

namespace Backend.Infrastructure
{
    public class PythonProcessManager
    {
        private Process _pythonProcess = null!;
        private readonly string _pythonExe = "python";

        private readonly string _pythonScriptPath;

        public PythonProcessManager()
        {
            // Get the path of the root project directory dynamically
            string projectRoot = Directory.GetCurrentDirectory();
            
            // Build the full path to your main.py script
            _pythonScriptPath = Path.Combine(projectRoot, "PythonScripts", "main.py");
        }

        public void StartPythonProcess()
        {
            if (_pythonProcess != null && !_pythonProcess.HasExited)
            {
                Console.WriteLine("Python process is already running.");
                return;  // Prevent starting multiple instances of the Python process
            }

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = _pythonExe,
                //Arguments = _pythonScriptPath,
                Arguments = $"-u {_pythonScriptPath}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_pythonScriptPath), // Set working directory to script location
            };

            try
            {
                // Start the process
                _pythonProcess = Process.Start(startInfo);
                if (_pythonProcess != null)
                {
                    _pythonProcess.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
                    _pythonProcess.ErrorDataReceived += (sender, e) => { Console.WriteLine("ERROR: " + e.Data); };

                    // Start reading the output/error asynchronously to avoid blocking
                    _pythonProcess.BeginOutputReadLine();
                    _pythonProcess.BeginErrorReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start Python process: " + ex.Message);
            }
        }

        public bool IsPythonProcessRunning() => _pythonProcess != null && !_pythonProcess.HasExited;

        public void StopPythonProcess()
        {
            if (_pythonProcess != null && !_pythonProcess.HasExited)
            {
                _pythonProcess.StandardInput.WriteLine("shutdown"); //Send shutdown command to python process to start cleanup
                _pythonProcess.WaitForExit();  // Wait for Python process to finish its cleanup
                _pythonProcess.Dispose();
                _pythonProcess = null!;
            }
        }
    }
}
