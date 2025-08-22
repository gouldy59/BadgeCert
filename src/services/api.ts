const API_BASE_URL = window.location.protocol + '//' + window.location.hostname + ':5001/api';

class ApiService {
  private getHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    };
  }

  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers: {
        ...this.getHeaders(),
        ...options.headers
      }
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || `HTTP ${response.status}: ${response.statusText}`);
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return response.json();
    }
    
    return response.text() as any;
  }

  async login(email: string, password: string): Promise<any> {
    return this.request<any>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });
  }

  async getCurrentUser(): Promise<any> {
    return this.request<any>('/auth/me');
  }

  async getBadges(): Promise<any[]> {
    return this.request<any[]>('/badges');
  }

  async createBadge(badgeData: any): Promise<any> {
    return this.request<any>('/badges', {
      method: 'POST',
      body: JSON.stringify(badgeData)
    });
  }

  async deleteBadge(badgeId: string): Promise<void> {
    return this.request<void>(`/badges/${badgeId}`, {
      method: 'DELETE'
    });
  }

  async getResults(): Promise<any[]> {
    return this.request<any[]>('/results');
  }

  async downloadBadgePng(badgeId: string): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/badges/${badgeId}/download/png`, {
      headers: this.getHeaders()
    });
    
    if (!response.ok) {
      throw new Error('Failed to download PNG');
    }
    
    return response.blob();
  }

  async downloadBadgePdf(badgeId: string): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/badges/${badgeId}/download/pdf`, {
      headers: this.getHeaders()
    });
    
    if (!response.ok) {
      throw new Error('Failed to download PDF');
    }
    
    return response.blob();
  }
}

export const apiService = new ApiService();
