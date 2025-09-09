using System.ComponentModel.DataAnnotations;

namespace ProResults.Models
{
    public class ScoreReport
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;

        public byte[]? htmlBody { get; set; }

        public string Status { get; set; } = "pending"; // completed, in-progress, pending
        
        public DateTime AchievedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key
        public Guid UserId { get; set; }
        public string  email { get; set; } = null!;

        // Store PDF file as byte array
        public byte[]? PdfFile { get; set; }

        // Store image as byte array (optional, for a single image)
        public byte[]? ImageFile { get; set; }

        public string LogoUrl { get; set; }

    }
}
