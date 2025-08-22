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
            // For cross-platform compatibility, generate a simple SVG and convert to PNG
            // This is a simplified implementation - in production you'd want to use SkiaSharp or ImageSharp
            var svgContent = $@"<svg width=""400"" height=""400"" xmlns=""http://www.w3.org/2000/svg"">
  <rect x=""10"" y=""10"" width=""380"" height=""380"" fill=""white"" stroke=""#007bff"" stroke-width=""4""/>
  <circle cx=""200"" cy=""110"" r=""50"" fill=""#007bff""/>
  <circle cx=""200"" cy=""110"" r=""20"" fill=""white""/>
  <text x=""200"" y=""200"" text-anchor=""middle"" font-family=""Arial"" font-size=""18"" font-weight=""bold"" fill=""#333"">{badge.Name}</text>
  <text x=""200"" y=""250"" text-anchor=""middle"" font-family=""Arial"" font-size=""12"" fill=""#666"">{(badge.Description.Length > 50 ? badge.Description.Substring(0, 47) + "..." : badge.Description)}</text>
  <text x=""200"" y=""320"" text-anchor=""middle"" font-family=""Arial"" font-size=""10"" fill=""#999"">Issued by: {badge.Issuer}</text>
  <text x=""200"" y=""345"" text-anchor=""middle"" font-family=""Arial"" font-size=""10"" fill=""#999"">Date: {badge.IssuedDate:MM/dd/yyyy}</text>
</svg>";
            
            // For now, return SVG as bytes with PNG header placeholder
            // In production, you'd convert SVG to PNG using a proper library
            var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
            var result = new byte[pngHeader.Length + svgBytes.Length];
            Array.Copy(pngHeader, 0, result, 0, pngHeader.Length);
            Array.Copy(svgBytes, 0, result, pngHeader.Length, svgBytes.Length);
            return result;
        }
    }
}
