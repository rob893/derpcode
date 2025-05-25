import { Link, useNavigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';
import { useEffect } from 'react';

export function LandingPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated) {
      // If user is authenticated, redirect to problems
      navigate('/problems', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  return (
    <div className="landing-page">
      <header className="landing-header">
        <div className="container">
          <nav className="nav">
            <div className="nav-brand">
              <h1>DerpCode</h1>
            </div>
            <div className="nav-links">
              <Link to="/login" className="nav-link">
                Sign In
              </Link>
              <Link to="/register" className="nav-link register-link">
                Sign Up
              </Link>
            </div>
          </nav>
        </div>
      </header>

      <main className="landing-main">
        <section className="hero">
          <div className="container">
            <div className="hero-content">
              <h2>Practice Coding with DerpCode</h2>
              <p>
                Sharpen your programming skills with our collection of coding challenges. Practice algorithms, data
                structures, and problem-solving in multiple programming languages.
              </p>
              <div className="hero-actions">
                <Link to="/register" className="btn btn-primary">
                  Get Started
                </Link>
                <Link to="/login" className="btn btn-secondary">
                  Sign In
                </Link>
              </div>
            </div>
          </div>
        </section>

        <section className="features">
          <div className="container">
            <h3>Why Choose DerpCode?</h3>
            <div className="features-grid">
              <div className="feature">
                <div className="feature-icon">💡</div>
                <h4>Multiple Languages</h4>
                <p>Practice in JavaScript, TypeScript, Python, and more programming languages.</p>
              </div>
              <div className="feature">
                <div className="feature-icon">🎯</div>
                <h4>Difficulty Levels</h4>
                <p>From very easy to very hard - find challenges that match your skill level.</p>
              </div>
              <div className="feature">
                <div className="feature-icon">⚡</div>
                <h4>Instant Feedback</h4>
                <p>Get immediate results and detailed feedback on your solutions.</p>
              </div>
              <div className="feature">
                <div className="feature-icon">📊</div>
                <h4>Track Progress</h4>
                <p>Monitor your improvement and see detailed execution statistics.</p>
              </div>
            </div>
          </div>
        </section>
      </main>

      <footer className="landing-footer">
        <div className="container">
          <p>&copy; 2025 DerpCode. Start your coding journey today!</p>
        </div>
      </footer>
    </div>
  );
}
