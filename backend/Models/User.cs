using System.ComponentModel.DataAnnotations;

namespace BadgeManagement.Models
{
    public class User
    {
        public Guid Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public List<Badge> Badges { get; set; } = new();
        
        public List<Result> Results { get; set; } = new();
        
        public List<BlockcertsCertificate> BlockcertsCertificates { get; set; } = new();
    }
}
