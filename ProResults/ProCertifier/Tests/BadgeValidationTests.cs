using System.Threading.Tasks;
using NJsonSchema;
using Xunit;

namespace ProCertifier.Tests
{
    public class BadgeValidationTests
    {
        private const string SchemaUrl =
            "https://purl.imsglobal.org/spec/ob/v3p0/schema/json/ob_v3p0_achievementcredential_schema.json";

        [Fact]
        public async Task InvalidBadge_ShouldFailValidation()
        {
            // ❌ Invalid badge: missing "issuer" and wrong type for credentialSubject
            string invalidBadgeJson = @"{
            ""@context"": [
                ""https://www.w3.org/2018/credentials/v2"",
                ""https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json""
            ],
            ""type"": [""VerifiableCredential"", ""OpenBadgeCredential""],
            ""issuanceDate"": ""2025-09-08T12:00:00Z"",
            ""credentialSubject"": ""this-should-be-an-object-not-a-string""
        }";


            var schema = await JsonSchema.FromUrlAsync(SchemaUrl);
            var errors = schema.Validate(invalidBadgeJson);

            Assert.NotEmpty(errors); // ✅ should return errors
        }

        [Fact]
        public async Task ValidBadge_ShouldPassValidation()
        {
            // ✅ Proper badge JSON
            string validBadgeJson = @"{
            ""@context"": [
                ""https://www.w3.org/2018/credentials/v2"",
                ""https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json""
            ],
            ""type"": [""VerifiableCredential"", ""OpenBadgeCredential""],
            ""issuer"": ""did:web:example.org"",
            ""issuanceDate"": ""2025-09-08T12:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""mailto:student@example.com"",
                ""achievement"": ""https://example.org/achievements/exam-123""
            }
        }";

 
            var schema = await JsonSchema.FromUrlAsync(SchemaUrl);
            var errors = schema.Validate(validBadgeJson);

            Assert.Empty(errors); // ✅ no errors
        }
    }
}
