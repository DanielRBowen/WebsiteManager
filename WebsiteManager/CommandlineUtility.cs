using System.Diagnostics;

namespace WebsiteManager
{
    public static class CommandlineUtility
    {
        /// <summary>
        /// https://stackoverflow.com/questions/13738168/run-command-line-code-programmatically-using-c-sharp
        /// https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
        /// </summary>
        /// <param name="command"></param>
        /// <param name="workingDirectory"></param>
        public static string ExecuteCommand(string command, string workingDirectory)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/K " + command + " & exit");

            if (string.IsNullOrWhiteSpace(workingDirectory) == false)
            {
                processInfo.WorkingDirectory = workingDirectory;
            }

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            using var process = new Process
            {
                StartInfo = processInfo,
                EnableRaisingEvents = true
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.Kill();
            process.Close();
            return output;
        }

        public static string BuildAndPublish(string workingDirectory)
        {
            string publishCommand = "dotnet msbuild -t:Publish -p:Configuration=Release";
            return ExecuteCommand(publishCommand, workingDirectory);
        }

        public static string Copy(string sourcePath, string destinationPath, string workingDirectory)
        {
            string copyCommand = $"Xcopy /E /I {sourcePath} {destinationPath}";
            return ExecuteCommand(copyCommand, workingDirectory);
        }
    }
}
