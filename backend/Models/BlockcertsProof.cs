using Newtonsoft.Json;

namespace BadgeManagement.Models
{
    public class BlockcertsProof
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("created")]
        public string Created { get; set; } = string.Empty;
        
        [JsonProperty("proofPurpose")]
        public string ProofPurpose { get; set; } = string.Empty;
        
        [JsonProperty("verificationMethod")]
        public string VerificationMethod { get; set; } = string.Empty;
        
        [JsonProperty("merkleRoot")]
        public string? MerkleRoot { get; set; }
        
        [JsonProperty("targetHash")]
        public string? TargetHash { get; set; }
        
        [JsonProperty("anchors")]
        public BlockchainAnchor[] Anchors { get; set; } = Array.Empty<BlockchainAnchor>();
        
        [JsonProperty("merkleProof")]
        public MerkleProof2019? MerkleProof { get; set; }
    }

    public class BlockchainAnchor
    {
        [JsonProperty("sourceId")]
        public string SourceId { get; set; } = string.Empty;
        
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonProperty("chain")]
        public BlockchainInfo Chain { get; set; } = new();
    }

    public class BlockchainInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("test")]
        public bool Test { get; set; } = false;
        
        [JsonProperty("signatureValue")]
        public string? SignatureValue { get; set; }
        
        [JsonProperty("transactionId")]
        public string? TransactionId { get; set; }
        
        [JsonProperty("rawTransactionId")]
        public string? RawTransactionId { get; set; }
    }

    public class MerkleProof2019
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "MerkleProof2019";
        
        [JsonProperty("merkleRoot")]
        public string MerkleRoot { get; set; } = string.Empty;
        
        [JsonProperty("targetHash")]
        public string TargetHash { get; set; } = string.Empty;
        
        [JsonProperty("proof")]
        public MerkleProofData[] Proof { get; set; } = Array.Empty<MerkleProofData>();
        
        [JsonProperty("anchors")]
        public BlockchainAnchor[] Anchors { get; set; } = Array.Empty<BlockchainAnchor>();
    }

    public class MerkleProofData
    {
        [JsonProperty("left")]
        public string? Left { get; set; }
        
        [JsonProperty("right")]
        public string? Right { get; set; }
    }

    public enum BlockchainNetwork
    {
        Bitcoin,
        BitcoinTestnet,
        Ethereum,
        EthereumTestnet,
        Mockchain
    }

    public static class BlockchainNetworkConstants
    {
        public const string Bitcoin = "bitcoinMainnet";
        public const string BitcoinTestnet = "bitcoinTestnet";
        public const string Ethereum = "ethereumMainnet";
        public const string EthereumTestnet = "ethereumTestnet";
        public const string Mockchain = "mockchain";
    }
}