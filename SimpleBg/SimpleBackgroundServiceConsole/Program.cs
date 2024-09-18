using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
           .ConfigureServices((_, services) =>
               services.AddHostedService<MyBackgroundService>())
           .ConfigureLogging(logging =>
           {
             logging.ClearProviders();
             logging.AddConsole();
           })
           .Build();


await host.RunAsync();

public class MyBackgroundService : BackgroundService
{
  private readonly ILogger<MyBackgroundService> _logger;

  public MyBackgroundService(ILogger<MyBackgroundService> logger)
  {
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Background service is starting.");

    while (!stoppingToken.IsCancellationRequested)
    {
      _logger.LogInformation("Background service is working.");
      await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }

    _logger.LogInformation("Background service is stopping.");
  }
}