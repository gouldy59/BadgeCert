import React, { useState } from 'react';

interface AddBadgeModalProps {
  show: boolean;
  onClose: () => void;
  onAdd: (badgeData: any) => void;
}

const AddBadgeModal: React.FC<AddBadgeModalProps> = ({ show, onClose, onAdd }) => {
  const [badgeJson, setBadgeJson] = useState('');
  const [validationError, setValidationError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setValidationError('');

    try {
      // Parse and validate JSON
      const badgeData = JSON.parse(badgeJson);
      
      // Basic OpenBadges v3.0 validation
      if (!badgeData['@context'] || !Array.isArray(badgeData['@context'])) {
        throw new Error('Badge must include @context array');
      }
      
      if (!badgeData['@context'].includes('https://www.w3.org/ns/credentials/v2') ||
          !badgeData['@context'].includes('https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json')) {
        throw new Error('Badge must include required OpenBadges v3.0 context URLs');
      }
      
      if (!badgeData.type || !Array.isArray(badgeData.type) ||
          !badgeData.type.includes('VerifiableCredential') ||
          !badgeData.type.includes('OpenBadgeCredential')) {
        throw new Error('Badge must include type array with VerifiableCredential and OpenBadgeCredential');
      }
      
      if (!badgeData.issuer || !badgeData.credentialSubject || !badgeData.validFrom) {
        throw new Error('Badge must include issuer, credentialSubject, and validFrom properties');
      }

      await onAdd(badgeData);
      setBadgeJson('');
    } catch (error: any) {
      if (error instanceof SyntaxError) {
        setValidationError('Invalid JSON format');
      } else {
        setValidationError(error.message);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setBadgeJson('');
    setValidationError('');
    onClose();
  };

  const sampleBadge = {
    "@context": [
      "https://www.w3.org/ns/credentials/v2",
      "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
    ],
    "type": [
      "VerifiableCredential",
      "OpenBadgeCredential"
    ],
    "id": "https://example.org/credentials/badge-001",
    "issuer": {
      "id": "https://example.org",
      "type": "Profile",
      "name": "Example Organization"
    },
    "validFrom": new Date().toISOString(),
    "credentialSubject": {
      "id": "did:example:recipient",
      "type": "AchievementSubject",
      "achievement": {
        "id": "https://example.org/achievements/badge-001",
        "type": "Achievement",
        "name": "Sample Achievement",
        "description": "This is a sample OpenBadges v3.0 compliant badge",
        "criteria": {
          "narrative": "Awarded for completing the sample achievement"
        }
      }
    }
  };

  if (!show) return null;

  return (
    <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">
              <i className="fas fa-plus-circle me-2"></i>
              Add OpenBadges v3.0 Compliant Badge
            </h5>
            <button type="button" className="btn-close" onClick={handleClose}></button>
          </div>
          
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              <div className="alert alert-info">
                <i className="fas fa-info-circle me-2"></i>
                <strong>OpenBadges v3.0 Requirements:</strong>
                <ul className="mb-0 mt-2">
                  <li>Must include proper @context array with W3C and IMS URLs</li>
                  <li>Type must be ["VerifiableCredential", "OpenBadgeCredential"]</li>
                  <li>Must include issuer, credentialSubject, and validFrom</li>
                  <li>Should include cryptographic proof for verification</li>
                </ul>
              </div>

              {validationError && (
                <div className="alert alert-danger">
                  <i className="fas fa-exclamation-triangle me-2"></i>
                  {validationError}
                </div>
              )}

              <div className="mb-3">
                <label htmlFor="badgeJson" className="form-label">
                  Badge JSON (OpenBadges v3.0 format)
                </label>
                <textarea
                  id="badgeJson"
                  className="form-control"
                  rows={15}
                  value={badgeJson}
                  onChange={(e) => setBadgeJson(e.target.value)}
                  placeholder={JSON.stringify(sampleBadge, null, 2)}
                  required
                  disabled={loading}
                />
              </div>

              <div className="text-end">
                <button 
                  type="button"
                  className="btn btn-outline-secondary me-2"
                  onClick={() => setBadgeJson(JSON.stringify(sampleBadge, null, 2))}
                  disabled={loading}
                >
                  <i className="fas fa-file-import me-1"></i>
                  Use Sample
                </button>
              </div>
            </div>
            
            <div className="modal-footer">
              <button 
                type="button" 
                className="btn btn-secondary" 
                onClick={handleClose}
                disabled={loading}
              >
                Cancel
              </button>
              <button 
                type="submit" 
                className="btn btn-primary"
                disabled={loading || !badgeJson.trim()}
              >
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Validating...
                  </>
                ) : (
                  <>
                    <i className="fas fa-check me-1"></i>
                    Add Badge
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default AddBadgeModal;
