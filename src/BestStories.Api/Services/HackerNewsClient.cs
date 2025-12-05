using Microsoft.Extensions.Caching.Memory;

namespace BestStories.Api.Services;

public record Story(int Id, string? By, long Time, string? Title, string? Url, int? Score, int? Descendants, string? Type);

public class HackerNewsClient(IHttpClientFactory httpFactory, IMemoryCache cache, IConfiguration configuration) : IHackerNewsClient
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly string BestStoriesEndpoint = configuration.GetSection("HackerNews")
                                                               .GetValue<string>("BestStoriesEndpoint")!;

    public async Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
    {
        // cache for short time to avoid hammering HN when many callers ask concurrently
        var cacheItem = await _cache.GetOrCreateAsync("beststoryids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            var client = _httpFactory.CreateClient(Constants.HackerNewsHttpClientName);

            var response = await client.GetFromJsonAsync<int[]>(BestStoriesEndpoint, ct);

            return response ?? [];
        });

        return cacheItem ?? [];
    }


    public async Task<Story?> GetStoryAsync(int id, CancellationToken ct)
    {
        string key = $"item_{id}";

        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var client = _httpFactory.CreateClient(Constants.HackerNewsHttpClientName);

            var story = await client.GetFromJsonAsync<Story>($"item/{id}.json", ct);

            return story;
        });
    }
}