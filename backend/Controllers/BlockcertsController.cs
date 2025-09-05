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
    [Route("api/blockcerts")]
    [Authorize]
    public class BlockcertsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BlockcertsService _blockcertsService;

        public BlockcertsController(AppDbContext context, BlockcertsService blockcertsService)
        {
            _context = context;
            _blockcertsService = blockcertsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCertificates()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var certificates = await _context.BlockcertsCertificates
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        description = c.Description,
                        issuerName = c.IssuerName,
                        recipientId = c.RecipientId,
                        issuanceDate = c.IssuanceDate,
                        expirationDate = c.ExpirationDate,
                        imageUrl = c.ImageUrl,
                        isAnchored = c.IsAnchored,
                        transactionId = c.TransactionId,
                        blockchainNetwork = c.BlockchainNetwork,
                        isRevoked = c.IsRevoked,
                        credentialJson = c.CredentialJson
                    })
                    .ToListAsync();

                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve certificates", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificate(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var certificate = await _context.BlockcertsCertificates
                    .Where(c => c.Id == id && c.UserId == userId)
                    .FirstOrDefaultAsync();

                if (certificate == null)
                    return NotFound(new { message = "Certificate not found" });

                return Ok(new
                {
                    id = certificate.Id,
                    name = certificate.Name,
                    description = certificate.Description,
                    issuerName = certificate.IssuerName,
                    recipientId = certificate.RecipientId,
                    issuanceDate = certificate.IssuanceDate,
                    expirationDate = certificate.ExpirationDate,
                    imageUrl = certificate.ImageUrl,
                    isAnchored = certificate.IsAnchored,
                    transactionId = certificate.TransactionId,
                    blockchainNetwork = certificate.BlockchainNetwork,
                    merkleRoot = certificate.MerkleRoot,
                    isRevoked = certificate.IsRevoked,
                    credentialJson = certificate.CredentialJson,
                    createdAt = certificate.CreatedAt,
                    updatedAt = certificate.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve certificate", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCertificate([FromBody] CreateCertificateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                // Create issuer
                var issuer = new BlockcertsIssuer
                {
                    Context = new[] { "https://www.w3.org/2018/credentials/v1", "https://www.blockcerts.org/schema/3.0/context.json" },
                    Type = new[] { "Profile", "Issuer" },
                    Id = request.IssuerUrl ?? $"https://example.com/issuers/{Guid.NewGuid()}",
                    Name = request.IssuerName,
                    Url = request.IssuerUrl,
                    Email = request.IssuerEmail,
                    PublicKey = new[]
                    {
                        new BlockcertsPublicKey
                        {
                            Id = $"https://example.com/keys/{Guid.NewGuid()}",
                            Created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            PublicKeyPem = request.PublicKeyPem ?? GenerateSimulatedPublicKey()
                        }
                    }
                };

                // Create credential
                var credential = _blockcertsService.CreateCredential(
                    request.RecipientId,
                    request.RecipientName,
                    request.BadgeName,
                    request.BadgeDescription,
                    issuer,
                    request.ExpirationDate
                );

                // Validate credential
                var validationResult = _blockcertsService.ValidateBlockcertsCredential(credential);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { message = "Invalid credential", errors = validationResult.Errors });
                }

                // Store in database
                var certificate = new BlockcertsCertificate
                {
                    Id = Guid.NewGuid(),
                    Name = request.BadgeName,
                    Description = request.BadgeDescription,
                    IssuerName = request.IssuerName,
                    RecipientId = request.RecipientId,
                    IssuanceDate = DateTime.Parse(credential.IssuanceDate),
                    ExpirationDate = credential.ExpirationDate != null ? DateTime.Parse(credential.ExpirationDate) : null,
                    ImageUrl = request.ImageUrl,
                    CredentialJson = JsonConvert.SerializeObject(credential, Formatting.Indented),
                    UserId = userId.Value,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BlockcertsCertificates.Add(certificate);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCertificate), new { id = certificate.Id }, new
                {
                    id = certificate.Id,
                    message = "Certificate created successfully",
                    credential = credential
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create certificate", error = ex.Message });
            }
        }

        [HttpPost("{id}/anchor")]
        public async Task<IActionResult> AnchorCertificate(Guid id, [FromBody] AnchorCertificateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var certificate = await _context.BlockcertsCertificates
                    .Where(c => c.Id == id && c.UserId == userId)
                    .FirstOrDefaultAsync();

                if (certificate == null)
                    return NotFound(new { message = "Certificate not found" });

                if (certificate.IsAnchored)
                    return BadRequest(new { message = "Certificate is already anchored" });

                // Parse credential from JSON
                var credential = JsonConvert.DeserializeObject<BlockcertsCredential>(certificate.CredentialJson);
                if (credential == null)
                    return BadRequest(new { message = "Invalid credential data" });

                // Hash the credential
                var targetHash = _blockcertsService.HashCredential(credential);

                // Create Merkle proof (simplified - in production, this would be part of a batch)
                var merkleRoot = targetHash; // For single credential, target hash becomes merkle root
                var proofPath = Array.Empty<MerkleProofData>(); // No siblings for single credential

                // Anchor to blockchain
                BlockchainAnchor anchor;
                if (request.Network?.ToLower() == "ethereum")
                {
                    anchor = await _blockcertsService.AnchorToEthereumAsync(merkleRoot, request.Testnet);
                }
                else
                {
                    anchor = await _blockcertsService.AnchorToBitcoinAsync(merkleRoot, request.Testnet);
                }

                // Create proof
                var proof = _blockcertsService.CreateMerkleProof(targetHash, merkleRoot, proofPath, new[] { anchor });
                credential.Proof = proof;

                // Update certificate in database
                certificate.CredentialJson = JsonConvert.SerializeObject(credential, Formatting.Indented);
                certificate.IsAnchored = true;
                certificate.TransactionId = anchor.Chain.TransactionId;
                certificate.BlockchainNetwork = anchor.Chain.Name;
                certificate.MerkleRoot = merkleRoot;
                certificate.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Certificate anchored successfully",
                    transactionId = anchor.Chain.TransactionId,
                    blockchainNetwork = anchor.Chain.Name,
                    merkleRoot = merkleRoot,
                    proof = proof
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to anchor certificate", error = ex.Message });
            }
        }

        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyCertificate(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var certificate = await _context.BlockcertsCertificates
                    .Where(c => c.Id == id && c.UserId == userId)
                    .FirstOrDefaultAsync();

                if (certificate == null)
                    return NotFound(new { message = "Certificate not found" });

                // Parse credential from JSON
                var credential = JsonConvert.DeserializeObject<BlockcertsCredential>(certificate.CredentialJson);
                if (credential == null)
                    return BadRequest(new { message = "Invalid credential data" });

                // Validate credential structure
                var validationResult = _blockcertsService.ValidateBlockcertsCredential(credential);

                // Verify blockchain anchor if present
                if (credential.Proof?.Anchors != null && credential.Proof.Anchors.Length > 0)
                {
                    foreach (var anchor in credential.Proof.Anchors)
                    {
                        var isBlockchainValid = await _blockcertsService.VerifyBlockchainAnchor(anchor);
                        validationResult.IsBlockchainVerified = isBlockchainValid;
                        validationResult.TransactionId = anchor.Chain.TransactionId;
                        validationResult.BlockchainNetwork = anchor.Chain.Name;
                        
                        if (!isBlockchainValid)
                        {
                            validationResult.Errors.Add($"Blockchain verification failed for transaction {anchor.Chain.TransactionId}");
                        }
                    }

                    // Verify Merkle proof if present
                    if (credential.Proof.MerkleProof != null)
                    {
                        var isMerkleValid = _blockcertsService.VerifyMerkleProof(credential.Proof.MerkleProof);
                        if (!isMerkleValid)
                        {
                            validationResult.Errors.Add("Merkle proof verification failed");
                        }
                    }
                }

                return Ok(new
                {
                    isValid = validationResult.IsValid && validationResult.Errors.Count == 0,
                    isBlockchainVerified = validationResult.IsBlockchainVerified,
                    transactionId = validationResult.TransactionId,
                    blockchainNetwork = validationResult.BlockchainNetwork,
                    errors = validationResult.Errors,
                    warnings = validationResult.Warnings,
                    isRevoked = certificate.IsRevoked,
                    verificationDate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to verify certificate", error = ex.Message });
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public IActionResult ValidateCredential([FromBody] BlockcertsCredential credential)
        {
            try
            {
                var validationResult = _blockcertsService.ValidateBlockcertsCredential(credential);

                return Ok(new
                {
                    isValid = validationResult.IsValid,
                    errors = validationResult.Errors,
                    warnings = validationResult.Warnings
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to validate credential", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCertificate(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var certificate = await _context.BlockcertsCertificates
                    .Where(c => c.Id == id && c.UserId == userId)
                    .FirstOrDefaultAsync();

                if (certificate == null)
                    return NotFound(new { message = "Certificate not found" });

                _context.BlockcertsCertificates.Remove(certificate);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Certificate deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete certificate", error = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        private string GenerateSimulatedPublicKey()
        {
            // Generate a simulated PEM public key for demo purposes
            return @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1234567890abcdefghij
klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghij
klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghij
klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghij
klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghij
klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZwIDAQAB
-----END PUBLIC KEY-----";
        }
    }

    public class CreateCertificateRequest
    {
        public string RecipientId { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string BadgeName { get; set; } = string.Empty;
        public string BadgeDescription { get; set; } = string.Empty;
        public string IssuerName { get; set; } = string.Empty;
        public string? IssuerUrl { get; set; }
        public string? IssuerEmail { get; set; }
        public string? ImageUrl { get; set; }
        public string? PublicKeyPem { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class AnchorCertificateRequest
    {
        public string Network { get; set; } = "bitcoin";
        public bool Testnet { get; set; } = true;
    }
}