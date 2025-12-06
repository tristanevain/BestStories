
using BestStories.Api.Model;

namespace BestStories.Api.Services
{
    public interface IHackerNewsClient
    {
        Task<int[]> GetBestStoriesIdsAsync(CancellationToken ct);
        Task<Story?> GetStoryAsync(int id, CancellationToken ct);
    }
}