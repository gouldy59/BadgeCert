using Newtonsoft.Json;

namespace ProResults.Models
{
    public class OpenBadgeCredential
    {
        [JsonProperty("@context")]
        public string[] Context { get; set; } = Array.Empty<string>();
        
        [JsonProperty("type")]
        public string[] Type { get; set; } = Array.Empty<string>();
        
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("issuer")]
        public Issuer Issuer { get; set; } = new();
        
        [JsonProperty("validFrom")]
        public string ValidFrom { get; set; } = string.Empty;
        
        [JsonProperty("validUntil")]
        public string? ValidUntil { get; set; }
        
        [JsonProperty("credentialSubject")]
        public CredentialSubject CredentialSubject { get; set; } = new();
        
        [JsonProperty("proof")]
        public Proof? Proof { get; set; }
    }
    
    public class Issuer
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
    
    public class CredentialSubject
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("achievement")]
        public Achievement Achievement { get; set; } = new();
    }
    
    public class Achievement
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonProperty("criteria")]
        public Criteria Criteria { get; set; } = new();
        
        [JsonProperty("image")]
        public BadgeImage? Image { get; set; }
    }
    
    public class Criteria
    {
        [JsonProperty("narrative")]
        public string Narrative { get; set; } = string.Empty;
    }
    
    public class BadgeImage
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
    }
    
    public class Proof
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("created")]
        public string Created { get; set; } = string.Empty;
        
        [JsonProperty("verificationMethod")]
        public string VerificationMethod { get; set; } = string.Empty;
        
        [JsonProperty("proofPurpose")]
        public string ProofPurpose { get; set; } = string.Empty;
        
        [JsonProperty("proofValue")]
        public string ProofValue { get; set; } = string.Empty;
    }
}
