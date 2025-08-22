import React, { useState } from 'react';
import { Badge } from '../types/badge';
import { downloadUtils } from '../utils/downloadUtils';
import { linkedinService } from '../services/linkedinService';

interface BadgeCardProps {
  badge: Badge;
  onDelete: (badgeId: string) => void;
}

const BadgeCard: React.FC<BadgeCardProps> = ({ badge, onDelete }) => {
  const [loading, setLoading] = useState(false);
  const [sharing, setSharing] = useState(false);

  const handleDownloadPNG = async () => {
    setLoading(true);
    try {
      await downloadUtils.downloadBadgeAsPNG(badge);
    } catch (error) {
      console.error('Failed to download badge:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadPDF = async () => {
    setLoading(true);
    try {
      await downloadUtils.downloadBadgeAsPDF(badge);
    } catch (error) {
      console.error('Failed to download PDF:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleShareToLinkedIn = async () => {
    setSharing(true);
    try {
      await linkedinService.shareBadge(badge);
    } catch (error) {
      console.error('Failed to share to LinkedIn:', error);
    } finally {
      setSharing(false);
    }
  };

  const handleDelete = () => {
    if (window.confirm('Are you sure you want to delete this badge?')) {
      onDelete(badge.id);
    }
  };

  return (
    <div className="card h-100">
      <div className="card-body">
        <div className="text-center mb-3">
          {badge.imageUrl ? (
            <img 
              src={badge.imageUrl} 
              alt={badge.name}
              className="img-fluid rounded"
              style={{ maxHeight: '120px' }}
            />
          ) : (
            <div 
              className="bg-primary text-white rounded d-flex align-items-center justify-content-center"
              style={{ height: '120px' }}
            >
              <i className="fas fa-medal" style={{ fontSize: '3rem' }}></i>
            </div>
          )}
        </div>

        <h5 className="card-title text-center">{badge.name}</h5>
        <p className="card-text text-muted small">{badge.description}</p>
        
        <div className="text-center mb-3">
          <span className="badge bg-info me-2">
            <i className="fas fa-calendar me-1"></i>
            {new Date(badge.issuedDate).toLocaleDateString()}
          </span>
          {badge.expirationDate && (
            <span className="badge bg-warning">
              <i className="fas fa-clock me-1"></i>
              Expires {new Date(badge.expirationDate).toLocaleDateString()}
            </span>
          )}
        </div>

        <div className="d-flex justify-content-between align-items-center mb-2">
          <small className="text-muted">Issuer: {badge.issuer}</small>
          <span className="badge bg-success">
            <i className="fas fa-check-circle me-1"></i>
            OpenBadges v3.0
          </span>
        </div>

        <div className="btn-group w-100 mb-2" role="group">
          <button 
            className="btn btn-outline-primary btn-sm"
            onClick={handleDownloadPNG}
            disabled={loading}
          >
            <i className="fas fa-download me-1"></i>
            SVG
          </button>
          <button 
            className="btn btn-outline-primary btn-sm"
            onClick={handleDownloadPDF}
            disabled={loading}
          >
            <i className="fas fa-file-pdf me-1"></i>
            PDF
          </button>
        </div>

        <div className="d-grid gap-2">
          <button 
            className="btn btn-primary btn-sm"
            onClick={handleShareToLinkedIn}
            disabled={sharing}
          >
            {sharing ? (
              <>
                <span className="spinner-border spinner-border-sm me-2"></span>
                Sharing...
              </>
            ) : (
              <>
                <i className="fab fa-linkedin me-1"></i>
                Share to LinkedIn
              </>
            )}
          </button>
          
          <button 
            className="btn btn-outline-danger btn-sm"
            onClick={handleDelete}
          >
            <i className="fas fa-trash me-1"></i>
            Delete
          </button>
        </div>
      </div>
    </div>
  );
};

export default BadgeCard;
