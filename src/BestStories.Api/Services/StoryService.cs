using BestStories.Api.Model.Dtos;
using BestStories.Api.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace BestStories.Api.Services;

public sealed class StoryService(IHackerNewsClient hackerNewsClient, IOptions<HackerNewsApiSettings> settings) : IStoryService
{
    private readonly IHackerNewsClient _hackerNewsClient = hackerNewsClient;
    private readonly HackerNewsApiSettings _settings = settings.Value;


    public async Task<IEnumerable<StoryDto>> GetTopStoriesByScoreAsync(int n, CancellationToken ct)
    {
        var ids = await _hackerNewsClient.GetBestStoriesIdsAsync(ct);
        if (ids.Length == 0)
            return [];

        var idsToScan = ids.Take(_settings.MaxStoriesToScan);
        var bag = new ConcurrentBag<StoryDto>();

        using var sem = new SemaphoreSlim(_settings.MaxDegreeOfParallelism);

        var tasks = idsToScan.Select(async id =>
        {
            await sem.WaitAsync(ct);

            try
            {
                var item = await _hackerNewsClient.GetStoryAsync(id, ct);
                if (item != null && item.Type == "story")
                {
                    bag.Add(new StoryDto(
                        item.Title ?? string.Empty,
                        item.Url,
                        item.By,
                        DateTimeOffset.FromUnixTimeSeconds(item.Time).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        item.Score ?? 0,
                        item.Descendants ?? 0
                    ));
                }
            }
            finally
            {
                sem.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        var topStories = bag.OrderByDescending(s => s.Score)
                            .Take(n)
                            .ToArray();

        return topStories;
    }
}
