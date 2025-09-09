using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProResults.Data;
using ProResults.Models;

namespace ProResults.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    public class ScoreReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        private string GetCurrentUserEmail()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return userEmail;
        }
    
        public ScoreReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetScoreReports()
        {
            try
            {
                var userId = GetCurrentUserEmail();
                if (userId == null) return Unauthorized();
                var results = await _context.ScoreReports
                                        .Where(r => r.email == userId)
                                        .OrderByDescending(r => r.AchievedDate)
                                        .Select(r => new
                                        {
                                            id = r.Id,
                                            title = r.Title,
                                            description = r.Description,
                                            status = r.Status,
                                            achievedDate = r.AchievedDate,
                                  
                                        })
                                        .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve results", error = ex.Message });
            }
        }

        [HttpGet("html/{scoreReportId}")]
        public async Task<IActionResult> GetScoreReportHtml(Guid scoreReportId)
        {
            try
            {
                var userId = GetCurrentUserEmail();
                //if (userId == null) return Unauthorized();
                userId = userId ?? "shane.gould@prometric.com";

                var scoreReport = await _context.ScoreReports
                    .Where(r => r.Id == scoreReportId).FirstOrDefaultAsync();


                string htmlString = System.Text.Encoding.UTF8.GetString(scoreReport.htmlBody);

                // Return as ContentResult with correct content type
                return Content(htmlString, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve results", error = ex.Message });
            }
        }

        [HttpGet("pdf/{scoreReportId}")]
        public async Task<IActionResult> GetScoreReportPdf(Guid scoreReportId)
        {
            try
            {
                var userId = GetCurrentUserEmail();
                //if (userId == null) return Unauthorized();
                userId = userId ?? "shane.gould@prometric.com";

                var scoreReport = await _context.ScoreReports
                    .Where(r => r.Id == scoreReportId).FirstOrDefaultAsync();
            

                var contentType = "application/pdf";
                var fileName = "ScoreReport.pdf"; // Or use scoreReport.Name or similar

                return File(scoreReport.PdfFile, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve results", error = ex.Message });
            }

        }

        [HttpGet("image/{scoreReportId}")]
            public async Task<IActionResult> GetScoreReportImage(Guid scoreReportId)
            {
                try
                {
                    var userId = GetCurrentUserEmail();
                //if (userId == null) return Unauthorized();
                userId = userId ?? "shane.gould@prometric.com";

                var scoreReport = await _context.ScoreReports
                        .Where(r => r.Id == scoreReportId).FirstOrDefaultAsync();

                    var contentType = "image/png"; // Set the correct mime type for your images
                    var fileName = "ScoreReport.png"; // Or use a property if you store the original file name

                    return File(scoreReport.ImageFile, contentType, fileName);
                }
            catch (Exception ex)
                {
                    return BadRequest(new { message = "Failed to retrieve results", error = ex.Message });
                }
            }
        //[HttpPost]
        //public async Task<IActionResult> CreateResult([FromBody] CreateResultRequest request)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        if (userId == null) return Unauthorized();

            //        var result = new Result
            //        {
            //            Id = Guid.NewGuid(),
            //            Title = request.Title,
            //            Description = request.Description,
            //            Status = request.Status,
            //            AchievedDate = request.AchievedDate,
            //            Score = request.Score,
            //            BadgeId = request.BadgeId,
            //            UserId = userId.Value
            //        };

            //        _context.Results.Add(result);
            //        await _context.SaveChangesAsync();

            //        return Ok(new
            //        {
            //            id = result.Id,
            //            title = result.Title,
            //            description = result.Description,
            //            status = result.Status,
            //            achievedDate = result.AchievedDate,
            //            score = result.Score,
            //            badgeId = result.BadgeId
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        return BadRequest(new { message = "Failed to create result", error = ex.Message });
            //    }
            //}

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
