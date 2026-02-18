using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizGame.Api.Data;

namespace QuizGame.Api.Controllers;



/// <summary>
/// REST API controller for retrieving game history records.
/// Accessible at <c>GET /api/gamehistory</c>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GameHistoryController : ControllerBase
{
    private readonly QuizDbContext _dbContext;

    public GameHistoryController(QuizDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the 20 most recent completed games, ordered by most recently played.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of <see cref="QuizGame.Api.Models.GameHistory"/> records.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _dbContext.GameHistories
        .OrderByDescending(h => h.PlayedAt)
        .Take(20)
        .ToListAsync();


        return Ok(history);
    }
}