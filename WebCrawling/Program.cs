using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebCrawling
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHost(args);
            try
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Called from Main");
                host.Run();
            }
            catch (Exception)
            {
                // do something
            }
        }

        public static IWebHost CreateWebHost(string[] args) =>
       WebHost.CreateDefaultBuilder(args)
           .UseStartup<Startup>()
           .ConfigureLogging(logging =>
           {
               logging.ClearProviders();
               logging.AddFilter("Microsoft", LogLevel.Warning);
               logging.AddFilter("System", LogLevel.Warning);
               logging.AddFilter("Hangfire", LogLevel.Warning);
               logging.AddConsole(); // Or any other provider
           }).Build();

    }
}
