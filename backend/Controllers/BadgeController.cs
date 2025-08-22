using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BadgeManagement.Data;
using BadgeManagement.Models;
using BadgeManagement.Services;
using Newtonsoft.Json;

namespace BadgeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    [Authorize]
    public class BadgeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BadgeValidationService _validationService;
        private readonly PdfService _pdfService;

        public BadgeController(AppDbContext context, BadgeValidationService validationService, PdfService pdfService)
        {
            _context = context;
            _validationService = validationService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBadges()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var badges = await _context.Badges
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new
                    {
                        id = b.Id,
                        name = b.Name,
                        description = b.Description,
                        issuer = b.Issuer,
                        issuedDate = b.IssuedDate,
                        expirationDate = b.ExpirationDate,
                        imageUrl = b.ImageUrl,
                        isVerified = b.IsVerified,
                        credentialJson = b.CredentialJson
                    })
                    .ToListAsync();

                return Ok(badges);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve badges", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBadge([FromBody] OpenBadgeCredential badgeCredential)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // Validate OpenBadges v3.0 compliance
                var validationResult = _validationService.ValidateOpenBadgeV3(badgeCredential);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        message = "Badge validation failed", 
                        errors = validationResult.Errors 
                    });
                }

                var badge = new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = badgeCredential.CredentialSubject.Achievement.Name,
                    Description = badgeCredential.CredentialSubject.Achievement.Description,
                    Issuer = badgeCredential.Issuer.Name,
                    IssuedDate = DateTime.Parse(badgeCredential.ValidFrom),
                    ExpirationDate = !string.IsNullOrEmpty(badgeCredential.ValidUntil) 
                        ? DateTime.Parse(badgeCredential.ValidUntil) 
                        : null,
                    ImageUrl = badgeCredential.CredentialSubject.Achievement.Image?.Id,
                    CredentialJson = JsonConvert.SerializeObject(badgeCredential, Formatting.Indented),
                    IsVerified = validationResult.IsValid,
                    UserId = userId.Value
                };

                _context.Badges.Add(badge);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = badge.Id,
                    name = badge.Name,
                    description = badge.Description,
                    issuer = badge.Issuer,
                    issuedDate = badge.IssuedDate,
                    expirationDate = badge.ExpirationDate,
                    imageUrl = badge.ImageUrl,
                    isVerified = badge.IsVerified,
                    credentialJson = badge.CredentialJson
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create badge", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBadge(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var badge = await _context.Badges
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (badge == null)
                {
                    return NotFound(new { message = "Badge not found" });
                }

                _context.Badges.Remove(badge);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Badge deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete badge", error = ex.Message });
            }
        }

        [HttpGet("{id}/download/png")]
        public async Task<IActionResult> DownloadBadgePng(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var badge = await _context.Badges
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (badge == null)
                {
                    return NotFound(new { message = "Badge not found" });
                }

                // Generate SVG that can be converted to PNG
                var svgContent = _pdfService.GenerateBadgeSVG(badge);
                var fileName = $"{badge.Name.Replace(" ", "_")}_badge.svg";

                return File(System.Text.Encoding.UTF8.GetBytes(svgContent), "image/svg+xml", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to generate badge", error = ex.Message });
            }
        }

        [HttpGet("{id}/download/pdf")]
        public async Task<IActionResult> DownloadBadgePdf(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var badge = await _context.Badges
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (badge == null)
                {
                    return NotFound(new { message = "Badge not found" });
                }

                var pdfBytes = await _pdfService.GenerateBadgePDF(badge);
                var fileName = $"{badge.Name.Replace(" ", "_")}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to generate PDF", error = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
