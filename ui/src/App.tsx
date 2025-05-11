import { Routes, Route, Navigate } from 'react-router';
import { ProblemList } from './components/ProblemList';
import { ProblemView } from './components/ProblemView';
import { CreateProblem } from './components/CreateProblem';
import './App.css';

function App() {
  return (
    <div className="app">
      <header>
        <h1>LeetCode Clone</h1>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<ProblemList />} />
          <Route path="/problems/new" element={<CreateProblem />} />
          <Route path="/problems/:id" element={<ProblemView />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
