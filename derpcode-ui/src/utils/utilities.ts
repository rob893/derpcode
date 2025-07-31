import { Language, ProblemDifficulty } from '../types/models';

export const getLanguageLabel = (language: Language): string => {
  switch (language) {
    case Language.CSharp:
      return 'C#';
    default:
      return language;
  }
};

export const getDifficultyColor = (difficulty: ProblemDifficulty) => {
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

export const getDifficultyLabel = (difficulty: ProblemDifficulty): string => {
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
