using System.ComponentModel.DataAnnotations;

namespace ProResults.Models
{
    public class Result
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = "pending"; // completed, in-progress, pending
        
        public DateTime AchievedDate { get; set; }
        
        public double? Score { get; set; }
        
        public Guid? BadgeId { get; set; }
        public Badge? Badge { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
