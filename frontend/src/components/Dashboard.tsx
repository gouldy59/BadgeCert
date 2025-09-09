import React, { useState, useEffect } from 'react';
import BadgeCard from './BadgeCard';
import AddBadgeModal from './AddBadgeModal';
import { Badge, Result } from '../types/badge';
import { badgeService } from '../services/badgeService';

interface DashboardProps {
  user: any;
  onLogout: () => void;
}

const Dashboard: React.FC<DashboardProps> = ({ user, onLogout }) => {
  const [badges, setBadges] = useState<Badge[]>([]);
  const [results, setResults] = useState<Result[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'badges' | 'results'>('badges');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      debugger;
      setLoading(true);
      const [badgesData, resultsData] = await Promise.all([
        badgeService.getBadges(),
        badgeService.getResults()
      ]);
      debugger;
      setBadges(badgesData);
      setResults(resultsData);
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
            Badge Management
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
              className={`nav-link ${activeTab === 'results' ? 'active' : ''}`}
              onClick={() => setActiveTab('results')}
            >
              <i className="fas fa-chart-bar me-2"></i>
              My Results ({results.length})
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
        {activeTab === 'results' && (
          <div>
            <h3 className="mb-4">My Results</h3>
            
            {results.length === 0 ? (
              <div className="text-center py-5">
                <i className="fas fa-chart-bar text-muted" style={{ fontSize: '4rem' }}></i>
                <h4 className="mt-3 text-muted">No results yet</h4>
                <p className="text-muted">Your achievement results will appear here</p>
              </div>
            ) : (
              <div className="row">
                {results.map(result => (
                  <div key={result.id} className="col-md-6 mb-3">
                    <div className="card">
                      <div className="card-body">
                        <h5 className="card-title">{result.title}</h5>
                        <p className="card-text">{result.description}</p>
                        <div className="d-flex justify-content-between align-items-center">
                          <span className={`badge ${result.status === 'completed' ? 'bg-success' : 'bg-warning'}`}>
                            {result.status}
                          </span>
                          <small className="text-muted">
                            {new Date(result.achievedDate).toLocaleDateString()}
                          </small>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
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
