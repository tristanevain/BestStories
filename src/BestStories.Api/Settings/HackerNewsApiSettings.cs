namespace BestStories.Api.Settings;

public class HackerNewsApiSettings
{
    public string BaseUrl { get; set; } = default!;
    public string BestStoriesEndpoint { get; set; } = default!;
    public int MaxN { get; set; } = 100; // default overriden in appsettings.json
    public int MaxStoriesToScan { get; set; } = 500; // default overriden in appsettings.json
    public int MaxDegreeOfParallelism { get; set; } = 10; // default overriden in appsettings.json
}
