using CriServer;
using CriServer.IServices;
using CriServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Net.Sockets;
using System.Text;

namespace CriServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            string connectionStringOption = "PostgresDefault";
            if (args.Length == 1 && args[0] == "--is-standalone")
            {
                connectionStringOption = "PostgresStandalone";
            }
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                services
                    .AddDbContext<CriContext>(options =>
                        options.UseNpgsql(configuration.GetConnectionString(connectionStringOption)))
                    .AddScoped<IUserService, UserService>()
                    .AddScoped<IGroupService, GroupService>()
                ).Build();
            CriContext dbContext = host.Services.GetService<CriContext>();
            dbContext.Database.EnsureCreated();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("/var/log/criserver/criserver.log", rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            RegistryServer registryServer = new RegistryServer(host.Services.GetService<IUserService>());
            registryServer.Start();

            Log.CloseAndFlush();
        }
    }
}
