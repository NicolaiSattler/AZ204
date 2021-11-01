using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace TrainingAZ204
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var directory = $"{Directory.GetCurrentDirectory()}/TrainingAZ204/";
            return Host.CreateDefaultBuilder(args)
                   .UseContentRoot(directory)
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                       webBuilder.ConfigureAppConfiguration((context, config) => {
                           config.AddJsonFile("appsettings.json");
                           config.AddJsonFile("appsettings.Development.json");
                       });

                       webBuilder.UseStartup<Startup>();
                   });
        }
    }
}
