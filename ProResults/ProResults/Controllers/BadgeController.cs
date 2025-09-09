using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProResults.Data;
using ProResults.Models;
using ProResults.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Text.Json;

namespace ProResults.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    [Authorize]
    public class BadgeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BadgeValidationService _validationService;
        private readonly PdfService _pdfService;
        private readonly HttpClient _httpClient;
        public BadgeController(AppDbContext context, BadgeValidationService validationService, PdfService pdfService)
        {
            _context = context;
            _validationService = validationService;
            _pdfService = pdfService;
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<IActionResult> GetBadges()
        {
            try
            {
                var userId = GetCurrentUserEmail();
                if (userId == null) return Unauthorized();

                var url = $"https://localhost:7184/credentials/byUserId/{userId}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // Read and return the JSON credential as a string
                    var json = await response.Content.ReadAsStringAsync();

                      var credentials = JsonConvert.DeserializeObject<List<OpenBadgeCredentialWithProof>>(json);
                      var badges = credentials.Select(c => new
                      {
                          id = c.Id,
                          name = c.CredentialSubject.Achievement.Name,
                          description = c.CredentialSubject.Achievement.Description,
                          issuer = c.Issuer.Name,
                          issuedDate = c.ValidFrom,
                          expirationDate = c.ValidFrom, // maybe should be c.ExpirationDate if available?
                          imageUrl = c.CredentialSubject.Achievement.Image?.Id,
                          credentialJson = json,
                          isVerified = true
                      }).ToList();
                    return Ok(badges);

//var badges = await _context.Badges
//                    .Where(b => b.UserId == userId)
//                    .OrderByDescending(b => b.CreatedAt)
//                    .Select(b => new
//                    {
//                        id = b.Id,
//                        name = b.Name,
//                        description = b.Description,
//                        issuer = b.Issuer,
//                        issuedDate = b.IssuedDate,
//                        expirationDate = b.ExpirationDate,
//                        imageUrl = b.ImageUrl,
//                        isVerified = b.IsVerified,
//                        credentialJson = b.CredentialJson
//                    })
//                    .ToListAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve badges", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBadge([FromBody] BadgeRequest BadgeCredential)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // Validate OpenBadges v3.0 compliance
                var validationResult = _validationService.ValidateOpenBadgeV3(BadgeCredential.BadgeCredential);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Badge validation failed",
                        errors = validationResult.Errors
                    });
                }

                var badge = new Badge
                {
                    Id = Guid.NewGuid(),
                    Name = BadgeCredential.BadgeCredential.CredentialSubject.Achievement.Name,
                    Description = BadgeCredential.BadgeCredential.CredentialSubject.Achievement.Description,
                    Issuer = BadgeCredential.BadgeCredential.Issuer.Name,
                    IssuedDate = DateTime.Parse(BadgeCredential.BadgeCredential.ValidFrom),
                    ExpirationDate = !string.IsNullOrEmpty(BadgeCredential.BadgeCredential.ValidUntil)
                        ? DateTime.Parse(BadgeCredential.BadgeCredential.ValidUntil)
                        : null,
                    ImageUrl = BadgeCredential.BadgeCredential.CredentialSubject.Achievement.Image?.Id,
                    CredentialJson = JsonConvert.SerializeObject(BadgeCredential, Formatting.Indented),
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


        [HttpGet("{id}/verify")]
        public async Task<IActionResult> VerifyBadge(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var url = $"https://localhost:7184/credentials/{id}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response2 = await _httpClient.PostAsync("https://localhost:7184/credentials/verify-badge", stringContent);
                    var json2 = await response2.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<VerifyResponse>(json2);

                    return Ok(result.Valid);
                }

                return Ok(false);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to generate PDF", error = ex.Message });
            }
        }
        public class VerifyResponse
        {
            public bool Valid { get; set; }
        }


        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }


        private string GetCurrentUserEmail()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            return userEmail;
        }
    }
}
