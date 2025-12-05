using BestStories.Api.Services;
using NSubstitute;

namespace BestStories.Api.Tests.Services;

public class StoryServiceTests
{
    [Fact]
    public async Task GetTopStoriesByScoreAsync_WhenCalled_ReturnsTopNStoriesByScore()
    {
        // arrange
        var hackerNewsClientMock = Substitute.For<IHackerNewsClient>();
        var storyService = new StoryService(hackerNewsClientMock);

        var storyIds = new int[] { 1, 2, 3, 4, 5 };
        hackerNewsClientMock.GetBestStoryIdsAsync(Arg.Any<CancellationToken>())
                            .Returns(Task.FromResult(storyIds));
        
        var stories = new Dictionary<int, Story>
        {
            { 1, new Story(1, "author1", 1620000000, "Title 1", "http://example.com/1", 10, 5, "story") },
            { 2, new Story(2, "author2", 1620000001, "Title 2", "http://example.com/2", 50, 10, "story") },
            { 3, new Story(3, "author3", 1620000002, "Title 3", "http://example.com/3", 30, 8, "story") },
            { 4, new Story(4, "author4", 1620000003, "Title 4", "http://example.com/4", 20, 6, "story") },
            { 5, new Story(5, "author5", 1620000004, "Title 5", "http://example.com/5", 40, 9, "story") },
        };
        foreach (var keyValuePair in stories)
        {
            hackerNewsClientMock.GetStoryAsync(keyValuePair.Key, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Story?>(keyValuePair.Value));
        }

        // act
        var result = await storyService.GetTopStoriesByScoreAsync(3, CancellationToken.None);
        
        // assert
        var expectedOrder = new[] { stories[2], stories[5], stories[3] };
        Assert.Equal(expectedOrder.Select(s => s.Title), result.Select(dto => dto.Title));
    }

    [Fact]
    public async Task GetTopStoriesByScoreAsync_WhenNoStories_ReturnsEmptyList()
    {
        // arrange
        var hackerNewsClientMock = Substitute.For<IHackerNewsClient>();
        var storyService = new StoryService(hackerNewsClientMock);
        hackerNewsClientMock.GetBestStoryIdsAsync(Arg.Any<CancellationToken>())
                            .Returns(Task.FromResult(Array.Empty<int>()));
        
        // act
        var result = await storyService.GetTopStoriesByScoreAsync(5, CancellationToken.None);
        
        // assert
        Assert.Empty(result);
    }
}
