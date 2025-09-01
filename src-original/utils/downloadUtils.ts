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

  generateBadgePNGLocally(badge: Badge): Promise<Blob> {
    return new Promise((resolve, reject) => {
      try {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        if (!ctx) {
          reject(new Error('Canvas context not available'));
          return;
        }

        canvas.width = 400;
        canvas.height = 400;

        // Background
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Border
        ctx.strokeStyle = '#007bff';
        ctx.lineWidth = 4;
        ctx.strokeRect(10, 10, canvas.width - 20, canvas.height - 20);

        // Badge icon (circle)
        ctx.fillStyle = '#007bff';
        ctx.beginPath();
        ctx.arc(canvas.width / 2, 120, 50, 0, 2 * Math.PI);
        ctx.fill();

        // Badge name
        ctx.fillStyle = '#333333';
        ctx.font = 'bold 24px Arial';
        ctx.textAlign = 'center';
        ctx.fillText(badge.name, canvas.width / 2, 220);

        // Badge description
        ctx.font = '16px Arial';
        ctx.fillStyle = '#666666';
        const words = badge.description.split(' ');
        let line = '';
        let y = 250;
        
        for (let n = 0; n < words.length; n++) {
          const testLine = line + words[n] + ' ';
          const metrics = ctx.measureText(testLine);
          const testWidth = metrics.width;
          
          if (testWidth > canvas.width - 40 && n > 0) {
            ctx.fillText(line, canvas.width / 2, y);
            line = words[n] + ' ';
            y += 20;
          } else {
            line = testLine;
          }
        }
        ctx.fillText(line, canvas.width / 2, y);

        // Issuer
        ctx.font = '14px Arial';
        ctx.fillStyle = '#999999';
        ctx.fillText(`Issued by: ${badge.issuer}`, canvas.width / 2, y + 40);

        // Date
        ctx.fillText(
          `Date: ${new Date(badge.issuedDate).toLocaleDateString()}`, 
          canvas.width / 2, 
          y + 60
        );

        canvas.toBlob((blob) => {
          if (blob) {
            resolve(blob);
          } else {
            reject(new Error('Failed to generate PNG'));
          }
        }, 'image/png');

      } catch (error) {
        reject(error);
      }
    });
  }
}

export const downloadUtils = new DownloadUtils();
