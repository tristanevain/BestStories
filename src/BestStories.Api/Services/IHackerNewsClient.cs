
using BestStories.Api.Model;

namespace BestStories.Api.Services
{
    public interface IHackerNewsClient
    {
        Task<int[]> GetBestStoryIdsAsync(CancellationToken ct);
        Task<Story?> GetStoryAsync(int id, CancellationToken ct);
    }
}