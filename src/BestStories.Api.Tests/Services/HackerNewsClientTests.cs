using BestStories.Api.Model;
using BestStories.Api.Services;
using BestStories.Api.Settings;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BestStories.Api.Tests.Services;

public class HackerNewsClientTests
{
    [Fact]
    public async Task GetBestStoryIdsAsync_WhenCacheHit_ReturnsCachedIds()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var settings = Options.Create(new HackerNewsApiSettings
        {
            BaseUrl = "https://hacker-news.firebaseio.com/v0/", // will not be actually called
            BestStoriesEndpoint = "beststories.json"
        });

        var cachedIds = new int[] { 1, 2, 3, 4, 5 };
        memoryCacheMock.TryGetValue("beststoriesids", out Arg.Any<int[]?>())
                       .Returns(callInfo =>
                       {
                           callInfo[1] = cachedIds; // set out parameter
                           return true;             // indicate cache hit
                       });

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, settings);

        // act
        var result = await client.GetBestStoriesIdsAsync(CancellationToken.None);

        // assert
        client.Should().NotBeNull();
        result.Should().Equal(cachedIds);
        httpClientFactoryMock.DidNotReceive().CreateClient(Arg.Any<string>());
    }

    [Fact]
    public async Task GetBestStoriesIdsAsync_WhenCacheNotHit_CallsHackerNewsApi_AndStoresTheValueInCache()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var settings = Options.Create(new HackerNewsApiSettings
        {
            BaseUrl = "https://hacker-news.firebaseio.com/v0/", // will not be actually called
            BestStoriesEndpoint = "beststories.json"
        });

        memoryCacheMock.TryGetValue("beststoriesids", out Arg.Any<int[]?>())
                       .Returns(false); // indicate cache miss

        var ids = new int[] { 1, 2, 3, 4, 5 };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(ids),
                Encoding.UTF8,
                "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(settings.Value.BaseUrl)
        };
        httpClientFactoryMock.CreateClient(Constants.HackerNewsHttpClientName)
                             .Returns(httpClient);

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, settings);

        // act
        var result = await client.GetBestStoriesIdsAsync(CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Should().Equal(ids);
        httpClientFactoryMock.Received(1)
                             .CreateClient(Arg.Any<string>());
        memoryCacheMock.Received(1)
                       .Set("beststoriesids",
                           ids,
                           TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task GetStoryAsync_WhenCacheHit_ReturnsCachedStory()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var settings = Options.Create(new HackerNewsApiSettings
        {
            BaseUrl = "https://hacker-news.firebaseio.com/v0/", // will not be actually called
            BestStoriesEndpoint = "beststories.json"
        });

        var cachedStory = new Story(1, "author", 1633036800, "Test Story", "https://example.com", 100, 50, "story");

        memoryCacheMock.TryGetValue("item_1", out Arg.Any<Story?>())
                       .Returns(callInfo =>
                       {
                           callInfo[1] = cachedStory; // set out parameter
                           return true;               // indicate cache hit
                       });

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, settings);

        // act
        var result = await client.GetStoryAsync(1, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(cachedStory);
        httpClientFactoryMock.DidNotReceive().CreateClient(Arg.Any<string>());
    }

    [Fact]
    public async Task GetStoryAsync_WhenCacheNotHit_CallsHackerNewsApi_AndStoresTheValueInCache()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        var settings = Options.Create(new HackerNewsApiSettings
        {
            BaseUrl = "https://hacker-news.firebaseio.com/v0/", // will not be actually called
            BestStoriesEndpoint = "beststories.json"
        });

        memoryCacheMock.TryGetValue("item_1", out Arg.Any<Story?>())
                       .Returns(false); // indicate cache miss

        var story = new Story(1, "author", 1633036800, "Test Story", "https://example.com", 100, 50, "story");

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(story),
                Encoding.UTF8,
                "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(settings.Value.BaseUrl)
        };
        httpClientFactoryMock.CreateClient(Constants.HackerNewsHttpClientName)
                             .Returns(httpClient);

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, settings);

        // act
        var result = await client.GetStoryAsync(1, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(story);
        httpClientFactoryMock.Received(1)
                             .CreateClient(Arg.Any<string>());
        memoryCacheMock.Received(1)
                       .Set("item_1", story, TimeSpan.FromMinutes(5));
    }
}
