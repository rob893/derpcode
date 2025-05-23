import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { ProblemDifficulty } from '../types/models';
import type { Problem, CursorPaginatedResponse } from '../types/models';

export const ProblemList = () => {
  const [problems, setProblems] = useState<Problem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchProblems = async () => {
      try {
        const response = await fetch('https://localhost:7059/api/v1/problems');
        if (!response.ok) {
          throw new Error('Failed to fetch problems');
        }
        const data: CursorPaginatedResponse<Problem> = await response.json();
        // Extract problems from paginated response
        const problemList = data.nodes || data.edges?.map(edge => edge.node) || [];
        setProblems(problemList);
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

  const getDifficultyColor = (difficulty: ProblemDifficulty) => {
    switch (difficulty) {
      case ProblemDifficulty.VeryEasy:
      case ProblemDifficulty.Easy:
        return '#00af9b';
      case ProblemDifficulty.Medium:
        return '#ffc01e';
      case ProblemDifficulty.Hard:
      case ProblemDifficulty.VeryHard:
        return '#ff375f';
      default:
        return '#808080';
    }
  };

  const getDifficultyLabel = (difficulty: ProblemDifficulty): string => {
    switch (difficulty) {
      case ProblemDifficulty.VeryEasy:
        return 'Very Easy';
      case ProblemDifficulty.Easy:
        return 'Easy';
      case ProblemDifficulty.Medium:
        return 'Medium';
      case ProblemDifficulty.Hard:
        return 'Hard';
      case ProblemDifficulty.VeryHard:
        return 'Very Hard';
      default:
        return 'Unknown';
    }
  };

  return (
    <div className="problem-list">
      <div className="list-header">
        <h2>Problems</h2>
        <button className="create-button" onClick={() => navigate('/problems/new')}>
          Create Problem
        </button>
      </div>
      <div className="problems">
        {problems.map(problem => (
          <div key={problem.id} className="problem-item" onClick={() => navigate(`/problems/${problem.id}`)}>
            <div className="problem-header">
              <h3>{problem.name}</h3>
              <span className="difficulty-badge" style={{ backgroundColor: getDifficultyColor(problem.difficulty) }}>
                {getDifficultyLabel(problem.difficulty)}
              </span>
            </div>
            {problem.tags && problem.tags.length > 0 && (
              <div className="problem-tags">
                {problem.tags.map((tag, index) => (
                  <span key={index} className="tag">
                    {tag.name}
                  </span>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};
