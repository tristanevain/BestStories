using System.Collections.Concurrent;

namespace BestStories.Api.Services;

public record StoryDto(string Title, string? Uri, string? PostedBy, string Time, int Score, int CommentCount);

public class StoryService(IHackerNewsClient hackerNewsClient) : IStoryService
{
    private readonly IHackerNewsClient _hackerNewsClient = hackerNewsClient;
    private readonly int _maxStoriesToScan = 500; // todo: configurable
    private readonly int _maxDegreeOfParallelism = 10; // todo: configurable


    public async Task<IEnumerable<StoryDto>> GetTopByScoreAsync(int n, CancellationToken ct)
    {
        var ids = await _hackerNewsClient.GetBestStoryIdsAsync(ct);
        if (ids.Length == 0)
            return [];

        var idsToScan = ids.Take(_maxStoriesToScan);
        var bag = new ConcurrentBag<StoryDto>();

        using var sem = new SemaphoreSlim(_maxDegreeOfParallelism);

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
