using BestStories.Api.Services;
using BestStories.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController(IStoryService storyService, IOptions<HackerNewsApiSettings> settings) : ControllerBase
{
    private readonly IStoryService _storyService = storyService;
    private readonly HackerNewsApiSettings _settings = settings.Value;

    [HttpGet]
    [Route("best")]
    public async Task<IActionResult> GetBestStories([FromQuery] int n = 10, CancellationToken ct = default)
    {
        if (n <= 0 || n > _settings.MaxN)
            return BadRequest($"n must be between 1 and {_settings.MaxN}");

        var stories = await _storyService.GetTopStoriesByScoreAsync(n, ct);

        return Ok(stories);
    }
}