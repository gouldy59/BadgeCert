using BadgeManagement.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Net.Http;

namespace BadgeManagement.Services
{
    public class PdfService
    {
        public async Task<byte[]> GenerateBadgePDF(Badge badge)
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

                // Badge Image (if available)
                if (!string.IsNullOrEmpty(badge.ImageUrl))
                {
                    try
                    {
                        // Download and embed the actual image
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.Timeout = TimeSpan.FromSeconds(10);
                            var imageBytes = await httpClient.GetByteArrayAsync(badge.ImageUrl);
                            var badgeImage = Image.GetInstance(imageBytes);
                            
                            // Resize image to fit nicely in PDF
                            badgeImage.ScaleToFit(100f, 100f);
                            badgeImage.Alignment = Element.ALIGN_CENTER;
                            badgeImage.SpacingAfter = 15f;
                            
                            document.Add(badgeImage);
                            
                            var imageCaption = new Paragraph("Official Badge Image", FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(100, 100, 100)))
                            {
                                Alignment = Element.ALIGN_CENTER,
                                SpacingAfter = 20
                            };
                            document.Add(imageCaption);
                        }
                    }
                    catch (Exception)
                    {
                        // Fallback if image download fails
                        var imageHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(0, 123, 255));
                        var imageHeader = new Paragraph("🖼️ Badge Image", imageHeaderFont)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 5
                        };
                        document.Add(imageHeader);

                        var imageNote = new Paragraph($"View the badge image at: {badge.ImageUrl}", FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(100, 100, 100)))
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 20
                        };
                        document.Add(imageNote);
                    }
                }
                else
                {
                    var noImageNote = new Paragraph("🏆 Digital Badge (No image provided)", FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(100, 100, 100)))
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 20
                    };
                    document.Add(noImageNote);
                }

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

        public string GenerateBadgeSVG(Badge badge)
        {
            // Create a self-contained SVG that doesn't rely on external images
            var imageElement = string.IsNullOrEmpty(badge.ImageUrl) 
                ? @"<circle cx=""200"" cy=""110"" r=""50"" fill=""#007bff""/>
                   <circle cx=""200"" cy=""110"" r=""20"" fill=""white""/>
                   <text x=""200"" y=""120"" text-anchor=""middle"" font-family=""Arial"" font-size=""24"" fill=""white"">🏆</text>"
                : $@"<circle cx=""200"" cy=""110"" r=""50"" fill=""#007bff""/>
                    <circle cx=""200"" cy=""110"" r=""45"" fill=""white""/>
                    <text x=""200"" y=""90"" text-anchor=""middle"" font-family=""Arial"" font-size=""8"" fill=""#333"">Badge Image:</text>
                    <text x=""200"" y=""120"" text-anchor=""middle"" font-family=""Arial"" font-size=""24"" fill=""#007bff"">🖼️</text>
                    <text x=""200"" y=""135"" text-anchor=""middle"" font-family=""Arial"" font-size=""7"" fill=""#666"">View: {badge.ImageUrl.Substring(badge.ImageUrl.LastIndexOf('/') + 1)}</text>";

            return $@"<svg width=""400"" height=""400"" xmlns=""http://www.w3.org/2000/svg"">
  <rect x=""10"" y=""10"" width=""380"" height=""380"" fill=""white"" stroke=""#007bff"" stroke-width=""4""/>
  {imageElement}
  <text x=""200"" y=""200"" text-anchor=""middle"" font-family=""Arial"" font-size=""18"" font-weight=""bold"" fill=""#333"">{badge.Name}</text>
  <text x=""200"" y=""250"" text-anchor=""middle"" font-family=""Arial"" font-size=""12"" fill=""#666"">{(badge.Description.Length > 50 ? badge.Description.Substring(0, 47) + "..." : badge.Description)}</text>
  <text x=""200"" y=""320"" text-anchor=""middle"" font-family=""Arial"" font-size=""10"" fill=""#999"">Issued by: {badge.Issuer}</text>
  <text x=""200"" y=""345"" text-anchor=""middle"" font-family=""Arial"" font-size=""10"" fill=""#999"">Date: {badge.IssuedDate:MM/dd/yyyy}</text>
  {(badge.IsVerified ? "<rect x=\"130\" y=\"360\" width=\"140\" height=\"20\" fill=\"#28a745\" rx=\"3\"/><text x=\"200\" y=\"375\" text-anchor=\"middle\" font-family=\"Arial\" font-size=\"8\" fill=\"white\">✓ OpenBadges v3.0 Verified</text>" : "")}
</svg>";
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
        {(string.IsNullOrEmpty(badge.ImageUrl) ? 
            "<div class=\"badge-icon\">🏆</div>" : 
            $"<img src=\"{badge.ImageUrl}\" alt=\"{badge.Name}\" class=\"badge-icon\" style=\"width: 80px; height: 80px; border-radius: 50%; object-fit: cover;\" />"
        )}
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
