namespace BestStories.Api.Settings;

public class HackerNewsApiSettings
{
    public string BaseUrl { get; set; } = default!;
    public string BestStoriesEndpoint { get; set; } = default!;
    public int MaxN { get; set; }
}
