using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizGame.Api.Data;

namespace QuizGame.Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class GameHistoryController : ControllerBase
{
    private readonly QuizDbContext _dbContext;

    public GameHistoryController(QuizDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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