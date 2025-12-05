using BestStories.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStories.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController(IStoryService storyService) : ControllerBase
{
    private readonly IStoryService _storyService = storyService;

    [HttpGet]
    [Route("best")]
    public async Task<IActionResult> GetBestStories([FromQuery] int n = 10, CancellationToken ct = default)
    {
        if (n <= 0 || n > Constants.MaxN) 
            return BadRequest($"n must be between 1 and {Constants.MaxN}");

        var stories = await _storyService.GetTopStoriesByScoreAsync(n, ct);

        return Ok(stories);
    }
}