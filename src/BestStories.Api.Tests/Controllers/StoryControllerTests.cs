using BestStories.Api.Controllers;
using BestStories.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace BestStories.Api.Tests.Controllers;

public class StoryControllerTests
{
    [Fact]
    public async Task GetBestStories_WhenNLowerThan1_ThenReturnBadRequest()
    {
        // arrange
        var storyServiceMock = Substitute.For<IStoryService>();
        var controller = new BestStoriesController(storyServiceMock);

        // act
        var result = await controller.GetBestStories(0);

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBestStories_WhenNGreaterThanMaxN_ThenReturnBadRequest()
    {
        // arrange
        var storyServiceMock = Substitute.For<IStoryService>();
        var controller = new BestStoriesController(storyServiceMock);

        // act
        var result = await controller.GetBestStories(Constants.MaxN + 1);

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBestStories_WhenNIsValid_ThenReturnOk()
    {
        // arrange
        var storyServiceMock = Substitute.For<IStoryService>();
        var controller = new BestStoriesController(storyServiceMock);

        // act
        var result = await controller.GetBestStories(Constants.MaxN);

        // assert
        Assert.IsType<OkObjectResult>(result);
        await storyServiceMock.Received(1).GetTopStoriesByScoreAsync(Constants.MaxN, Arg.Any<CancellationToken>());
    }
}
