using BestStories.Api;
using BestStories.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var hackerNewsSettings = builder.Configuration.GetSection("HackerNews");
var baseUrl = hackerNewsSettings.GetValue<string>("BaseUrl") 
                ?? throw new InvalidOperationException("HackerNews:BaseUrl configuration is missing.");


builder.Services.AddHttpClient(Constants.HackerNewsHttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IHackerNewsClient, HackerNewsClient>();
builder.Services.AddSingleton<IStoryService, StoryService>();


builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();