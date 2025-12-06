using BestStories.Api;
using BestStories.Api.Services;
using BestStories.Api.Settings;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

var hackerNewsApiSettings = builder.Configuration
    .GetSection(Constants.HackerNewsApiSettingsKey)
    .Get<HackerNewsApiSettings>()
    ?? throw new InvalidOperationException($"Configuration section '{Constants.HackerNewsApiSettingsKey}' is missing.");

builder.Services.AddHttpClient(Constants.HackerNewsHttpClientName, client =>
{
    client.BaseAddress = new Uri(hackerNewsApiSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IHackerNewsClient, HackerNewsClient>();
builder.Services.AddSingleton<IStoryService, StoryService>();

builder.Services.Configure<HackerNewsApiSettings>(builder.Configuration.GetSection(Constants.HackerNewsApiSettingsKey));

builder.Services.AddControllers();


var app = builder.Build();

app.MapControllers();

app.Run();