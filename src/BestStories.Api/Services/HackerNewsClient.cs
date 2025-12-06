using BestStories.Api.Model;
using BestStories.Api.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Services;

public sealed class HackerNewsClient(IHttpClientFactory httpFactory, IMemoryCache cache, IOptions<HackerNewsApiSettings> settings) : IHackerNewsClient
{
    private const string BestStoriesCacheKey = "beststoryids";

    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly IMemoryCache _cache = cache;
    private readonly HackerNewsApiSettings _settings = settings.Value;

    public async Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
    {
        if(!_cache.TryGetValue<int[]>(BestStoriesCacheKey, out var cachedIds))
        {
            var client = _httpFactory.CreateClient(Constants.HackerNewsHttpClientName);
            var response = await client.GetFromJsonAsync<int[]>(_settings.BestStoriesEndpoint, ct);

            // cache for short time to avoid overloading HackerNews with many concurrent requests
            _cache.Set(BestStoriesCacheKey, response ?? [], TimeSpan.FromSeconds(30));
            cachedIds = response;
        }

        return cachedIds ?? [];
    }

    public async Task<Story?> GetStoryAsync(int id, CancellationToken ct)
    {
        string cacheKey = $"item_{id}";

        if(!_cache.TryGetValue<Story?>(cacheKey, out var cachedStory))
        {
            var client = _httpFactory.CreateClient(Constants.HackerNewsHttpClientName);

            var story = await client.GetFromJsonAsync<Story>($"item/{id}.json", ct);

            // cache for 5 minutes, reasonable duration for fresh story data
            _cache.Set(cacheKey, story, TimeSpan.FromMinutes(5));
            cachedStory = story;
        }

        return cachedStory;
    }
}