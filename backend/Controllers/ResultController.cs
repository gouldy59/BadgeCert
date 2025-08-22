using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BadgeManagement.Data;
using BadgeManagement.Models;

namespace BadgeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    [Authorize]
    public class ResultController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var results = await _context.Results
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.AchievedDate)
                    .Select(r => new
                    {
                        id = r.Id,
                        title = r.Title,
                        description = r.Description,
                        status = r.Status,
                        achievedDate = r.AchievedDate,
                        score = r.Score,
                        badgeId = r.BadgeId
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve results", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var result = new Result
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    AchievedDate = request.AchievedDate,
                    Score = request.Score,
                    BadgeId = request.BadgeId,
                    UserId = userId.Value
                };

                _context.Results.Add(result);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = result.Id,
                    title = result.Title,
                    description = result.Description,
                    status = result.Status,
                    achievedDate = result.AchievedDate,
                    score = result.Score,
                    badgeId = result.BadgeId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create result", error = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public class CreateResultRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public DateTime AchievedDate { get; set; } = DateTime.UtcNow;
        public double? Score { get; set; }
        public Guid? BadgeId { get; set; }
    }
}
