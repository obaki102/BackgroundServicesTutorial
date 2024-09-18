using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((context, services) =>
               {

                 services.AddTransient<IMyService, MyService>();
                 services.AddHostedService<MyHostedService>();
               })
               .Build();

await host.RunAsync();



public interface IMyService
{
  Task DoWorkAsync();
}

public class MyService : IMyService
{
  public async Task DoWorkAsync()
  {
    Console.WriteLine("Work started...");
    await Task.Delay(2000);
    Console.WriteLine("Work completed!");
  }
}


public class MyHostedService : IHostedService
{
  private readonly IMyService _myService;

  public MyHostedService(IMyService myService)
  {
    _myService = myService;
  }


  public async Task StartAsync(CancellationToken cancellationToken)
  {
    Console.WriteLine("Hosted service starting...");
    await _myService.DoWorkAsync();
    Console.WriteLine("Hosted service started.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    Console.WriteLine("Hosted service stopping...");
    return Task.CompletedTask;
  }
}