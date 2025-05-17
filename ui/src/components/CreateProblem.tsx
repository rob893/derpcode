import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import { type DriverTemplate, type Problem, type ProblemDriver, Language } from '../types/models';
import { CodeEditor } from './CodeEditor';

export const CreateProblem = () => {
  const navigate = useNavigate();
  const [driverTemplates, setDriverTemplates] = useState<DriverTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedLanguage, setSelectedLanguage] = useState<Language>(Language.JavaScript);

  const [problem, setProblem] = useState<Partial<Problem>>({
    name: '',
    description: '',
    difficulty: 'easy',
    tags: [],
    input: [],
    expectedOutput: [],
    drivers: []
  });

  const [driverCode, setDriverCode] = useState('');
  const [uiTemplate, setUITemplate] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [problemInput, setProblemInput] = useState('');
  const [problemExpectedOutput, setProblemExpectedOutput] = useState('');

  useEffect(() => {
    const fetchDriverTemplates = async () => {
      try {
        const response = await fetch('https://localhost:7059/api/v1/driverTemplates');
        if (!response.ok) throw new Error('Failed to fetch driver templates');
        const templates: DriverTemplate[] = await response.json();
        setDriverTemplates(templates);

        // Set initial driver code from first available template
        if (templates.length > 0) {
          const firstLanguage = templates[0].language;
          setSelectedLanguage(firstLanguage);
          setDriverCode(templates[0].template);
          setUITemplate(templates[0].uiTemplate);
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch driver templates');
      } finally {
        setLoading(false);
      }
    };

    fetchDriverTemplates();
  }, []);

  const handleLanguageChange = (newLanguage: Language) => {
    setSelectedLanguage(newLanguage);
    setDriverCode(driverTemplates.find(x => x.language === newLanguage)?.template || '');
    setUITemplate(driverTemplates.find(x => x.language === newLanguage)?.uiTemplate || '');
  };

  const handleAddTag = () => {
    if (tagInput.trim() && !problem.tags?.includes(tagInput.trim())) {
      setProblem(prev => ({
        ...prev,
        tags: [...(prev.tags || []), tagInput.trim()]
      }));
      setTagInput('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setProblem(prev => ({
      ...prev,
      tags: prev.tags?.filter(tag => tag !== tagToRemove) || []
    }));
  };

  const handleSubmit = async () => {
    try {
      // Create a driver for the current language
      const driver: ProblemDriver = {
        id: crypto.randomUUID(),
        language: selectedLanguage,
        image: `code-executor-${selectedLanguage.toLowerCase()}`,
        driverCode,
        uiTemplate
      };

      const newProblem: Problem = {
        ...(problem as Problem),
        id: crypto.randomUUID(),
        drivers: [driver]
      };

      const response = await fetch('https://localhost:7059/api/v1/problems', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(newProblem)
      });

      if (!response.ok) throw new Error('Failed to create problem');

      const createdProblem = await response.json();
      navigate(`/problems/${createdProblem.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create problem');
    }
  };

  if (loading) return <div>Loading driver templates...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="create-problem">
      <button className="back-button" onClick={() => navigate('/')}>
        ← Back to Problems
      </button>

      <h2>Create New Problem</h2>

      <div className="form-group">
        <label>Name:</label>
        <input
          type="text"
          value={problem.name}
          onChange={e => setProblem(prev => ({ ...prev, name: e.target.value }))}
          placeholder="Problem name"
        />
      </div>

      <div className="form-group">
        <label>Description:</label>
        <textarea
          value={problem.description}
          onChange={e => setProblem(prev => ({ ...prev, description: e.target.value }))}
          placeholder="Problem description"
          rows={4}
        />
      </div>

      <div className="form-group">
        <label>Difficulty:</label>
        <select
          value={problem.difficulty}
          onChange={e => setProblem(prev => ({ ...prev, difficulty: e.target.value as 'easy' | 'medium' | 'hard' }))}
        >
          <option value="easy">Easy</option>
          <option value="medium">Medium</option>
          <option value="hard">Hard</option>
        </select>
      </div>

      <div className="form-group">
        <label>Tags:</label>
        <div className="tag-input">
          <input
            type="text"
            value={tagInput}
            onChange={e => setTagInput(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleAddTag()}
            placeholder="Add tag and press Enter"
          />
          <button type="button" onClick={handleAddTag}>
            Add Tag
          </button>
        </div>
        <div className="tags-list">
          {problem.tags?.map(tag => (
            <span key={tag} className="tag">
              {tag}
              <button onClick={() => handleRemoveTag(tag)}>&times;</button>
            </span>
          ))}
        </div>
      </div>

      <div className="form-group">
        <label>Input Array (JSON):</label>
        <textarea
          value={problemInput}
          onChange={e => {
            try {
              setProblemInput(e.target.value);
              const parsed = JSON.parse(e.target.value);
              setProblem(prev => ({ ...prev, input: parsed }));
            } catch (e) {
              console.error('Invalid JSON:', e);
            }
          }}
          placeholder="[1, 2, 3]"
          rows={4}
        />
      </div>

      <div className="form-group">
        <label>Expected Output Array (JSON):</label>
        <textarea
          value={problemExpectedOutput}
          onChange={e => {
            try {
              setProblemExpectedOutput(e.target.value);
              const parsed = JSON.parse(e.target.value);
              setProblem(prev => ({ ...prev, expectedOutput: parsed }));
            } catch (e) {
              console.error('Invalid JSON:', e);
            }
          }}
          placeholder="[3, 5, 7]"
          rows={4}
        />
      </div>

      <div className="form-group">
        <label>Language:</label>
        <select value={selectedLanguage} onChange={e => handleLanguageChange(e.target.value as Language)}>
          {driverTemplates.map(x => (
            <option key={x.language} value={x.language}>
              {x.language}
            </option>
          ))}
        </select>
      </div>

      <div className="form-group">
        <label>UI Template:</label>
        <CodeEditor
          language={selectedLanguage}
          code={uiTemplate}
          onChange={value => setUITemplate(value ?? '')}
          uiTemplate=""
        />
      </div>

      <div className="form-group">
        <label>Driver Code:</label>
        <CodeEditor
          language={selectedLanguage}
          code={driverCode}
          onChange={value => setDriverCode(value ?? '')}
          uiTemplate=""
        />
      </div>

      <div className="form-actions">
        <button
          className="submit-button"
          onClick={handleSubmit}
          disabled={!problem.name || !problem.description || !driverCode}
        >
          Create Problem
        </button>
      </div>
    </div>
  );
};
