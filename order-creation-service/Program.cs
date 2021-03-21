using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Massive.SqlServer;
using Common.DataAccess;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;

namespace order_creation_service
{
   public class Program
   {
      public static void Main(string[] args)
      {
         Log.Logger = new LoggerConfiguration()
         .Enrich.FromLogContext()
         .WriteTo.File("order-creation-service.log")
         .CreateLogger();

         try
         {
            Log.Information("Starting up");
            CreateHostBuilder(args).Build().Run();
         }
         catch (Exception ex)
         {
            Log.Fatal(ex, "Application start-up failed");
         }
         finally
         {
            Log.CloseAndFlush();
         }

         Console.ReadKey();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
      .UseSerilog()
          .ConfigureServices((hostContext, services) =>
          {
             services.AddHostedService<KafkaConsumerHostedService>();
             //services.AddHostedService<KafkaProducerHostedService>(); // debugging locally!
             services.AddSingleton<IConnectionStringProvider, AppSettingsBasedConfigurationProvider>();
             services.AddSingleton<IMassiveOrmDataAccess, MassiveOrmDataAccess>();

             services.AddSingleton<IOrderService, OrderService>();

               // Build configuration
               var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                 .AddJsonFile("appsettings.json", false)
                 .Build();

               // Add access to generic IConfigurationRoot
               services.AddSingleton<IConfigurationRoot>(configuration);

          });
   }
}
