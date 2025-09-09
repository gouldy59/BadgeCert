using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Chaos.NaCl;
using SimpleBase;
using Stratumn.CanonicalJson;
using ProCertifier.Models;
using Microsoft.Extensions.FileProviders;
using System;

namespace ProCertifier.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CredentialsController : ControllerBase
    {
        private static readonly Dictionary<string, string> issuedCredentials = new();
        private static readonly List<KeyValuePair<string, string>> CandidateCredentials = new List <KeyValuePair<string, string>>();
        private static readonly string did = "did:web:localhost";
        private static readonly byte[] seed;
        private static readonly byte[] publicKey;
        private static readonly byte[] privateKey;
        private static readonly string publicKeyBase58;
        // Example sub-issuer registry
        private static readonly Dictionary<string, dynamic> Clients = new Dictionary<string, dynamic>()
        {
            ["client-shane"] = new { Name = "shane Organization A", Description = "Awarded for passing shane Organization Exam A", Url = "https://client-shane.example.com" },
            ["client-kerrie"] = new { Name = "kerrie Organization B", Description = "Awarded for passing kerrie Organization Exam B", Url = "https://client-kerrie.example.com" }
        };
        static CredentialsController()
        {
            // ===== Generate DID and Ed25519 Key Pair =====
            seed = new byte[32];
            Random.Shared.NextBytes(seed);
            publicKey = Ed25519.PublicKeyFromSeed(seed);
            privateKey = Ed25519.ExpandedPrivateKeyFromSeed(seed);
            publicKeyBase58 = Base58.Bitcoin.Encode(publicKey);
        }

        [HttpGet("/.well-known/did.json")]
        public IActionResult GetDidDoc()
        {
            var didDoc = new
            {
                @context = "https://www.w3.org/ns/did/v1",
                id = did,
                verificationMethod = new[]
                {
                    new {
                        id = did + "#key-1",
                        type = "Ed25519VerificationKey2020",
                        controller = did,
                        publicKeyBase58
                    }
                },
                assertionMethod = new[] { did + "#key-1" }
            };
            return new JsonResult(didDoc, new JsonSerializerOptions { WriteIndented = true });
        }

        [HttpPost("issue-badge/{clientId}")]
        public async Task<IActionResult> IssueBadge(string clientId)
        {
            if (!Clients.ContainsKey(clientId))
                return BadRequest("ClienIdNotFound");
            var client = Clients[clientId];

            using var doc = await JsonDocument.ParseAsync(Request.Body);
            if (!doc.RootElement.TryGetProperty("studentId", out var studentIdElement))
            {
                return BadRequest("Missing 'studentId' in request body.");
            }

            string studentId = studentIdElement.GetString() ?? throw new Exception("Invalid studentId");

            var achievement = new Achievement()
            {
                Id = $"https://certs.example.com/achievements/{clientId}/exam-123",
                Type = new List<string> { "Achievement" },
                Name = client.Name,
                Description = client.Description, 
                Criteria = new Criteria() { Narrative = "Pass the online exam with 80% or higher." },
                Image = new Image()
                {
                    Id = clientId == "client-shane" ? "https://localhost:7184/badges/mybadge.jpg" : "https://localhost:7184/badges/mybadge2.jpg",
                    Type = "Image"
                },
                Issuer = did
            };

            var issuanceDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var validFrom = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var credentialSubject = new CredentialSubject
            {
                Id = studentId,
                Type = new List<string> { "AchievementSubject" },
                Achievement = achievement
            };

            var credentialId = $"https://localhost:7184/credentials/{Guid.NewGuid()}";

            var unsignedCredential = new OpenBadgeCredential
            {
                Context =
                    [
                    "https://www.w3.org/ns/credentials/v2",
                    "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
                    ],
                Type = [ "VerifiableCredential", "OpenBadgeCredential"],
                Id = credentialId,
                Name = "Sample Badge Credential",
                Issuer = new Issuer()
                {
                    Id = did,
                    Type =  "Profile",
                    Name = "Prometric",
                    SubIssuer = new Issuer()
                    {
                        Id = clientId,
                        Name = client.Name
                    }
                },
                IssuanceDate = issuanceDate,
                ValidFrom = validFrom,
                CredentialSubject = credentialSubject
            };

            // Serialize -> canonicalize for signing
            string unsignedJson = JsonSerializer.Serialize(unsignedCredential);
            string canonicalJson = Canonicalizer.Canonicalize(unsignedJson);
            byte[] credentialBytes = Encoding.UTF8.GetBytes(canonicalJson);

            // Sign credential
            byte[] signature = Ed25519.Sign(credentialBytes, privateKey);
            string signatureBase58 = Base58.Bitcoin.Encode(signature);

            var proof = new Proof()
            {
                Type = "Ed25519Signature2020",
                Created = issuanceDate,
                VerificationMethod = did + "#key-1",
                ProofPurpose = "assertionMethod",
                Jws = signatureBase58
            };

            var signedCredential = new OpenBadgeCredentialWithProof
            {
                Context = unsignedCredential.Context,
                Type = unsignedCredential.Type,
                Id = unsignedCredential.Id,
                Name = unsignedCredential.Name,
                Issuer = unsignedCredential.Issuer,
                IssuanceDate = unsignedCredential.IssuanceDate,
                ValidFrom = unsignedCredential.ValidFrom,
                CredentialSubject = unsignedCredential.CredentialSubject,
                Proof = proof
            };
            issuedCredentials[credentialId] = JsonSerializer.Serialize(signedCredential);
            CandidateCredentials.Add(new KeyValuePair<string, string>(studentId, credentialId));
            return new JsonResult(signedCredential);
        }


        [HttpPost("issue-badge")]
        public async Task<IActionResult> IssueBadge()
        {
            using var doc = await JsonDocument.ParseAsync(Request.Body);
            if (!doc.RootElement.TryGetProperty("studentId", out var studentIdElement))
            {
                return BadRequest("Missing 'studentId' in request body.");
            }

            string studentId = studentIdElement.GetString() ?? throw new Exception("Invalid studentId");

            var achievement = new Achievement()
            {
                Id = "https://certs.example.com/achievements/exam-123",
                Type = new List<string> { "Achievement" },
                Name = "Certified Data Analyst Exam",
                Description = "Awarded for passing the Data Analyst Exam with at least 80%.",
                Criteria = new Criteria() { Narrative = "Pass the online exam with 80% or higher." },
                Image = new Image()
                {
                    Id = "https://localhost:7184/badges/mybadge.jpg",
                    Type = "Image"
                },
                Issuer = did
            };

            var issuanceDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var validFrom = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var credentialSubject = new CredentialSubject
            {
                Id = studentId,
                Type = new List<string> { "AchievementSubject" },
                Achievement = achievement
            };

            var credentialId = $"https://localhost:7184/credentials/{Guid.NewGuid()}";

            var unsignedCredential = new OpenBadgeCredential
            {
                Context =
                    [
                    "https://www.w3.org/ns/credentials/v2",
                    "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
                    ],
                Type = ["VerifiableCredential", "OpenBadgeCredential"],
                Id = credentialId,
                Name = "Sample Badge Credential",
                Issuer = new Issuer()
                {
                    Id = did,
                    Type = "Profile",
                    Name = "Prometric"
                },
                IssuanceDate = issuanceDate,
                ValidFrom = validFrom,
                CredentialSubject = credentialSubject
            };

            // Serialize -> canonicalize for signing
            string unsignedJson = JsonSerializer.Serialize(unsignedCredential);
            string canonicalJson = Canonicalizer.Canonicalize(unsignedJson);
            byte[] credentialBytes = Encoding.UTF8.GetBytes(canonicalJson);

            // Sign credential
            byte[] signature = Ed25519.Sign(credentialBytes, privateKey);
            string signatureBase58 = Base58.Bitcoin.Encode(signature);

            var proof = new Proof()
            {
                Type = "Ed25519Signature2020",
                Created = issuanceDate,
                VerificationMethod = did + "#key-1",
                ProofPurpose = "assertionMethod",
                Jws = signatureBase58
            };

            var signedCredential = new OpenBadgeCredentialWithProof
            {
                Context = unsignedCredential.Context,
                Type = unsignedCredential.Type,
                Id = unsignedCredential.Id,
                Name = unsignedCredential.Name,
                Issuer = unsignedCredential.Issuer,
                IssuanceDate = unsignedCredential.IssuanceDate,
                ValidFrom = unsignedCredential.ValidFrom,
                CredentialSubject = unsignedCredential.CredentialSubject,
                Proof = proof
            };
            issuedCredentials[credentialId] = JsonSerializer.Serialize(signedCredential);
            CandidateCredentials.Add(new KeyValuePair<string, string>(studentId, credentialId));
            return new JsonResult(signedCredential);
        }



        [HttpPost("verify-badge")]
        public async Task<IActionResult> VerifyBadge()
        {
            using var doc = await JsonDocument.ParseAsync(Request.Body);
            var root = doc.RootElement;

            if (!root.TryGetProperty("proof", out var proofElement))
            {
                return BadRequest("Missing 'proof' in badge.");
            }

            // Extract signature
            string signatureBase58 = proofElement.GetProperty("jws").GetString()!;
            byte[] signature = Base58.Bitcoin.Decode(signatureBase58);

            // Remove proof for canonicalization
            var unsignedDict = new Dictionary<string, JsonElement>();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name != "proof")
                    unsignedDict[prop.Name] = prop.Value;
            }
            string unsignedJson = JsonSerializer.Serialize(unsignedDict);
            string canonicalJson = Stratumn.CanonicalJson.Canonicalizer.Canonicalize(unsignedJson);
            byte[] unsignedBytes = Encoding.UTF8.GetBytes(canonicalJson);

            // Fetch public key from DID doc
            string verificationMethod = proofElement.GetProperty("verificationMethod").GetString()!;
            string didFromProof = verificationMethod.Split('#')[0];
            using var client = new HttpClient();
            string didDocJson = await client.GetStringAsync($"https://localhost:7184/.well-known/did.json");
            var didDoc = JsonDocument.Parse(didDocJson);
            string pubKeyBase58 = didDoc.RootElement
                .GetProperty("verificationMethod")[0]
                .GetProperty("publicKeyBase58").GetString()!;
            byte[] pubKey = Base58.Bitcoin.Decode(pubKeyBase58);

            // Verify signature
            bool valid = Ed25519.Verify(signature, unsignedBytes, pubKey);

            return new JsonResult(new { valid });
        }

        [HttpGet("/credentials/{id}")]
        public IActionResult GetCredential(string id)
        {
            string url = $"https://localhost:7184/credentials/{id}";
            if (issuedCredentials.TryGetValue(url, out var credentialJson))
            {
                return new JsonResult(JsonSerializer.Deserialize<object>(credentialJson)!,
                    new JsonSerializerOptions { WriteIndented = true });
            }
            return NotFound(new { message = "Credential not found" });
        }


        [HttpGet("/credentials/byUserId/{id}")]
        public IActionResult GetCredentialsByUserId(string id)
        {

            var credentialIds = CandidateCredentials
                .Where(c => c.Key == id)
                .Select(c => c.Value)
                .ToList();

            if (!credentialIds.Any())
            {
                return NotFound(new { message = "No credentials found for this user" });
            }

            var results = new List<object>();

            foreach (var credentialId in credentialIds)
            {
                if (issuedCredentials.TryGetValue(credentialId, out var credentialJson))
                {
                    var deserialized = JsonSerializer.Deserialize<object>(credentialJson);
                    if (deserialized != null)
                    {
                        results.Add(deserialized);
                    }
                }
            }

            if (!results.Any())
            {
                return NotFound(new { message = "No credentials issued for this user" });
            }

            return new JsonResult(results, new JsonSerializerOptions { WriteIndented = true });
        }

        [HttpGet("/credentials/{id}/view")]
        public IActionResult ViewCredential(string id)
        {
            string url = $"https://localhost:7184/credentials/{id}";
            if (!issuedCredentials.TryGetValue(url, out var credentialJson))
            {
                return NotFound("Credential not found");
            }

            var credential = JsonDocument.Parse(credentialJson).RootElement;
            string subjectId = credential.GetProperty("credentialSubject").GetProperty("id").GetString() ?? "";
            string achievementName = credential.GetProperty("credentialSubject").GetProperty("achievement").GetProperty("name").GetString() ?? "";
            string image = credential.GetProperty("credentialSubject").GetProperty("achievement").GetProperty("image").GetProperty("id").GetString() ?? "";
            string issueDate = credential.GetProperty("issuanceDate").GetString() ?? "";
            string issuer = credential.GetProperty("issuer").GetProperty("name").GetString() ?? "";

            string html = $@"
            <html>
            <head>
                <title>Badge Verification</title>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 40px; }}
                    .badge {{ text-align: center; }}
                    img {{ max-width: 200px; margin-bottom: 20px; }}
                    .details {{ margin-top: 20px; }}
                    .details p {{ margin: 5px 0; }}
                    .verify {{ margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='badge'>
                    <h2>Open Badge Credential</h2>
                    <img src='{image}' alt='Badge Image' />
                    <div class='details'>
                        <p><strong>Recipient:</strong> {subjectId}</p>
                        <p><strong>Achievement:</strong> {achievementName}</p>
                        <p><strong>Issuer:</strong> {issuer}</p>
                        <p><strong>Issued on:</strong> {issueDate}</p>
                    </div>
                </div>
            </body>
            </html>";

            return Content(html, "text/html");
        }
    }
}