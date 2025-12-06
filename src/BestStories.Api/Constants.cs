namespace BestStories.Api;

public class Constants
{
    public const string HackerNewsApiSettingsKey = "HackerNewsApi";
    public const string HackerNewsHttpClientName = "hacker-news";

    public static readonly TimeSpan BestStoriesIdsCacheDuration = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan StoryCacheDuration = TimeSpan.FromMinutes(5);
}
