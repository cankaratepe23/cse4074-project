using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CriServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                services
                    .AddDbContext<CriContext>(options =>
                        options.UseNpgsql(configuration.GetConnectionString("PostgresDefault")))
                ).Build();
        }
    }
}
