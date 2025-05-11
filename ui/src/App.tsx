import { useState } from 'react';
import type { Problem } from './types/models';
import { ProblemList } from './components/ProblemList';
import { ProblemView } from './components/ProblemView';
import './App.css';

function App() {
  const [selectedProblem, setSelectedProblem] = useState<Problem | null>(null);

  return (
    <div className="app">
      <header>
        <h1>LeetCode Clone</h1>
      </header>
      <main>
        {selectedProblem ? (
          <div className="problem-container">
            <button className="back-button" onClick={() => setSelectedProblem(null)}>
              ← Back to Problems
            </button>
            <ProblemView problem={selectedProblem} />
          </div>
        ) : (
          <ProblemList onProblemSelect={setSelectedProblem} />
        )}
      </main>
    </div>
  );
}

export default App;
