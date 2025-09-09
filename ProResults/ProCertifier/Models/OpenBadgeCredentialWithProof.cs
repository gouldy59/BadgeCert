using System.Text.Json.Serialization;

namespace ProCertifier.Models
{
    public class OpenBadgeCredentialWithProof
    {
        [JsonPropertyName("@context")]
        public List<string> Context { get; set; }

        [JsonPropertyName("type")]
        public List<string> Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("issuer")]
        public Issuer Issuer { get; set; }

        [JsonPropertyName("issuanceDate")]
        public string IssuanceDate { get; set; }

        [JsonPropertyName("validFrom")]
        public string ValidFrom { get; set; }

        [JsonPropertyName("credentialSubject")]
        public CredentialSubject CredentialSubject { get; set; }
        [JsonPropertyName("proof")]
        public Proof Proof { get; set; }
    }

    public class Proof
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("created")]
        public string Created { get; set; }
        [JsonPropertyName("verificationMethod")]
        public string VerificationMethod { get; set; }
        [JsonPropertyName("proofPurpose")]
        public string ProofPurpose { get; set; }
        [JsonPropertyName("jws")]
        public string Jws { get; set; }
    }

    public class OpenBadgeCredential
    {
        [JsonPropertyName("@context")]
        public List<string> Context { get; set; }

        [JsonPropertyName("type")]
        public List<string> Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("issuer")]
        public Issuer Issuer { get; set; }

        [JsonPropertyName("issuanceDate")]
        public string IssuanceDate { get; set; }

        [JsonPropertyName("validFrom")]
        public string ValidFrom { get; set; }

        [JsonPropertyName("credentialSubject")]
        public CredentialSubject CredentialSubject { get; set; }
    }

    public class Issuer
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("subIssuer")]
        public Issuer SubIssuer { get; set; }
    }

    public class CredentialSubject
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public List<string> Type { get; set; }

        [JsonPropertyName("achievement")]
        public Achievement Achievement { get; set; }
    }

    public class Achievement
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public List<string> Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("criteria")]
        public Criteria Criteria { get; set; }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }
    }

    public class Criteria
    {
        [JsonPropertyName("narrative")]
        public string Narrative { get; set; }
    }

    public class Image
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
