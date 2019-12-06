using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteManager
{
    public static class CommandlineUtility
    {
        // https://stackoverflow.com/questions/13738168/run-command-line-code-programmatically-using-c-sharp
        public static void ExecuteCommand(string command, string workingDirectory)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/K " + command);

            if (string.IsNullOrWhiteSpace(workingDirectory) == false)
            {
                processInfo.WorkingDirectory = workingDirectory;
            }

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;

            var commandTask = Task.Run(() =>
            {
                using var process = Process.Start(processInfo);
                process.Kill();
                process.Close();
            });

            commandTask.Wait(TimeSpan.FromSeconds(2));

            //var output = new StringBuilder();

            //await Task.Run(() =>
            //{
            //    while (!process.StandardOutput.EndOfStream)
            //    {
            //        output.Append(process.StandardOutput.ReadLine());
            //    }
            //});

            //process.StandardInput.WriteLine("exit");
            //process.Close();
            //return output.ToString();
        }

        public static void BuildAndPublish(string workingDirectory)
        {
            string publishCommand = "dotnet msbuild -t:Publish -p:Configuration=Release";
            ExecuteCommand(publishCommand, workingDirectory);
        }

        public static void Copy(string sourcePath, string destinationPath, string workingDirectory)
        {
            string copyCommand = $"Xcopy /E /I {sourcePath} {destinationPath}";
            ExecuteCommand(copyCommand, workingDirectory);
        }
    }
}
