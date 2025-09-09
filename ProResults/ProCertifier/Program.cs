using System.Text.Json;
using System.Text;
using Chaos.NaCl;
using SimpleBase;
using Stratumn.CanonicalJson;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;
using ProCertifier.Models;

var issuedCredentials = new Dictionary<string, string>(); // my db :)

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve from "badges" folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "badges")),
    RequestPath = "/badges"
});
app.MapControllers();
// ===== Generate DID and Ed25519 Key Pair =====
string did = "did:web:localhost";

// Generate a random 32-byte seed
byte[] seed = new byte[32];
Random.Shared.NextBytes(seed);

// Generate public key from seed
byte[] publicKey = Ed25519.PublicKeyFromSeed(seed);

// Expand seed to private key for signing
byte[] privateKey = Ed25519.ExpandedPrivateKeyFromSeed(seed);

// Encode public key to Base58 for DID document
string publicKeyBase58 = Base58.Bitcoin.Encode(publicKey);

//// ===== DID Document Endpoint =====
//app.MapGet("/.well-known/did.json", () =>
//{
//    var didDoc = new
//    {
//        @context = "https://www.w3.org/ns/did/v1",
//        id = did,
//        verificationMethod = new[]
//        {
//            new {
//                id = did + "#key-1",
//                type = "Ed25519VerificationKey2020",
//                controller = did,
//                publicKeyBase58
//            }
//        },
//        assertionMethod = new[] { did + "#key-1" }
//    };
//    return Results.Json(didDoc, new JsonSerializerOptions { WriteIndented = true });
//});

//// ===== Issue Badge Endpoint =====
//app.MapPost("/issue-badge", async (HttpRequest request, HttpResponse response) =>
//{
//    using var doc = await JsonDocument.ParseAsync(request.Body);
//    if (!doc.RootElement.TryGetProperty("studentId", out var studentIdElement))
//    {
//        response.StatusCode = 400;
//        await response.WriteAsync("Missing 'studentId' in request body.");
//        return;
//    }

//    string studentId = studentIdElement.GetString() ?? throw new Exception("Invalid studentId");

//    var achievement = new Achievement()
//    {
//        Id = "https://certs.example.com/achievements/exam-123",
//        Type = ["Achievement"],
//        Name = "Certified Data Analyst Exam",
//        Description = "Awarded for passing the Data Analyst Exam with at least 80%.", 
//        Criteria = new Criteria() { Narrative = "Pass the online exam with 80% or higher." },
//        Image = new Image()
//        {
//            Id = "https://localhost:7184/badges/mybadge.jpg",
//            Type = "Image"

//        }, // <-- Badge image URL
//        Issuer = did
//    };
//    // Create unsigned credential
//    var issuanceDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
//    var validFrom = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
//    var credentialSubject = new CredentialSubject
//    {
//        Id = studentId,
//        Type = ["AchievementSubject"],
//        Achievement = achievement
//      , // Include image URL here
//    };

//    var credentialId = $"https://localhost:7184/credentials/{Guid.NewGuid()}";

//    var unsignedCredential = new OpenBadgeCredential
//    {
//        Context =
//        [
//            "https://www.w3.org/ns/credentials/v2",
//            "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
//        ],
//        Type = ["VerifiableCredential", "OpenBadgeCredential"],
//        Id = credentialId,
//        Name = "Sample Badge Credential",
//        Issuer = new Issuer()
//        {
//            Id = did,
//            Type = "Profile",
//            Name = "Prometric"
//        },
//        IssuanceDate = issuanceDate,
//        ValidFrom = validFrom,
//        CredentialSubject = credentialSubject
//    };

//    // Serialize -> canonicalize for signing
//    string unsignedJson = JsonSerializer.Serialize(unsignedCredential);
//    string canonicalJson = Canonicalizer.Canonicalize(unsignedJson);

//    //string canonicalJson = canonical.GetSerializedString();
//    byte[] credentialBytes = Encoding.UTF8.GetBytes(canonicalJson);


//    // Sign credential
//    byte[] signature = Ed25519.Sign(credentialBytes, privateKey);
//    string signatureBase58 = Base58.Bitcoin.Encode(signature);

//    var proof = new Proof()
//    {
//        Type = "Ed25519Signature2020",
//        Created = issuanceDate,
//        VerificationMethod = did + "#key-1",
//        ProofPurpose = "assertionMethod",
//        Jws = signatureBase58
//    };

//    var signedCredential = new OpenBadgeCredentialWithProof
//    {
//        Context = unsignedCredential.Context,
//        Type = unsignedCredential.Type,
//        Id = unsignedCredential.Id,
//        Name = unsignedCredential.Name,
//        Issuer = unsignedCredential.Issuer,
//        IssuanceDate = unsignedCredential.IssuanceDate,
//        ValidFrom = unsignedCredential.ValidFrom,
//        CredentialSubject = unsignedCredential.CredentialSubject,
//        Proof = proof
//    };
//    issuedCredentials[credentialId] = JsonSerializer.Serialize(signedCredential);

