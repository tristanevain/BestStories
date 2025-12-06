using BestStories.Api.Controllers;
using BestStories.Api.Services;
using BestStories.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace BestStories.Api.Tests.Controllers;

public class StoryControllerTests
{
    [Fact]
    public async Task GetBestStories_WhenNLowerThan1_ThenReturnBadRequest()
    {
        // arrange
        var storyServiceMock = Substitute.For<IStoryService>();
        var controller = new StoriesController(storyServiceMock, Options.Create(new HackerNewsApiSettings()));

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
        var settings = Options.Create(new HackerNewsApiSettings { MaxN = 100 });
        var controller = new StoriesController(storyServiceMock, settings);

        // act
        var result = await controller.GetBestStories(settings.Value.MaxN + 1);

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBestStories_WhenNIsValid_ThenReturnOk()
    {
        // arrange
        var storyServiceMock = Substitute.For<IStoryService>();
        var settings = Options.Create(new HackerNewsApiSettings { MaxN = 100 });
        var controller = new StoriesController(storyServiceMock, settings);

        // act
        var result = await controller.GetBestStories(settings.Value.MaxN);

        // assert
        Assert.IsType<OkObjectResult>(result);
        await storyServiceMock.Received(1).GetTopStoriesByScoreAsync(settings.Value.MaxN, Arg.Any<CancellationToken>());
    }
}
