using BestStories.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStories.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BestStoriesController(IStoryService storyService) : ControllerBase
{
    private readonly IStoryService _storyService = storyService;
    private const int MaxN = 100; // todo: use settings

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int n = 10, CancellationToken ct = default)
    {
        if (n <= 0 || n > MaxN) 
            return BadRequest($"n must be between 1 and {MaxN}");

        var stories = await _storyService.GetTopByScoreAsync(n, ct);

        return Ok(stories);
    }
}