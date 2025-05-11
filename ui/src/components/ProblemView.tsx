import { useState, useEffect } from 'react';
import { type Problem, Language, type SubmissionResult } from '../types/models';
import { CodeEditor } from './CodeEditor';

interface ProblemViewProps {
  problem: Problem;
}

export const ProblemView = ({ problem }: ProblemViewProps) => {
  const [selectedLanguage, setSelectedLanguage] = useState<Language>(Language.JavaScript);
  const [selectedUITemplate, setSelectedUITemplate] = useState<string>('');
  const [code, setCode] = useState('');
  const [result, setResult] = useState<SubmissionResult | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // Set the initial code to the UI template of the selected language on initial render
  useEffect(() => {
    // Find the initial driver for JavaScript (or the first available driver)
    const initialDriver = problem.drivers.find(driver => driver.language === Language.JavaScript) || problem.drivers[0];

    if (initialDriver) {
      setSelectedLanguage(initialDriver.language);
      setSelectedUITemplate(initialDriver.uiTemplate);
      setCode(initialDriver.uiTemplate);
    }
  }, [problem]); // Only re-run if problem changes

  const handleSubmit = async () => {
    setSubmitting(true);
    try {
      const response = await fetch(`http://localhost:3000/problems/${problem.id}/submissions`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          userCode: code,
          language: selectedLanguage
        })
      });

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

  return (
    <div className="problem-view">
      <div className="problem-details">
        <h2>{problem.name}</h2>
        <div className="problem-input">
          <h3>Input</h3>
          <pre>{problem.input}</pre>
        </div>
        <div className="problem-expected">
          <h3>Expected Output</h3>
          <pre>{problem.expectedOutput}</pre>
        </div>
      </div>

      <div className="code-section">
        <div className="language-selector">
          <select
            value={selectedLanguage}
            onChange={e => {
              setSelectedLanguage(e.target.value as Language);
              const selectedDriver = problem.drivers.find(driver => driver.language === e.target.value);
              if (selectedDriver) {
                setSelectedUITemplate(selectedDriver.uiTemplate);
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
          uiTemplate={selectedUITemplate}
        />

        <div className="submission-controls">
          <button onClick={handleSubmit} disabled={submitting || !code.trim()}>
            {submitting ? 'Submitting...' : 'Submit Solution'}
          </button>
        </div>

        {result && (
          <div className={`submission-result ${result.pass ? 'success' : 'failure'}`}>
            {result.pass ? 'Success! All test cases passed.' : 'Some test cases failed.'}
          </div>
        )}
      </div>
    </div>
  );
};
