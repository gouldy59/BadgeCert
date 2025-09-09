import { Badge } from '../types/badge';
import { apiService } from '../services/api';

class DownloadUtils {
  async downloadBadgeAsPNG(badge: Badge): Promise<void> {
    try {
      const blob = await apiService.downloadBadgePng(badge.id);
      this.downloadBlob(blob, `${badge.name.replace(/[^a-z0-9]/gi, '_')}.png`);
    } catch (error) {
      console.error('PNG download failed:', error);
      throw error;
    }
  }
async VerifyBadge(badge: Badge): Promise<boolean> {
    try {
      const uid = badge.id.split("/").pop() || "";;

      const result = await apiService.verifyBadge(uid) ;
       return !!result;
    } catch (error) {
      console.error('PNG download failed:', error);
      throw error;
    }
  }
  async downloadBadgeAsPDF(badge: Badge): Promise<void> {
    try {
      const blob = await apiService.downloadBadgePdf(badge.id);
      this.downloadBlob(blob, `${badge.name.replace(/[^a-z0-9]/gi, '_')}.pdf`);
    } catch (error) {
      console.error('PDF download failed:', error);
      throw error;
    }
  }

  private downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}

export const downloadUtils = new DownloadUtils();
