import { useEffect, useState } from 'react';
import type { Problem } from '../types/models';

export const ProblemList = ({ onProblemSelect }: { onProblemSelect: (problem: Problem) => void }) => {
  const [problems, setProblems] = useState<Problem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProblems = async () => {
      try {
        const response = await fetch('http://localhost:3000/problems');
        if (!response.ok) {
          throw new Error('Failed to fetch problems');
        }
        const data = await response.json();
        setProblems(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchProblems();
  }, []);

  if (loading) return <div>Loading problems...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="problem-list">
      <h2>Problems</h2>
      <div className="problems">
        {problems.map(problem => (
          <div key={problem.id} className="problem-item" onClick={() => onProblemSelect(problem)}>
            <h3>{problem.name}</h3>
          </div>
        ))}
      </div>
    </div>
  );
};