//    response.ContentType = "application/json";
//    await response.WriteAsync(JsonSerializer.Serialize(signedCredential));
//});

//// ===== Verify Badge Endpoint =====
//app.MapPost("/verify-badge", async (HttpRequest request, HttpResponse response) =>
//{
//    using var doc = await JsonDocument.ParseAsync(request.Body);
//    var root = doc.RootElement;

//    if (!root.TryGetProperty("proof", out var proofElement))
//    {
//        response.StatusCode = 400;
//        await response.WriteAsync("Missing 'proof' in badge.");
//        return;
//    }

//    // Extract signature
//    string signatureBase58 = proofElement.GetProperty("jws").GetString()!;
//    byte[] signature = Base58.Bitcoin.Decode(signatureBase58);

//    // Remove proof for canonicalization
//    var unsignedDict = new Dictionary<string, JsonElement>();
//    foreach (var prop in root.EnumerateObject())
//    {
//        if (prop.Name != "proof")
//            unsignedDict[prop.Name] = prop.Value;
//    }
//    string unsignedJson = JsonSerializer.Serialize(unsignedDict);
//    string canonicalJson = Canonicalizer.Canonicalize(unsignedJson);
//    byte[] unsignedBytes = Encoding.UTF8.GetBytes(canonicalJson);

//    // Fetch public key from DID doc
//    string verificationMethod = proofElement.GetProperty("verificationMethod").GetString()!;
//    string didFromProof = verificationMethod.Split('#')[0];
//    using var client = new HttpClient();
//    string didDocJson = await client.GetStringAsync($"https://localhost:7184/.well-known/did.json");
//    var didDoc = JsonDocument.Parse(didDocJson);
//    string pubKeyBase58 = didDoc.RootElement
//        .GetProperty("verificationMethod")[0]
//        .GetProperty("publicKeyBase58").GetString()!;
//    byte[] pubKey = Base58.Bitcoin.Decode(pubKeyBase58);

//    // Verify signature
//    bool valid = Ed25519.Verify(signature, unsignedBytes, pubKey);

//    response.ContentType = "application/json";
//    await response.WriteAsync(JsonSerializer.Serialize(new { valid }));
//});
//app.MapGet("/credentials/{id}", (string id) =>
//{
//    string url = $"https://localhost:7184/credentials/{id}";
//    if (issuedCredentials.TryGetValue(url, out var credentialJson))
//    {
//        return Results.Json(JsonSerializer.Deserialize<object>(credentialJson)!,
//            new JsonSerializerOptions { WriteIndented = true });
//    }
//    return Results.NotFound(new { message = "Credential not found" });
//});

//// ===== Public Verification Page =====
//app.MapGet("/credentials/{id}/view", (string id) =>
//{
//    string url = $"https://localhost:7184/credentials/{id}";
//    if (!issuedCredentials.TryGetValue(url, out var credentialJson))
//    {
//        return Results.NotFound("Credential not found");
//    }

//    var credential = JsonDocument.Parse(credentialJson).RootElement;
//    string subjectId = credential.GetProperty("credentialSubject").GetProperty("id").GetString() ?? "";
//    string achievementId = credential.GetProperty("credentialSubject").GetProperty("achievement").GetString() ?? "";
//    string image = credential.GetProperty("credentialSubject").GetProperty("image").GetString() ?? "";
//    string issueDate = credential.GetProperty("issuanceDate").GetString() ?? "";
//    string issuer = credential.GetProperty("issuer").GetString() ?? "";

//    string html = $@"
//    <html>
//    <head>
//        <title>Badge Verification</title>
//        <style>
//            body {{ font-family: Arial, sans-serif; margin: 40px; }}
//            .badge {{ text-align: center; }}
//            img {{ max-width: 200px; margin-bottom: 20px; }}
//            .details {{ margin-top: 20px; }}
//            .details p {{ margin: 5px 0; }}
//            .verify {{ margin-top: 20px; }}
//        </style>
//    </head>
//    <body>
//        <div class='badge'>
//            <h2>Open Badge Credential</h2>
//            <img src='{image}' alt='Badge Image' />
//            <div class='details'>
//                <p><strong>Recipient:</strong> {subjectId}</p>
//                <p><strong>Achievement:</strong> {achievementId}</p>
//                <p><strong>Issuer:</strong> {issuer}</p>
//                <p><strong>Issued on:</strong> {issueDate}</p>
//            </div>
//            <div class='verify'>
//                <form action='/verify-badge' method='post'>
//                    <input type='hidden' name='credential' value='{System.Web.HttpUtility.HtmlEncode(credentialJson)}' />
//                    <button type='submit'>Verify Badge</button>
//                </form>
//            </div>
//        </div>
//    </body>
//    </html>";

//    return Results.Content(html, "text/html");
//});
app.Run("https://0.0.0.0:7184");

