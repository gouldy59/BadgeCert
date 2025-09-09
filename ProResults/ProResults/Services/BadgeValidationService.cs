using ProResults.Models;
using Newtonsoft.Json;

namespace ProResults.Services
{
    public class BadgeValidationService
    {
        private readonly string[] RequiredContexts = {
            "https://www.w3.org/ns/credentials/v2",
            "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
        };

        private readonly string[] RequiredTypes = {
            "VerifiableCredential",
            "OpenBadgeCredential"
        };

        public ValidationResult ValidateOpenBadgeV3(OpenBadgeCredentialWithProof credential)
        {
            var errors = new List<string>();

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
                {
                    errors.Add("Missing credential id");
                }

                if (string.IsNullOrEmpty(credential.ValidFrom))
                {
                    errors.Add("Missing validFrom field");
                }
                else
                {
                    if (!DateTime.TryParse(credential.ValidFrom, out _))
                    {
                        errors.Add("Invalid validFrom date format");
                    }
                }

                // Validate issuer
                if (credential.Issuer == null)
                {
                    errors.Add("Missing issuer");
                }
                else
                {
                    if (string.IsNullOrEmpty(credential.Issuer.Id))
                    {
                        errors.Add("Missing issuer id");
                    }
                    if (string.IsNullOrEmpty(credential.Issuer.Type))
                    {
                        errors.Add("Missing issuer type");
                    }
                    if (string.IsNullOrEmpty(credential.Issuer.Name))
                    {
                        errors.Add("Missing issuer name");
                    }
                }

                // Validate credentialSubject
                if (credential.CredentialSubject == null)
                {
                    errors.Add("Missing credentialSubject");
                }
                else
                {
                    if (string.IsNullOrEmpty(credential.CredentialSubject.Id))
                    {
                        errors.Add("Missing credentialSubject id");
                    }
                    //if (string.IsNullOrEmpty(credential.CredentialSubject.Type))
                    //{
                    //    errors.Add("Missing credentialSubject type");
                    //}

                    // Validate achievement
                    if (credential.CredentialSubject.Achievement == null)
                    {
                        errors.Add("Missing achievement in credentialSubject");
                    }
                    else
                    {
                        var achievement = credential.CredentialSubject.Achievement;
                        if (string.IsNullOrEmpty(achievement.Id))
                        {
                            errors.Add("Missing achievement id");
                        }
                        //if (string.IsNullOrEmpty(achievement.Type))
                        //{
                        //    errors.Add("Missing achievement type");
                        //}
                        if (string.IsNullOrEmpty(achievement.Name))
                        {
                            errors.Add("Missing achievement name");
                        }
                        if (string.IsNullOrEmpty(achievement.Description))
                        {
                            errors.Add("Missing achievement description");
                        }
                        if (achievement.Criteria == null || string.IsNullOrEmpty(achievement.Criteria.Narrative))
                        {
                            errors.Add("Missing achievement criteria narrative");
                        }
                    }
                }

                // Validate validUntil if provided
                if (!string.IsNullOrEmpty(credential.ValidUntil))
                {
                    if (!DateTime.TryParse(credential.ValidUntil, out _))
                    {
                        errors.Add("Invalid validUntil date format");
                    }
                }

                // Additional structural validations
                ValidateCredentialStructure(credential, errors);

            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        private void ValidateCredentialStructure(OpenBadgeCredentialWithProof credential, List<string> errors)
        {
            // Check for recommended fields
            if (credential.Proof == null)
            {
                // This is a warning, not an error for basic validation
                // In production, you might want to enforce cryptographic proof
            }

            // Validate ID formats (basic URL validation)
            if (!string.IsNullOrEmpty(credential.Id) && !IsValidUrl(credential.Id))
            {
                errors.Add("Credential id must be a valid URL");
            }

            if (credential.Issuer != null && !string.IsNullOrEmpty(credential.Issuer.Id) && !IsValidUrl(credential.Issuer.Id))
            {
                errors.Add("Issuer id must be a valid URL");
            }

            if (credential.CredentialSubject?.Achievement != null && 
                !string.IsNullOrEmpty(credential.CredentialSubject.Achievement.Id) && 
                !IsValidUrl(credential.CredentialSubject.Achievement.Id))
            {
                errors.Add("Achievement id must be a valid URL");
            }
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        public bool VerifyCredentialSignature(OpenBadgeCredentialWithProof credential)
        {
            // In a production environment, this would:
            // 1. Extract the proof from the credential
            // 2. Verify the cryptographic signature
            // 3. Check the verification method
            // 4. Validate the proof purpose
            // 5. Ensure the signature matches the credential content

            // For this implementation, we'll do basic proof structure validation
            if (credential.Proof == null)
            {
                return false; // No proof provided
            }

            var proof = credential.Proof;
            return !string.IsNullOrEmpty(proof.Type) &&
                   !string.IsNullOrEmpty(proof.Created) &&
                   !string.IsNullOrEmpty(proof.VerificationMethod) &&
                   !string.IsNullOrEmpty(proof.ProofPurpose) &&
                   !string.IsNullOrEmpty(proof.ProofValue);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
