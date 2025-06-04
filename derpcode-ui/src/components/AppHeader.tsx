import { useNavigate } from 'react-router';
import {
  Navbar,
  NavbarBrand,
  NavbarContent,
  NavbarItem,
  Button,
  Avatar,
  Dropdown,
  DropdownTrigger,
  DropdownMenu,
  DropdownItem
} from '@heroui/react';
import { useAuth } from '../hooks/useAuth';
import { hasAdminRole } from '../utils/auth';

export function AppHeader() {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const isAdmin = hasAdminRole(user);

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  return (
    <Navbar
      isBordered
      classNames={{
        base: 'bg-content1/95 backdrop-blur-md',
        wrapper: 'max-w-7xl'
      }}
    >
      <NavbarBrand>
        <div
          className="flex items-center gap-3 cursor-pointer hover:opacity-80 transition-opacity"
          onClick={() => navigate('/problems')}
        >
          <img src="/favicon.ico" alt="DerpCode Logo" className="w-8 h-8" />
          <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
        </div>
      </NavbarBrand>

      <NavbarContent justify="end" className="gap-4">
        <div className="hidden sm:flex items-center gap-6">
          <button
            onClick={() => navigate('/problems')}
            className="text-foreground hover:text-primary transition-colors font-medium cursor-pointer"
          >
            Problems
          </button>
          {isAuthenticated && isAdmin && (
            <button
              onClick={() => navigate('/problems/new')}
              className="text-foreground hover:text-primary transition-colors font-medium cursor-pointer"
            >
              Create Problem
            </button>
          )}
        </div>

        <div className="hidden sm:block w-px h-6 bg-divider"></div>
        {isAuthenticated ? (
          <>
            <NavbarItem className="hidden lg:flex">
              {user && <span className="text-default-600 mr-4">Welcome, {user.userName}</span>}
            </NavbarItem>
            <NavbarItem>
              <Dropdown placement="bottom-end">
                <DropdownTrigger>
                  <Avatar
                    isBordered
                    color="primary"
                    size="sm"
                    name={user?.userName?.[0] || 'U'}
                    className="cursor-pointer"
                  />
                </DropdownTrigger>
                <DropdownMenu aria-label="Profile Actions" variant="flat">
                  <DropdownItem key="account" onPress={() => navigate('/account')}>
                    Account
                  </DropdownItem>
                  <DropdownItem key="logout" color="danger" onPress={handleLogout} className="text-danger">
                    Sign Out
                  </DropdownItem>
                </DropdownMenu>
              </Dropdown>
            </NavbarItem>
          </>
        ) : (
          <>
            <NavbarItem>
              <Button variant="ghost" color="primary" onPress={() => navigate('/login')}>
                Sign In
              </Button>
            </NavbarItem>
            <NavbarItem>
              <Button color="primary" variant="solid" onPress={() => navigate('/register')}>
                Sign Up
              </Button>
            </NavbarItem>
          </>
        )}
      </NavbarContent>
    </Navbar>
  );
}
