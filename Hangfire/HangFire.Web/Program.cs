using System.Text.Json;
using Hangfire;
using HangFire.Web;
using HangFire.Web.Jobs;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Services.AddHttpClient();
builder.Services.Configure<ConfigSettings>(builder.Configuration.GetSection("ConfigSettings"));
builder.Services.AddHangfire(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    config.UseSqlServerStorage(connectionString);
    config.UseColouredConsoleLogProvider();
});

builder.Services.AddHangfireServer();

var app = builder.Build();
app.MapHangfireDashboard();


var configOptions = app.Services.GetRequiredService<IOptions<ConfigSettings>>();
var config = configOptions.Value;

//Example:
//0   1   *   *   *
//^   ^   ^   ^   ^
//|   |   |   |   |
//|   |   |   |   |
//|   |   |   | Day of the week
//|   |   |   Month
//|   |   Day of the month
//|   Hour
//Minute

//Meaning: runs 1:00am every day, every month, every day of the week
//`*` means `every` or `any` value


//Example: *****
//Meaning: runs every minutes
//Recurring Job
RecurringJob.AddOrUpdate<WebPuller>("pull-rss-feed",
                                    p => p.GetRssItemUrlsAsync(config.RssUrl, config.TempPath),
                                    "* * * * *");
//Recurring Delete
RecurringJob.RemoveIfExists("id to delete");

app.MapGet("/trigger-recurringjob", (IBackgroundJobClient bg) =>
{
    RecurringJob.TriggerJob("pull-rss-feed");
});


app.MapGet("/on-demand", (IBackgroundJobClient bg) =>
{

    bg.Enqueue<WebPuller>(p => p.GetRssItemUrlsAsync(config.RssUrl, config.TempPath));
});

app.MapGet("/scheduled", (IBackgroundJobClient bg) =>
{
    var json = File.ReadAllText(config.TempPath);
    var rssItemUrls = JsonSerializer.Deserialize<List<string>>(json);

    var outputPath = Path.Combine(config.Directory, "output");
    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

    if (rssItemUrls == null || rssItemUrls.Count == 0) return;
    var delayInSeconds = 5;
    foreach (var url in rssItemUrls.Take(5))
    {
        var u = new Uri(url);
        var stub = u.Segments.Last();
        // trim trailing slash, if any and add .html
        if (stub.EndsWith("/")) stub = stub.Substring(0, stub.Length - 1);
        stub += ".html";

        var filePath = Path.Combine(outputPath, stub);

        bg.Schedule<WebPuller>(p => p.DownloadFileFromUrl(url, filePath),
            TimeSpan.FromSeconds(delayInSeconds));
        delayInSeconds += 5;
    }
});

app.Run();
