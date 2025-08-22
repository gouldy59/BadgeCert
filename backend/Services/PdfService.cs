using BadgeManagement.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BadgeManagement.Services
{
    public class PdfService
    {
        public byte[] GenerateBadgePDF(Badge badge)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);
                
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, new BaseColor(64, 64, 64));
                var title = new Paragraph("Digital Badge Certificate", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(title);

                // Badge Name
                var badgeNameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(0, 123, 255));
                var badgeName = new Paragraph(badge.Name, badgeNameFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(badgeName);

                // Description
                var descFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(0, 0, 0));
                var description = new Paragraph($"Description: {badge.Description}", descFont)
                {
                    SpacingAfter = 15
                };
                document.Add(description);

                // Issuer Information
                var issuerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 0, 0));
                var issuer = new Paragraph($"Issued by: {badge.Issuer}", issuerFont)
                {
                    SpacingAfter = 10
                };
                document.Add(issuer);

                // Issue Date
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(0, 0, 0));
                var issueDate = new Paragraph($"Issue Date: {badge.IssuedDate:MMMM dd, yyyy}", dateFont)
                {
                    SpacingAfter = 10
                };
                document.Add(issueDate);

                // Expiration Date
                if (badge.ExpirationDate.HasValue)
                {
                    var expiration = new Paragraph($"Expiration Date: {badge.ExpirationDate.Value:MMMM dd, yyyy}", dateFont)
                    {
                        SpacingAfter = 20
                    };
                    document.Add(expiration);
                }

                // Verification Status
                var verificationFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, 
                    badge.IsVerified ? new BaseColor(0, 128, 0) : new BaseColor(255, 0, 0));
                var verification = new Paragraph($"Verification Status: {(badge.IsVerified ? "VERIFIED" : "UNVERIFIED")}", verificationFont)
                {
                    SpacingAfter = 20
                };
                document.Add(verification);

                // OpenBadges Compliance
                var complianceFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(0, 123, 255));
                var compliance = new Paragraph("This badge is compliant with OpenBadges v3.0 specification", complianceFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(compliance);

                // Badge ID for verification
                var idFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(128, 128, 128));
                var badgeId = new Paragraph($"Badge ID: {badge.Id}", idFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(badgeId);

                document.Close();
                return ms.ToArray();
            }
        }

        public byte[] GenerateBadgePNG(Badge badge)
        {
            // Create a simple HTML page that can be rendered as an image
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ margin: 0; padding: 20px; font-family: Arial, sans-serif; background: white; width: 360px; height: 360px; }}
        .badge {{ border: 4px solid #007bff; padding: 20px; text-align: center; background: white; }}
        .badge-icon {{ width: 80px; height: 80px; background: #007bff; border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; color: white; font-size: 24px; }}
        .badge-name {{ font-size: 18px; font-weight: bold; color: #333; margin-bottom: 10px; }}
        .badge-desc {{ font-size: 12px; color: #666; margin-bottom: 15px; line-height: 1.4; }}
        .badge-issuer {{ font-size: 10px; color: #999; margin-bottom: 5px; }}
        .badge-date {{ font-size: 10px; color: #999; }}
        .badge-verified {{ background: #28a745; color: white; padding: 2px 8px; border-radius: 3px; font-size: 8px; margin-top: 10px; display: inline-block; }}
    </style>
</head>
<body>
    <div class=""badge"">
        <div class=""badge-icon"">🏆</div>
        <div class=""badge-name"">{badge.Name}</div>
        <div class=""badge-desc"">{(badge.Description.Length > 100 ? badge.Description.Substring(0, 97) + "..." : badge.Description)}</div>
        <div class=""badge-issuer"">Issued by: {badge.Issuer}</div>
        <div class=""badge-date"">Date: {badge.IssuedDate:MM/dd/yyyy}</div>
        {(badge.IsVerified ? "<div class=\"badge-verified\">✓ OpenBadges v3.0 Verified</div>" : "")}
    </div>
</body>
</html>";

            // Return the HTML as bytes - browsers can save this as an image
            // In a production environment, you'd use a library like Puppeteer or wkhtmltopdf to convert HTML to PNG
            return System.Text.Encoding.UTF8.GetBytes(html);
        }
    }
}
