import React, { useState, useEffect } from 'react';
import BadgeCard from './BadgeCard';
import AddBadgeModal from './AddBadgeModal';
import { Badge } from '../types/badge';
import { ScoreReport } from '../types/scoreReport';
import { badgeService } from '../services/badgeService';

interface DashboardProps {
  user: any;
  onLogout: () => void;
}

const Dashboard: React.FC<DashboardProps> = ({ user, onLogout }) => {
  const [badges, setBadges] = useState<Badge[]>([]);
  const [scoreReports, setScoreReports] = useState<ScoreReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'badges' | 'scoreReports' | 'My Certificates'>('badges');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      debugger;
      setLoading(true);
      const [badgesData, scoreReportData] = await Promise.all([
        badgeService.getBadges(),
        badgeService.getScoreReports()
      ]);
      debugger;
      setBadges(badgesData);
      setScoreReports(scoreReportData);
    } catch (error: any) {
      setError('Failed to load data: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleAddBadge = async (badgeData: any) => {
    try {
      const newBadge = await badgeService.createBadge(badgeData);
      setBadges(prev => [...prev, newBadge]);
      setShowAddModal(false);
    } catch (error: any) {
      setError('Failed to add badge: ' + error.message);
    }
  };
  const handleDownloadImage = async (scoreReportId: string) => {
    setLoading(true);
    try {
          const response = await fetch(`http://localhost:5002/api/ScoreReports/image/${scoreReportId}`);
          if (!response.ok) {
            // handle error
            return;
          }
          const blob = await response.blob();
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = 'ScoreReport.png'; // or use dynamic name
          document.body.appendChild(link);
          link.click();
          link.remove();
          window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to download image:', error);
    } finally {
      setLoading(false);
    }
  };  const handleDownloadPdf = async (scoreReportId: string)  => {
      setLoading(true);
      try {
          const response = await fetch(`http://localhost:5002/api/ScoreReports/pdf/${scoreReportId}`);
          if (!response.ok) {
            // handle error
            return;
          }
          const blob = await response.blob();
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = 'ScoreReport.pdf'; // or use dynamic name
          document.body.appendChild(link);
          link.click();
          link.remove();
          window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error('Failed to download pdf:', error);
      } finally {
        setLoading(false);
      }
    };  


    const handleDisplayHtml = async (scoreReportId: string)  => {
        setLoading(true);
        try {
           const response = await fetch(`http://localhost:5002/api/ScoreReports/html/${scoreReportId}`);
            if (!response.ok) {
              alert('Failed to fetch HTML');
              return;
            }
            const htmlText = await response.text();

            // Open a new window and write the HTML into it
            const win = window.open('', '_blank');
            if (win) {

            win.document.open();
            win.document.write(htmlText);
            win.document.close();
            }
            else
            {
                alert("Could not open new window. Please allow popups for this site.");
            }
        } catch (error) {
          console.error('Failed to download badge:', error);
        } finally {
          setLoading(false);
        }
      };


  const handleDeleteBadge = async (badgeId: string) => {
    try {
      await badgeService.deleteBadge(badgeId);
      setBadges(prev => prev.filter(badge => badge.id !== badgeId));
    } catch (error: any) {
      setError('Failed to delete badge: ' + error.message);
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="min-vh-100 bg-light">
      {/* Navigation */}
      <nav className="navbar navbar-expand-lg navbar-dark bg-primary">
        <div className="container">
          <span className="navbar-brand">
            <i className="fas fa-medal me-2"></i>
           My Result Portal
          </span>
          
          <div className="navbar-nav ms-auto">
            <span className="navbar-text me-3">
              Welcome, {user.name}
            </span>
            <button 
              className="btn btn-outline-light"
              onClick={onLogout}
            >
              <i className="fas fa-sign-out-alt me-1"></i>
              Logout
            </button>
          </div>
        </div>
      </nav>

      <div className="container mt-4">
        {error && (
          <div className="alert alert-danger alert-dismissible" role="alert">
            <i className="fas fa-exclamation-triangle me-2"></i>
            {error}
            <button 
              type="button" 
              className="btn-close" 
              onClick={() => setError('')}
            ></button>
          </div>
        )}

        {/* Tabs */}
        <ul className="nav nav-tabs mb-4">
          <li className="nav-item">
            <button 
              className={`nav-link ${activeTab === 'badges' ? 'active' : ''}`}
              onClick={() => setActiveTab('badges')}
            >
              <i className="fas fa-medal me-2"></i>
              My Badges ({badges.length})
            </button>
          </li>
          <li className="nav-item">
            <button 
              className={`nav-link ${activeTab === 'scoreReports' ? 'active' : ''}`}
              onClick={() => setActiveTab('scoreReports')}
            >
              <i className="fas fa-chart-bar me-2"></i>
              My Score Reports ({scoreReports.length})
            </button>
          </li>
    <li className="nav-item">
            <button 
              className={`nav-link ${activeTab === 'My Certificates' ? 'active' : ''}`}
              onClick={() => setActiveTab('My Certificates')}
            >
              <i className="fas fa-chart-bar me-2"></i>
              My Certificates (0)
            </button>
          </li>

        </ul>

        {/* Badges Tab */}
        {activeTab === 'badges' && (
          <div>
            <div className="d-flex justify-content-between align-items-center mb-4">
              <h3>My Badges</h3>
              <button 
                className="btn btn-primary"
                onClick={() => setShowAddModal(true)}
              >
                <i className="fas fa-plus me-2"></i>
                Add Badge
              </button>
            </div>

            {badges.length === 0 ? (
              <div className="text-center py-5">
                <i className="fas fa-medal text-muted" style={{ fontSize: '4rem' }}></i>
                <h4 className="mt-3 text-muted">No badges yet</h4>
                <p className="text-muted">Add your first OpenBadges v3.0 compliant badge</p>
                <button 
                  className="btn btn-primary"
                  onClick={() => setShowAddModal(true)}
                >
                  Add Your First Badge
                </button>
              </div>
            ) : (
              <div className="row">
                {badges.map(badge => (
                  <div key={badge.id} className="col-md-4 mb-4">
                    <BadgeCard 
                      badge={badge} 
                      onDelete={handleDeleteBadge}
                    />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Results Tab */}
        {activeTab === 'scoreReports' && (
          <div>
            <h3 className="mb-4">My Score Reports</h3>
            
            {scoreReports.length === 0 ? (
              <div className="text-center py-5">
                <i className="fas fa-chart-bar text-muted" style={{ fontSize: '4rem' }}></i>
                <h4 className="mt-3 text-muted">No Score Reports yet</h4>
                <p className="text-muted">Your Score Reports will appear here</p>
              </div>
            ) : (
              <div className="row">
                {scoreReports.map(ScoreReport => (
                  <div key={ScoreReport.id} className="col-md-6 mb-3">
                    <div className="card h-100">
                      <div className="card-body">
                        <h5 className="card-title text-center">{ScoreReport.title}</h5>
                        <p className="card-text text-muted small">{ScoreReport.description}</p>
                            <div className="text-center mb-3">
                            <span className="badge bg-info me-2">
                              <i className="fas fa-calendar me-1"></i>
                                {new Date(ScoreReport.achievedDate).toLocaleDateString()}
                            </span>
                          </div>
                        </div>
                      
                      <div className="btn-group w-100 mb-2" role="group">
                        <button 
                        className="btn btn-outline-primary btn-sm"
                        onClick={() => handleDownloadImage(ScoreReport.id)}
                        disabled={loading}
                      > <i className="fas fa-download me-1"></i>Image</button>
                       <button 
                        className="btn btn-outline-primary btn-sm"
                        onClick={() => handleDownloadPdf(ScoreReport.id)}
                        disabled={loading}
                      > <i className="fas fa-file-pdf me-1"></i>Pdf</button>
                       <button 
                        className="btn btn-outline-primary btn-sm"
                        onClick={() => handleDisplayHtml(ScoreReport.id)}
                        disabled={loading}
                      > <i className="fas fa-code me-1"></i>Display Html</button>
                      </div>
                    </div></div>
                  
                ))}
              </div>
            )}
          </div>
        )}
        {/* Results Tab */}
        {activeTab === 'My Certificates' && (
          <div>
            <h3 className="mb-4">My Certificates</h3>
            
            {0 === 0 ? (
              <div className="text-center py-5">
                <i className="fas fa-chart-bar text-muted" style={{ fontSize: '4rem' }}></i>
                <h4 className="mt-3 text-muted">No Certificates yet</h4>
                <p className="text-muted">Certifacts will issued sing a Public CA-issued certificate</p>
              </div>
            ) : (
              <div className="row">
               
              </div>
            )}
          </div>
        )}
      </div>

      {/* Add Badge Modal */}
      <AddBadgeModal
        show={showAddModal}
        onClose={() => setShowAddModal(false)}
        onAdd={handleAddBadge}
      />
    </div>
  );
};

export default Dashboard;
