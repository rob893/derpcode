import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { Language, ProblemDifficulty } from '../types/models';
import type { Problem, SubmissionResult } from '../types/models';
import { CodeEditor } from './CodeEditor';

export const ProblemView = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [problem, setProblem] = useState<Problem | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedLanguage, setSelectedLanguage] = useState<Language>(Language.JavaScript);
  const [code, setCode] = useState('');
  const [result, setResult] = useState<SubmissionResult | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const fetchProblem = async () => {
      try {
        const response = await fetch(`${import.meta.env.VITE_DERPCODE_API_BASE_URL}/api/v1/problems/${id}`);
        if (!response.ok) {
          throw new Error('Failed to fetch problem');
        }
        const data = await response.json();
        setProblem(data);

        // Set initial language and template
        const initialDriver = data.drivers.find((d: any) => d.language === Language.JavaScript) || data.drivers[0];
        if (initialDriver) {
          setSelectedLanguage(initialDriver.language);
          setCode(initialDriver.uiTemplate);
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchProblem();
  }, [id]);

  const handleSubmit = async () => {
    if (!problem) return;

    setSubmitting(true);
    try {
      const response = await fetch(
        `${import.meta.env.VITE_DERPCODE_API_BASE_URL}/api/v1/problems/${problem.id}/submissions`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            userCode: code,
            language: selectedLanguage
          })
        }
      );

      if (!response.ok) {
        throw new Error('Submission failed');
      }

      const result = await response.json();
      setResult(result);
    } catch (error) {
      console.error('Submission error:', error);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div>Loading problem...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!problem) return <div>Problem not found</div>;

  const formatValue = (value: any): string => {
    return JSON.stringify(value, null, 2);
  };

  return (
    <>
      <button className="back-button" onClick={() => navigate('/')}>
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
            <button onClick={handleSubmit} disabled={submitting || !code.trim()}>
              {submitting ? 'Submitting...' : 'Submit Solution'}
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
