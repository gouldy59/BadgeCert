using System.ComponentModel.DataAnnotations;

namespace BadgeManagement.Models
{
    public class BlockcertsCertificate
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string IssuerName { get; set; } = string.Empty;
        
        [Required]
        public string RecipientId { get; set; } = string.Empty;
        
        public DateTime IssuanceDate { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Required]
        public string CredentialJson { get; set; } = string.Empty;
        
        public bool IsAnchored { get; set; } = false;
        
        public string? TransactionId { get; set; }
        
        public string? BlockchainNetwork { get; set; }
        
        public string? MerkleRoot { get; set; }
        
        public bool IsRevoked { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public class BlockcertsValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool IsBlockchainVerified { get; set; } = false;
        public string? TransactionId { get; set; }
        public string? BlockchainNetwork { get; set; }
    }
}