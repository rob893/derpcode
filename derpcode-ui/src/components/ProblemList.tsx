import { useNavigate } from 'react-router';
import { ProblemDifficulty } from '../types/models';
import { useProblems } from '../hooks/api';

export const ProblemList = () => {
  const navigate = useNavigate();
  const { data: problems = [], isLoading, error } = useProblems();

  if (isLoading) return <div>Loading problems...</div>;
  if (error) return <div>Error: {error.message}</div>;

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
