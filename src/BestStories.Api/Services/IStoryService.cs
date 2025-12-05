
using BestStories.Api.Model.Dtos;

namespace BestStories.Api.Services
{
    public interface IStoryService
    {
        Task<IEnumerable<StoryDto>> GetTopStoriesByScoreAsync(int n, CancellationToken ct);
    }
}