using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OriginalWeb
{
    public class Program
    {
        private static int port = 50000;
        public static void Main(string[] args)
        {
            port = GetOpenPort(port);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://localhost:{port}");
                });

        /// <summary>
        /// https://stackoverflow.com/questions/679489/determine-if-port-is-in-use
        /// </summary>
        /// <param name="startPort"></param>
        /// <returns></returns>
        public static int GetOpenPort(int startPort = 2555)
        {
            int portStartIndex = startPort;
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();
            int unusedPort = 0;
            unusedPort = Enumerable.Range(portStartIndex, 99).Where(port => !usedPorts.Contains(port)).FirstOrDefault();
            return unusedPort;
        }
    }
}
