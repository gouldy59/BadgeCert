using BadgeManagement.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BadgeManagement.Services
{
    public class BlockcertsService
    {
        private readonly string[] RequiredContexts = {
            "https://www.w3.org/2018/credentials/v1",
            "https://www.blockcerts.org/schema/3.0/context.json"
        };

        private readonly string[] RequiredTypes = {
            "VerifiableCredential",
            "BlockcertsCredential"
        };

        public BlockcertsValidationResult ValidateBlockcertsCredential(BlockcertsCredential credential)
        {
            var result = new BlockcertsValidationResult();
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // Validate @context
                if (credential.Context == null || credential.Context.Length == 0)
                {
                    errors.Add("Missing @context array");
                }
                else
                {
                    foreach (var requiredContext in RequiredContexts)
                    {
                        if (!credential.Context.Contains(requiredContext))
                        {
                            errors.Add($"Missing required context: {requiredContext}");
                        }
                    }
                }

                // Validate type
                if (credential.Type == null || credential.Type.Length == 0)
                {
                    errors.Add("Missing type array");
                }
                else
                {
                    foreach (var requiredType in RequiredTypes)
                    {
                        if (!credential.Type.Contains(requiredType))
                        {
                            errors.Add($"Missing required type: {requiredType}");
                        }
                    }
                }

                // Validate required fields
                if (string.IsNullOrEmpty(credential.Id))
                    errors.Add("Missing required field: id");
                if (string.IsNullOrEmpty(credential.IssuanceDate))
                    errors.Add("Missing required field: issuanceDate");
                if (credential.Issuer == null)
                    errors.Add("Missing required field: issuer");
                if (credential.CredentialSubject == null)
                    errors.Add("Missing required field: credentialSubject");

                // Validate issuer
                if (credential.Issuer != null)
                {
                    ValidateIssuer(credential.Issuer, errors);
                }

                // Validate credential subject
                if (credential.CredentialSubject != null)
                {
                    ValidateCredentialSubject(credential.CredentialSubject, errors);
                }

                // Validate proof if present
                if (credential.Proof != null)
                {
                    ValidateProof(credential.Proof, errors, warnings);
                }

                result.Errors = errors;
                result.Warnings = warnings;
                result.IsValid = errors.Count == 0;
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
                result.Errors = errors;
                result.IsValid = false;
            }

            return result;
        }

        public BlockcertsCredential CreateCredential(string recipientId, string recipientName, 
            string badgeName, string badgeDescription, BlockcertsIssuer issuer, 
            DateTime? expirationDate = null)
        {
            var credentialId = Guid.NewGuid().ToString();
            var badgeId = Guid.NewGuid().ToString();

            var credential = new BlockcertsCredential
            {
                Context = new[]
                {
                    "https://www.w3.org/2018/credentials/v1",
                    "https://www.blockcerts.org/schema/3.0/context.json",
                    "https://w3id.org/openbadges/v2"
                },
                Type = new[] { "VerifiableCredential", "BlockcertsCredential" },
                Id = $"urn:uuid:{credentialId}",
                IssuanceDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ExpirationDate = expirationDate?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Issuer = issuer,
                CredentialSubject = new BlockcertsCredentialSubject
                {
                    Id = recipientId,
                    Type = "Recipient",
                    Name = recipientName
                },
                Badge = new BlockcertsBadge
                {
                    Type = "BadgeClass",
                    Id = $"urn:uuid:{badgeId}",
                    Name = badgeName,
                    Description = badgeDescription,
                    Issuer = issuer
                },
                Nonce = GenerateNonce()
            };

            return credential;
        }

        public string CanonicalizeCredential(BlockcertsCredential credential)
        {
            // Create a copy without proof for canonicalization
            var credentialForHashing = new BlockcertsCredential
            {
                Context = credential.Context,
                Type = credential.Type,
                Id = credential.Id,
                Issuer = credential.Issuer,
                IssuanceDate = credential.IssuanceDate,
                ExpirationDate = credential.ExpirationDate,
                CredentialSubject = credential.CredentialSubject,
                Badge = credential.Badge,
                Verification = credential.Verification,
                Nonce = credential.Nonce
                // Intentionally exclude Proof
            };

            // Serialize to JSON with consistent formatting
            var json = JsonConvert.SerializeObject(credentialForHashing, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            });

            // Normalize the JSON-LD (simplified approach)
            return NormalizeJsonLd(json);
        }

        public string HashCredential(BlockcertsCredential credential)
        {
            var canonicalJson = CanonicalizeCredential(credential);
            var bytes = Encoding.UTF8.GetBytes(canonicalJson);
            
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToLower();
            }
        }

        public BlockcertsProof CreateMerkleProof(string targetHash, string merkleRoot, 
            MerkleProofData[] proofPath, BlockchainAnchor[] anchors)
        {
            return new BlockcertsProof
            {
                Type = "MerkleProof2019",
                Created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ProofPurpose = "assertionMethod",
                VerificationMethod = anchors.FirstOrDefault()?.SourceId ?? string.Empty,
                MerkleRoot = merkleRoot,
                TargetHash = targetHash,
                Anchors = anchors,
                MerkleProof = new MerkleProof2019
                {
                    Type = "MerkleProof2019",
                    MerkleRoot = merkleRoot,
                    TargetHash = targetHash,
                    Proof = proofPath,
                    Anchors = anchors
                }
            };
        }

        public async Task<BlockchainAnchor> AnchorToBitcoinAsync(string merkleRoot, bool testnet = true)
        {
            // Simulated Bitcoin anchoring - in production, this would interact with actual Bitcoin network
            var transactionId = GenerateSimulatedTransactionId();
            
            return new BlockchainAnchor
            {
                SourceId = $"btctx:{transactionId}",
                Type = "BTCOpReturn",
                Chain = new BlockchainInfo
                {
                    Name = testnet ? BlockchainNetworkConstants.BitcoinTestnet : BlockchainNetworkConstants.Bitcoin,
                    Test = testnet,
                    TransactionId = transactionId,
                    RawTransactionId = transactionId
                }
            };
        }

        public async Task<BlockchainAnchor> AnchorToEthereumAsync(string merkleRoot, bool testnet = true)
        {
            // Simulated Ethereum anchoring - in production, this would interact with actual Ethereum network
            var transactionId = GenerateSimulatedEthereumTransactionId();
            
            return new BlockchainAnchor
            {
                SourceId = $"ethtx:{transactionId}",
                Type = "ETHData",
                Chain = new BlockchainInfo
                {
                    Name = testnet ? BlockchainNetworkConstants.EthereumTestnet : BlockchainNetworkConstants.Ethereum,
                    Test = testnet,
                    TransactionId = transactionId,
                    RawTransactionId = transactionId
                }
            };
        }

        public async Task<bool> VerifyBlockchainAnchor(BlockchainAnchor anchor)
        {
            try
            {
                switch (anchor.Chain.Name)
                {
                    case BlockchainNetworkConstants.Bitcoin:
                    case BlockchainNetworkConstants.BitcoinTestnet:
                        return await VerifyBitcoinTransaction(anchor.Chain.TransactionId);
                    
                    case BlockchainNetworkConstants.Ethereum:
                    case BlockchainNetworkConstants.EthereumTestnet:
                        return await VerifyEthereumTransaction(anchor.Chain.TransactionId);
                    
                    case BlockchainNetworkConstants.Mockchain:
                        return true; // Always valid for mock/test purposes
                    
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyMerkleProof(MerkleProof2019 merkleProof)
        {
            try
            {
                var targetHash = merkleProof.TargetHash;
                var currentHash = targetHash;

                foreach (var proof in merkleProof.Proof)
                {
                    if (!string.IsNullOrEmpty(proof.Left))
                    {
                        currentHash = ComputeHash(proof.Left + currentHash);
                    }
                    else if (!string.IsNullOrEmpty(proof.Right))
                    {
                        currentHash = ComputeHash(currentHash + proof.Right);
                    }
                }

                return currentHash.Equals(merkleProof.MerkleRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void ValidateIssuer(BlockcertsIssuer issuer, List<string> errors)
        {
            if (string.IsNullOrEmpty(issuer.Id))
                errors.Add("Issuer must have an id");
            if (string.IsNullOrEmpty(issuer.Name))
                errors.Add("Issuer must have a name");
            if (issuer.Type == null || issuer.Type.Length == 0)
                errors.Add("Issuer must have a type");
            if (issuer.PublicKey == null || issuer.PublicKey.Length == 0)
                errors.Add("Issuer must have at least one public key");
        }

        private void ValidateCredentialSubject(BlockcertsCredentialSubject subject, List<string> errors)
        {
            if (string.IsNullOrEmpty(subject.Id))
                errors.Add("CredentialSubject must have an id");
            if (string.IsNullOrEmpty(subject.Type))
                errors.Add("CredentialSubject must have a type");
        }

        private void ValidateProof(BlockcertsProof proof, List<string> errors, List<string> warnings)
        {
            if (string.IsNullOrEmpty(proof.Type))
                errors.Add("Proof must have a type");
            if (string.IsNullOrEmpty(proof.Created))
                errors.Add("Proof must have a created timestamp");
            if (string.IsNullOrEmpty(proof.ProofPurpose))
                errors.Add("Proof must have a proofPurpose");

            if (proof.Anchors == null || proof.Anchors.Length == 0)
                warnings.Add("Proof has no blockchain anchors");
            
            if (string.IsNullOrEmpty(proof.MerkleRoot))
                warnings.Add("Proof has no Merkle root");
        }

        private string NormalizeJsonLd(string json)
        {
            // Simplified JSON-LD normalization
            // In production, use a proper JSON-LD library like JSON-LD.NET
            var jObject = JObject.Parse(json);
            SortJsonProperties(jObject);
            return jObject.ToString(Formatting.None);
        }

        private void SortJsonProperties(JToken token)
        {
            if (token is JObject obj)
            {
                var properties = obj.Properties().ToList();
                foreach (var prop in properties)
                {
                    prop.Remove();
                }

                foreach (var prop in properties.OrderBy(p => p.Name))
                {
                    obj.Add(prop);
                    SortJsonProperties(prop.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array)
                {
                    SortJsonProperties(item);
                }
            }
        }

        private string GenerateNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GenerateSimulatedTransactionId()
        {
            // Generate a realistic-looking Bitcoin transaction ID
            var random = new Random();
            var bytes = new byte[32];
            random.NextBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }

        private string GenerateSimulatedEthereumTransactionId()
        {
            // Generate a realistic-looking Ethereum transaction ID
            var random = new Random();
            var bytes = new byte[32];
            random.NextBytes(bytes);
            return "0x" + Convert.ToHexString(bytes).ToLower();
        }

        private async Task<bool> VerifyBitcoinTransaction(string? transactionId)
        {
            // Simulated Bitcoin verification - in production, query actual Bitcoin network
            await Task.Delay(100); // Simulate network delay
            return !string.IsNullOrEmpty(transactionId) && transactionId.Length == 64;
        }

        private async Task<bool> VerifyEthereumTransaction(string? transactionId)
        {
            // Simulated Ethereum verification - in production, query actual Ethereum network
            await Task.Delay(100); // Simulate network delay
            return !string.IsNullOrEmpty(transactionId) && transactionId.StartsWith("0x") && transactionId.Length == 66;
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToLower();
            }
        }
    }
}