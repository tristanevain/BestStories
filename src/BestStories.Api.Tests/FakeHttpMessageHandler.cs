namespace BestStories.Api.Tests;

internal class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    private readonly HttpResponseMessage _response = response;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(_response);
}
