using Hangfire;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddHangfire(config => {
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

  config.UseSqlServerStorage(connectionString);
  config.UseColouredConsoleLogProvider(); 
});

builder.Services.AddHangfireServer();
var app = builder.Build();
app.MapHangfireDashboard();
app.MapGet("/", () => "Hello World!");

app.Run();
 