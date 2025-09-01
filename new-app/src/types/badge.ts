export interface Badge {
  id: string;
  name: string;
  description: string;
  issuer: string;
  issuedDate: string;
  expirationDate?: string;
  imageUrl?: string;
  credentialJson: string;
  isVerified: boolean;
}

export interface Result {
  id: string;
  title: string;
  description: string;
  status: 'completed' | 'in-progress' | 'pending';
  achievedDate: string;
  score?: number;
  badgeId?: string;
}

export interface OpenBadgeCredential {
  '@context': string[];
  type: string[];
  id: string;
  issuer: {
    id: string;
    type: string;
    name: string;
  };
  validFrom: string;
  validUntil?: string;
  credentialSubject: {
    id: string;
    type: string;
    achievement: {
      id: string;
      type: string;
      name: string;
      description: string;
      criteria: {
        narrative: string;
      };
      image?: {
        id: string;
        type: string;
      };
    };
  };
  proof?: {
    type: string;
    created: string;
    verificationMethod: string;
    proofPurpose: string;
    proofValue: string;
  };
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    name: string;
  };
}
