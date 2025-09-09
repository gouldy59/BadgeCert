export interface ScoreReport {
  id: string;
  name: string;
  htmlBody: string;
  title: string;
  description: string;
  achievedDate: string;
  status: 'completed' | 'in-progress' | 'pending';
  logoUrl: string;
}
