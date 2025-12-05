
namespace BestStories.Api.Services
{
    public interface IStoryService
    {
        Task<IEnumerable<StoryDto>> GetTopByScoreAsync(int n, CancellationToken ct);
    }
}