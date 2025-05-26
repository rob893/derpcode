import { useNavigate } from 'react-router';
import { Card, CardBody, Chip, Button, Spinner, Divider } from '@heroui/react';
import { ProblemDifficulty } from '../types/models';
import { useProblems } from '../hooks/api';

export const ProblemList = () => {
  const navigate = useNavigate();
  const { data: problems = [], isLoading, error } = useProblems();

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <Spinner size="lg" color="primary" label="Loading problems..." />
      </div>
    );
  }

  if (error) {
    return (
      <Card className="max-w-md mx-auto">
        <CardBody className="text-center py-8">
          <p className="text-danger text-lg">Error: {error.message}</p>
        </CardBody>
      </Card>
    );
  }

  const getDifficultyColor = (difficulty: ProblemDifficulty) => {
    switch (difficulty) {
      case ProblemDifficulty.VeryEasy:
      case ProblemDifficulty.Easy:
        return 'success';
      case ProblemDifficulty.Medium:
        return 'warning';
      case ProblemDifficulty.Hard:
      case ProblemDifficulty.VeryHard:
        return 'danger';
      default:
        return 'default';
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
    <div className="max-w-7xl mx-auto space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-3xl font-bold text-foreground">Problems</h2>
        <Button
          color="primary"
          variant="solid"
          size="lg"
          onPress={() => navigate('/problems/new')}
          className="font-semibold"
        >
          Create Problem
        </Button>
      </div>

      <Divider />

      <div className="grid gap-4">
        {problems.map(problem => (
          <Card
            key={problem.id}
            isPressable
            isHoverable
            onPress={() => navigate(`/problems/${problem.id}`)}
            className="transition-all duration-200 hover:scale-[1.02]"
          >
            <CardBody className="p-6">
              <div className="flex justify-between items-start mb-3">
                <h3 className="text-xl font-semibold text-foreground hover:text-primary transition-colors">
                  {problem.name}
                </h3>
                <Chip color={getDifficultyColor(problem.difficulty)} variant="flat" size="sm" className="font-medium">
                  {getDifficultyLabel(problem.difficulty)}
                </Chip>
              </div>

              {problem.tags && problem.tags.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {problem.tags.map((tag, index) => (
                    <Chip key={index} size="sm" variant="bordered" color="secondary" className="text-xs">
                      {tag.name}
                    </Chip>
                  ))}
                </div>
              )}
            </CardBody>
          </Card>
        ))}
      </div>
    </div>
  );
};
