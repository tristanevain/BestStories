using BestStories.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
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
        var configurationMock = Substitute.For<IConfiguration>();

        var cachedIds = new int[] { 1, 2, 3, 4, 5 };
        memoryCacheMock.TryGetValue("beststoryids", out Arg.Any<int[]?>())
                       .Returns(callInfo =>
                       {
                           callInfo[1] = cachedIds; // set out parameter
                           return true;             // indicate cache hit
                       });

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, "https://hacker-news.firebaseio.com/v0/beststories.json");

        // act
        var result = await client.GetBestStoryIdsAsync(CancellationToken.None);

        // assert
        client.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedIds);
        httpClientFactoryMock.DidNotReceive().CreateClient(Arg.Any<string>());
    }

    [Fact]
    public async Task GetBestStoryIdsAsync_WhenCacheNotHit_CallsHackerNewsApi_AndStoresTheValueInCache()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();

        memoryCacheMock.TryGetValue("beststoryids", out Arg.Any<int[]?>())
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
        var httpClient = new HttpClient(handler);
        httpClientFactoryMock.CreateClient(Constants.HackerNewsHttpClientName)
                             .Returns(httpClient);

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, "https://hacker-news.firebaseio.com/v0/beststories.json");

        // act
        var result = await client.GetBestStoryIdsAsync(CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(ids);
        httpClientFactoryMock.Received(1)
                             .CreateClient(Arg.Any<string>());
        memoryCacheMock.Received(1)
                       .Set("beststoryids",
                           ids,
                           TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task GetStoryAsync_WhenCacheHit_ReturnsCachedStory()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        
        var cachedStory = new Story(1, "author", 1633036800, "Test Story", "https://example.com", 100, 50, "story");
        
        memoryCacheMock.TryGetValue("item_1", out Arg.Any<Story?>())
                       .Returns(callInfo =>
                       {
                           callInfo[1] = cachedStory; // set out parameter
                           return true;               // indicate cache hit
                       });

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, "https://hacker-news.firebaseio.com/v0/beststories.json");
        
        // act
        var result = await client.GetStoryAsync(1, CancellationToken.None);
        
        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedStory);
        httpClientFactoryMock.DidNotReceive().CreateClient(Arg.Any<string>());
    }

    [Fact]
    public async Task GetStoryAsync_WhenCacheNotHit_CallsHackerNewsApi_AndStoresTheValueInCache()
    {
        // arrange
        var memoryCacheMock = Substitute.For<IMemoryCache>();
        var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
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
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };
        httpClientFactoryMock.CreateClient(Constants.HackerNewsHttpClientName)
                             .Returns(httpClient);

        var client = new HackerNewsClient(httpClientFactoryMock, memoryCacheMock, "https://hacker-news.firebaseio.com/v0/beststories.json");
        
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
