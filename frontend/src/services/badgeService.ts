import { apiService } from './api';
import { Badge } from '../types/badge';
import { ScoreReport } from '../types/scoreReport';

class BadgeService {
  async getBadges(): Promise<Badge[]> {
    return apiService.getBadges();
  }

  async createBadge(badgeData: any): Promise<Badge> {
    return apiService.createBadge(badgeData);
  }

  async deleteBadge(badgeId: string): Promise<void> {
    return apiService.deleteBadge(badgeId);
  }

  async getScoreReports(): Promise<ScoreReport[]> {
    return apiService.getScoreReports();
  }

  validateOpenBadgeV3(badgeData: any): { isValid: boolean; errors: string[] } {

    debugger;
    const errors: string[] = [];

    // Check @context
    if (!badgeData['@context'] || !Array.isArray(badgeData['@context'])) {
      errors.push('Missing or invalid @context array');
    } else {
      const requiredContexts = [
        'https://www.w3.org/ns/credentials/v2',
        'https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json'
      ];
      
      for (const ctx of requiredContexts) {
        if (!badgeData['@context'].includes(ctx)) {
          errors.push(`Missing required context: ${ctx}`);
        }
      }
    }

    // Check type
    if (!badgeData.type || !Array.isArray(badgeData.type)) {
      errors.push('Missing or invalid type array');
    } else {
      const requiredTypes = ['VerifiableCredential', 'OpenBadgeCredential'];
      for (const type of requiredTypes) {
        if (!badgeData.type.includes(type)) {
          errors.push(`Missing required type: ${type}`);
        }
      }
    }

    // Check required fields
    const requiredFields = ['id', 'issuer', 'validFrom', 'credentialSubject'];
    for (const field of requiredFields) {
      if (!badgeData[field]) {
        errors.push(`Missing required field: ${field}`);
      }
    }

    // Check issuer structure
    if (badgeData.issuer) {
      if (!badgeData.issuer.id || !badgeData.issuer.type || !badgeData.issuer.name) {
        errors.push('Issuer must include id, type, and name');
      }
    }

    // Check credentialSubject structure
    if (badgeData.credentialSubject) {
      if (!badgeData.credentialSubject.id || !badgeData.credentialSubject.type) {
        errors.push('CredentialSubject must include id and type');
      }
      
      if (!badgeData.credentialSubject.achievement) {
        errors.push('CredentialSubject must include achievement');
      } else {
        const achievement = badgeData.credentialSubject.achievement;
        if (!achievement.id || !achievement.type || !achievement.name || !achievement.description) {
          errors.push('Achievement must include id, type, name, and description');
        }
      }
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }
}

export const badgeService = new BadgeService();
