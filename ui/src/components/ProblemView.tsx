import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { Language, ProblemDifficulty } from '../types/models';
import type { SubmissionResult } from '../types/models';
import { CodeEditor } from './CodeEditor';
import { useProblem, useSubmitSolution } from '../hooks/api';

export const ProblemView = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: problem, isLoading, error } = useProblem(Number(id!));
  const submitSolution = useSubmitSolution(problem?.id || 0);

  const [selectedLanguage, setSelectedLanguage] = useState<Language>(Language.JavaScript);
  const [code, setCode] = useState('');
  const [result, setResult] = useState<SubmissionResult | null>(null);

  // Set initial language and template when problem data is loaded
  useEffect(() => {
    if (problem) {
      const initialDriver = problem.drivers.find((d: any) => d.language === Language.JavaScript) || problem.drivers[0];
      if (initialDriver) {
        setSelectedLanguage(initialDriver.language);
        setCode(initialDriver.uiTemplate);
      }
    }
  }, [problem]);

  const handleSubmit = async () => {
    if (!problem) return;

    try {
      const submissionResult = await submitSolution.mutateAsync({
        userCode: code,
        language: selectedLanguage
      });
      setResult(submissionResult);
    } catch (error) {
      console.error('Submission error:', error);
    }
  };

  if (isLoading) return <div>Loading problem...</div>;
  if (error) return <div>Error: {error.message}</div>;
  if (!problem) return <div>Problem not found</div>;

  const formatValue = (value: any): string => {
    return JSON.stringify(value, null, 2);
  };

  return (
    <>
      <button className="back-button" onClick={() => navigate('/problems')}>
        ← Back to Problems
      </button>

      <div className="problem-view">
        <div className="problem-details">
          <div className="problem-header">
            <h2>{problem.name}</h2>
            <span
              className="difficulty-badge"
              style={{
                backgroundColor:
                  problem.difficulty === ProblemDifficulty.VeryEasy || problem.difficulty === ProblemDifficulty.Easy
                    ? '#00af9b'
                    : problem.difficulty === ProblemDifficulty.Medium
                      ? '#ffc01e'
                      : '#ff375f'
              }}
            >
              {problem.difficulty === ProblemDifficulty.VeryEasy
                ? 'Very Easy'
                : problem.difficulty === ProblemDifficulty.Easy
                  ? 'Easy'
                  : problem.difficulty === ProblemDifficulty.Medium
                    ? 'Medium'
                    : problem.difficulty === ProblemDifficulty.Hard
                      ? 'Hard'
                      : problem.difficulty === ProblemDifficulty.VeryHard
                        ? 'Very Hard'
                        : problem.difficulty}
            </span>
          </div>

          <div className="problem-tags">
            {problem.tags.map((tag, index) => (
              <span key={index} className="tag">
                {tag.name}
              </span>
            ))}
          </div>

          <div className="problem-description">
            <h3>Description</h3>
            <p>{problem.description}</p>
          </div>

          <div className="test-data">
            <div className="problem-input">
              <h3>Input</h3>
              <pre className="data-block">{formatValue(problem.input)}</pre>
            </div>

            <div className="problem-expected">
              <h3>Expected Output</h3>
              <pre className="data-block">{formatValue(problem.expectedOutput)}</pre>
            </div>
          </div>
        </div>

        <div className="code-section">
          <div className="language-selector">
            <select
              value={selectedLanguage}
              onChange={e => {
                const newLanguage = e.target.value as Language;
                setSelectedLanguage(newLanguage);
                const selectedDriver = problem.drivers.find(driver => driver.language === newLanguage);
                if (selectedDriver) {
                  setCode(selectedDriver.uiTemplate);
                }
              }}
            >
              {problem.drivers.map(driver => (
                <option key={driver.id} value={driver.language}>
                  {driver.language}
                </option>
              ))}
            </select>
          </div>

          <CodeEditor
            language={selectedLanguage}
            code={code}
            onChange={value => setCode(value ?? '')}
            uiTemplate={problem.drivers.find(d => d.language === selectedLanguage)?.uiTemplate ?? ''}
          />

          <div className="submission-controls">
            <button onClick={handleSubmit} disabled={submitSolution.isPending || !code.trim()}>
              {submitSolution.isPending ? 'Submitting...' : 'Submit Solution'}
            </button>
          </div>

          {result && (
            <div className={`submission-result ${result.pass ? 'success' : 'failure'}`}>
              <h4>{result.pass ? 'Success!' : 'Failed'}</h4>
              <div className="result-stats">
                <div className="stat">
                  <span>Test Cases:</span>
                  <span>{result.testCaseCount}</span>
                </div>
                <div className="stat">
                  <span>Passed:</span>
                  <span>{result.passedTestCases}</span>
                </div>
                <div className="stat">
                  <span>Failed:</span>
                  <span>{result.failedTestCases}</span>
                </div>
                <div className="stat">
                  <span>Execution Time:</span>
                  <span>{result.executionTimeInMs}ms</span>
                </div>
              </div>
              {result.errorMessage && (
                <div className="error-message">
                  <pre>{result.errorMessage}</pre>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
};
