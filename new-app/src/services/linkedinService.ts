import { Badge } from '../types/badge';

class LinkedInService {

  async shareBadge(badge: Badge): Promise<void> {
    try {
      // LinkedIn sharing URL
      const shareText = `I've earned the "${badge.name}" badge! ${badge.description}`;
      const shareUrl = encodeURIComponent(window.location.origin);
      
      const linkedinShareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${shareUrl}&text=${encodeURIComponent(shareText)}`;
      
      // Open LinkedIn share dialog
      const popup = window.open(
        linkedinShareUrl,
        'linkedin-share',
        'width=600,height=400,scrollbars=yes,resizable=yes'
      );

      if (!popup) {
        throw new Error('Popup blocked. Please allow popups and try again.');
      }

      // Check if popup was closed (user completed or cancelled)
      const checkClosed = setInterval(() => {
        if (popup.closed) {
          clearInterval(checkClosed);
          console.log('LinkedIn sharing completed');
        }
      }, 1000);

    } catch (error) {
      console.error('LinkedIn sharing failed:', error);
      throw error;
    }
  }

  async shareWithLinkedInAPI(_badge: Badge): Promise<void> {
    // This would require proper LinkedIn API integration with OAuth
    // For now, we'll use the simpler share URL approach above
    throw new Error('LinkedIn API integration not implemented. Using share URL instead.');
  }
}

export const linkedinService = new LinkedInService();
