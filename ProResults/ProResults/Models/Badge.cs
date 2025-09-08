using System.ComponentModel.DataAnnotations;

namespace ProResults.Models
{
    public class Badge
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Issuer { get; set; } = string.Empty;
        
        public DateTime IssuedDate { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Required]
        public string CredentialJson { get; set; } = string.Empty;
        
        public bool IsVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
