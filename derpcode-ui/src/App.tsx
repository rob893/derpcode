import { Routes, Route, Navigate } from 'react-router';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AppLayout } from './layouts/AppLayout';
import { ProblemList } from './components/ProblemList';
import { ProblemView } from './components/ProblemView';
import { CreateProblem } from './components/CreateProblem';
import { LandingPage } from './pages/LandingPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import './App.css';

function App() {
  return (
    <AuthProvider>
      <div className="app min-h-screen bg-background text-foreground">
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Protected routes */}
          <Route
            path="/problems"
            element={
              <ProtectedRoute>
                <AppLayout>
                  <ProblemList />
                </AppLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/problems/new"
            element={
              <ProtectedRoute>
                <AppLayout>
                  <CreateProblem />
                </AppLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/problems/:id"
            element={
              <ProtectedRoute>
                <AppLayout>
                  <ProblemView />
                </AppLayout>
              </ProtectedRoute>
            }
          />

          {/* Catch all route */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </AuthProvider>
  );
}

export default App;
