# Blockcerts Digital Certificate System

## Overview

The Blockcerts Digital Certificate System is a complete implementation of the [Blockcerts standard](https://www.blockcerts.org/) for creating, issuing, and verifying blockchain-anchored digital certificates. This system is architecturally separate from the existing OpenBadges implementation and provides full support for blockchain-based credential verification.

## Features

- **Standards Compliance**: Full support for Blockcerts 3.0 specification
- **Blockchain Anchoring**: Support for Bitcoin and Ethereum blockchain anchoring
- **Merkle Proof Verification**: Complete Merkle tree proof validation
- **JSON-LD Canonicalization**: Proper credential hashing using JSON-LD normalization
- **Cryptographic Verification**: Digital signature validation and proof verification
- **Multi-Network Support**: Bitcoin, Ethereum (mainnet and testnet), and mock chains
- **RESTful API**: Complete API for certificate management and verification
- **Issuer Profile Management**: Full issuer profile support with public keys and revocation lists

## Architecture

### Core Components

1. **Models (`/Models/`)**
   - `BlockcertsCredential.cs` - Main credential structure
   - `BlockcertsProof.cs` - Blockchain proof and anchoring models
   - `BlockcertsCertificate.cs` - Database entity and validation results

2. **Services (`/Services/`)**
   - `BlockcertsService.cs` - Core business logic for creation, validation, and verification

3. **Controllers (`/Controllers/`)**
   - `BlockcertsController.cs` - REST API endpoints

4. **Data Access**
   - Integrated with Entity Framework Core
   - Separate database table for Blockcerts certificates

### System Separation

The Blockcerts system is completely separate from the OpenBadges system:
- Independent models and data structures
- Separate service layer (`BlockcertsService` vs `BadgeValidationService`)
- Distinct API endpoints (`/api/blockcerts` vs `/api/badges`)
- Separate database table (`BlockcertsCertificates` vs `Badges`)
- No shared business logic or dependencies

## API Reference

### Base URL
```
https://your-domain.com/api/blockcerts
```

### Authentication
All endpoints (except validation) require JWT Bearer token authentication.

### Endpoints

#### 1. Get All Certificates
```http
GET /api/blockcerts
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "id": "uuid",
    "name": "Certificate Name",
    "description": "Certificate Description",
    "issuerName": "Issuer Name",
    "recipientId": "Recipient ID",
    "issuanceDate": "2024-01-01T00:00:00Z",
    "expirationDate": null,
    "imageUrl": "https://example.com/image.png",
    "isAnchored": true,
    "transactionId": "0x123...",
    "blockchainNetwork": "ethereumTestnet",
    "isRevoked": false,
    "credentialJson": "{...}"
  }
]
```

#### 2. Get Single Certificate
```http
GET /api/blockcerts/{id}
Authorization: Bearer <token>
```

#### 3. Create Certificate
```http
POST /api/blockcerts
Authorization: Bearer <token>
Content-Type: application/json

{
  "recipientId": "did:example:recipient123",
  "recipientName": "John Doe",
  "badgeName": "Software Engineering Certificate",
  "badgeDescription": "Certificate of completion for software engineering course",
  "issuerName": "Acme University",
  "issuerUrl": "https://acme-university.edu",
  "issuerEmail": "certificates@acme-university.edu",
  "imageUrl": "https://acme-university.edu/badge.png",
  "expirationDate": "2025-12-31T23:59:59Z"
}
```

#### 4. Anchor Certificate to Blockchain
```http
POST /api/blockcerts/{id}/anchor
Authorization: Bearer <token>
Content-Type: application/json

{
  "network": "ethereum",  // "bitcoin" or "ethereum"
  "testnet": true
}
```

**Response:**
```json
{
  "message": "Certificate anchored successfully",
  "transactionId": "0x123abc...",
  "blockchainNetwork": "ethereumTestnet",
  "merkleRoot": "abc123...",
  "proof": {
    "type": "MerkleProof2019",
    "created": "2024-01-01T00:00:00Z",
    "merkleRoot": "abc123...",
    "targetHash": "def456...",
    "anchors": [...]
  }
}
```

#### 5. Verify Certificate
```http
POST /api/blockcerts/{id}/verify
Authorization: Bearer <token>
```

**Response:**
```json
{
  "isValid": true,
  "isBlockchainVerified": true,
  "transactionId": "0x123abc...",
  "blockchainNetwork": "ethereumTestnet",
  "errors": [],
  "warnings": [],
  "isRevoked": false,
  "verificationDate": "2024-01-01T00:00:00Z"
}
```

#### 6. Validate Credential Structure (Public)
```http
POST /api/blockcerts/validate
Content-Type: application/json

{
  "@context": [...],
  "type": [...],
  "id": "...",
  "issuer": {...},
  "issuanceDate": "...",
  "credentialSubject": {...},
  "proof": {...}
}
```

#### 7. Delete Certificate
```http
DELETE /api/blockcerts/{id}
Authorization: Bearer <token>
```

## Usage Examples

### Creating a Certificate

```csharp
// Using the BlockcertsService directly
var service = new BlockcertsService();

var issuer = new BlockcertsIssuer
{
    Context = new[] { "https://www.w3.org/2018/credentials/v1", "https://www.blockcerts.org/schema/3.0/context.json" },
    Type = new[] { "Profile", "Issuer" },
    Id = "https://university.edu/issuer",
    Name = "University Name",
    PublicKey = new[] { /* public key info */ }
};

var credential = service.CreateCredential(
    recipientId: "did:example:student123",
    recipientName: "Jane Smith",
    badgeName: "Computer Science Degree",
    badgeDescription: "Bachelor of Science in Computer Science",
    issuer: issuer,
    expirationDate: DateTime.UtcNow.AddYears(5)
);
```

### Anchoring to Blockchain

```csharp
// Hash the credential
var targetHash = service.HashCredential(credential);

// Anchor to Ethereum testnet
var anchor = await service.AnchorToEthereumAsync(targetHash, testnet: true);

// Create Merkle proof
var proof = service.CreateMerkleProof(targetHash, targetHash, Array.Empty<MerkleProofData>(), new[] { anchor });

// Add proof to credential
credential.Proof = proof;
```

### Verification

```csharp
// Validate credential structure
var validationResult = service.ValidateBlockcertsCredential(credential);

// Verify blockchain anchor
if (credential.Proof?.Anchors != null)
{
    foreach (var anchor in credential.Proof.Anchors)
    {
        var isValid = await service.VerifyBlockchainAnchor(anchor);
        // Handle verification result
    }
}

// Verify Merkle proof
if (credential.Proof?.MerkleProof != null)
{
    var isMerkleValid = service.VerifyMerkleProof(credential.Proof.MerkleProof);
    // Handle Merkle verification result
}
```

## Blockchain Integration

### Supported Networks

1. **Bitcoin**
   - Mainnet: `bitcoinMainnet`
   - Testnet: `bitcoinTestnet`
   - Uses OP_RETURN for data anchoring

2. **Ethereum**
   - Mainnet: `ethereumMainnet`
   - Testnet: `ethereumTestnet`
   - Uses transaction data for anchoring

3. **Mock/Test**
   - Network: `mockchain`
   - For development and testing

### Transaction Format

Bitcoin transactions include the Merkle root in the OP_RETURN output. Ethereum transactions include the Merkle root in the transaction data field.

## Security Features

### Digital Signatures
- Ed25519 and RSA signature support
- Public key verification through issuer profiles
- Cryptographic proof validation

### Blockchain Anchoring
- Immutable timestamp proof
- Tamper-evident verification
- Distributed ledger verification

### Revocation
- Issuer-controlled revocation lists
- Real-time revocation checking
- Cryptographic revocation proofs

## Configuration

### Database Setup
The system automatically creates the necessary database tables through Entity Framework migrations:

```csharp
// In AppDbContext.cs
public DbSet<BlockcertsCertificate> BlockcertsCertificates { get; set; }
```

### Service Registration
Services are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<BlockcertsService>();
```

### Dependencies
Required NuGet packages:
- `NBitcoin` - Bitcoin network integration
- `Nethereum.Web3` - Ethereum network integration
- `System.Security.Cryptography.Algorithms` - Cryptographic operations
- `Newtonsoft.Json` - JSON processing

## Development Notes

### JSON-LD Canonicalization
The system implements a simplified JSON-LD canonicalization algorithm. For production use with complex JSON-LD documents, consider using a full JSON-LD library.

### Blockchain Integration
Current implementation provides simulated blockchain anchoring for development. For production:
- Implement actual Bitcoin/Ethereum network integration
- Add proper transaction fee handling
- Implement retry logic for network failures
- Add transaction confirmation waiting

### Performance Considerations
- Credential hashing is CPU-intensive for large documents
- Blockchain verification involves network calls
- Consider caching for frequently verified credentials
- Batch processing for multiple certificate operations

## Testing

The system includes comprehensive validation:
- Credential structure validation
- Blockchain anchor verification
- Merkle proof validation
- Issuer profile verification
- Revocation status checking

## Future Extensions

### Additional Blockchain Networks
The architecture supports easy addition of new blockchain networks:

```csharp
public async Task<BlockchainAnchor> AnchorToCustomBlockchainAsync(string merkleRoot)
{
    // Implement custom blockchain anchoring
}
```

### Advanced Verification
- Zero-knowledge proof support
- Multi-signature verification
- Threshold signature schemes
- Advanced revocation mechanisms

### Integration Features
- LDAP/Active Directory integration
- SAML/OAuth issuer authentication
- Automated certificate issuance workflows
- Bulk certificate processing

## License

This implementation follows the Blockcerts open standard specifications and is designed for educational and production use in compliance with relevant data protection regulations.