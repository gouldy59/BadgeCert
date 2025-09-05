using Newtonsoft.Json;

namespace BadgeManagement.Models
{
    public class BlockcertsCredential
    {
        [JsonProperty("@context")]
        public string[] Context { get; set; } = Array.Empty<string>();
        
        [JsonProperty("type")]
        public string[] Type { get; set; } = Array.Empty<string>();
        
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("issuer")]
        public BlockcertsIssuer Issuer { get; set; } = new();
        
        [JsonProperty("issuanceDate")]
        public string IssuanceDate { get; set; } = string.Empty;
        
        [JsonProperty("expirationDate")]
        public string? ExpirationDate { get; set; }
        
        [JsonProperty("credentialSubject")]
        public BlockcertsCredentialSubject CredentialSubject { get; set; } = new();
        
        [JsonProperty("proof")]
        public BlockcertsProof? Proof { get; set; }
        
        [JsonProperty("verification")]
        public BlockcertsVerification? Verification { get; set; }
        
        [JsonProperty("badge")]
        public BlockcertsBadge? Badge { get; set; }
        
        [JsonProperty("nonce")]
        public string? Nonce { get; set; }
    }

    public class BlockcertsIssuer
    {
        [JsonProperty("@context")]
        public string[] Context { get; set; } = Array.Empty<string>();
        
        [JsonProperty("type")]
        public string[] Type { get; set; } = Array.Empty<string>();
        
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("url")]
        public string? Url { get; set; }
        
        [JsonProperty("email")]
        public string? Email { get; set; }
        
        [JsonProperty("image")]
        public string? Image { get; set; }
        
        [JsonProperty("publicKey")]
        public BlockcertsPublicKey[] PublicKey { get; set; } = Array.Empty<BlockcertsPublicKey>();
        
        [JsonProperty("revocationList")]
        public string? RevocationList { get; set; }
        
        [JsonProperty("analyticsURL")]
        public string? AnalyticsURL { get; set; }
    }

    public class BlockcertsPublicKey
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("created")]
        public string Created { get; set; } = string.Empty;
        
        [JsonProperty("expires")]
        public string? Expires { get; set; }
        
        [JsonProperty("publicKeyPem")]
        public string? PublicKeyPem { get; set; }
        
        [JsonProperty("publicKeyMultibase")]
        public string? PublicKeyMultibase { get; set; }
    }

    public class BlockcertsCredentialSubject
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("publicKey")]
        public string? PublicKey { get; set; }
        
        [JsonProperty("hashed")]
        public bool? Hashed { get; set; }
    }

    public class BlockcertsVerification
    {
        [JsonProperty("type")]
        public string[] Type { get; set; } = Array.Empty<string>();
        
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; } = string.Empty;
    }

    public class BlockcertsBadge
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonProperty("image")]
        public string? Image { get; set; }
        
        [JsonProperty("criteria")]
        public BlockcertsCriteria? Criteria { get; set; }
        
        [JsonProperty("issuer")]
        public BlockcertsIssuer Issuer { get; set; } = new();
    }

    public class BlockcertsCriteria
    {
        [JsonProperty("narrative")]
        public string Narrative { get; set; } = string.Empty;
    }
}