import { getDifficultyColor, getDifficultyLabel, getLanguageLabel } from '../utilities';
import { Language, ProblemDifficulty } from '../../types/models';

describe('utilities', () => {
  test('getLanguageLabel special-cases CSharp', () => {
    expect(getLanguageLabel(Language.CSharp)).toBe('C#');
    expect(getLanguageLabel(Language.JavaScript)).toBe('JavaScript');
  });

  test('getDifficultyColor maps difficulty to semantic color', () => {
    expect(getDifficultyColor(ProblemDifficulty.VeryEasy)).toBe('success');
    expect(getDifficultyColor(ProblemDifficulty.Easy)).toBe('success');
    expect(getDifficultyColor(ProblemDifficulty.Medium)).toBe('warning');
    expect(getDifficultyColor(ProblemDifficulty.Hard)).toBe('danger');
    expect(getDifficultyColor(ProblemDifficulty.VeryHard)).toBe('danger');
    expect(getDifficultyColor('Weird' as unknown as ProblemDifficulty)).toBe('default');
  });

  test('getDifficultyLabel returns human-friendly labels', () => {
    expect(getDifficultyLabel(ProblemDifficulty.VeryEasy)).toBe('Very Easy');
    expect(getDifficultyLabel(ProblemDifficulty.Easy)).toBe('Easy');
    expect(getDifficultyLabel(ProblemDifficulty.Medium)).toBe('Medium');
    expect(getDifficultyLabel(ProblemDifficulty.Hard)).toBe('Hard');
    expect(getDifficultyLabel(ProblemDifficulty.VeryHard)).toBe('Very Hard');
    expect(getDifficultyLabel('Weird' as unknown as ProblemDifficulty)).toBe('Unknown');
  });
});
