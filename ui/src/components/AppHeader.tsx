import { useNavigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';

export function AppHeader() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  return (
    <header className="app-header">
      <div className="container">
        <div className="header-content">
          <div className="header-brand">
            <h1 onClick={() => navigate('/problems')}>DerpCode</h1>
          </div>

          <nav className="header-nav">
            <button className="nav-button" onClick={() => navigate('/problems')}>
              Problems
            </button>
            <button className="nav-button" onClick={() => navigate('/problems/new')}>
              Create Problem
            </button>
          </nav>

          <div className="header-user">
            {user && <span className="user-info">Welcome, {user.firstName || user.userName}</span>}
            <button className="logout-button" onClick={handleLogout}>
              Sign Out
            </button>
          </div>
        </div>
      </div>
    </header>
  );
}
