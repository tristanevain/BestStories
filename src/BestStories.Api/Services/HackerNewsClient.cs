using Microsoft.Extensions.Caching.Memory;

namespace BestStories.Api.Services;

public record HackerNewsItem(int Id, string? By, long Time, string? Title, string? Url, int? Score, int? Descendants, string? Type);

public class HackerNewsClient(IHttpClientFactory httpFactory, IMemoryCache cache, IConfiguration configuration)
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly string BestStoriesEndpoint = configuration.GetSection("HackerNews").GetValue<string>("BestStoriesEndpoint");

    public async Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
    {
        // cache for short time to avoid hammering HN when many callers ask concurrently
        var cacheItem = await _cache.GetOrCreateAsync("beststoryids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            var client = _httpFactory.CreateClient(Constants.HackerNewsHttpClientName);

            var res = await client.GetFromJsonAsync<int[]>(BestStoriesEndpoint, ct);

            return res ?? [];
        });

        return cacheItem ?? [];
    }


    public async Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken ct)
    {
        string key = $"item_{id}";

        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var client = _httpFactory.CreateClient("hn");
            // item/{id}.json
            var res = await client.GetFromJsonAsync<HackerNewsItem>($"item/{id}.json", ct);
            return res;
        });
    }
}