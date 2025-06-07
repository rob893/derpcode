import { Routes, Route, Navigate } from 'react-router';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminRoute } from './components/AdminRoute';
import { AppLayout } from './layouts/AppLayout';
import { ProblemList } from './components/ProblemList';
import { ProblemView } from './components/ProblemView';
import { CreateEditProblem } from './components/CreateEditProblem';
import { LandingPage } from './pages/LandingPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { OAuthCallbackPage } from './pages/OAuthCallbackPage';
import { ConfirmEmailPage } from './pages/ConfirmEmailPage';
import { PrivacyPolicyPage } from './pages/PrivacyPolicyPage';
import { AccountPage } from './pages/AccountPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
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
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route path="/privacy" element={<PrivacyPolicyPage />} />
          <Route path="/auth/github/callback" element={<OAuthCallbackPage provider="github" />} />
          <Route path="/auth/google/callback" element={<OAuthCallbackPage provider="google" />} />
          <Route path="/confirm-email" element={<ConfirmEmailPage />} />

          {/* Public problem routes */}
          <Route
            path="/problems"
            element={
              <AppLayout>
                <ProblemList />
              </AppLayout>
            }
          />
          <Route
            path="/problems/:id"
            element={
              <AppLayout>
                <ProblemView />
              </AppLayout>
            }
          />

          {/* Protected routes */}
          <Route
            path="/problems/new"
            element={
              <ProtectedRoute>
                <AdminRoute>
                  <AppLayout>
                    <CreateEditProblem mode="create" />
                  </AppLayout>
                </AdminRoute>
              </ProtectedRoute>
            }
          />

          <Route
            path="/problems/:id/edit"
            element={
              <ProtectedRoute>
                <AdminRoute>
                  <AppLayout>
                    <CreateEditProblem mode="edit" />
                  </AppLayout>
                </AdminRoute>
              </ProtectedRoute>
            }
          />

          <Route
            path="/account"
            element={
              <ProtectedRoute>
                <AppLayout>
                  <AccountPage />
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
