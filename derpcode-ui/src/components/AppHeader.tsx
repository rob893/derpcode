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
      <NavbarBrand className="flex gap-4">
        <h1
          className="text-3xl font-bold text-primary cursor-pointer hover:text-primary-600 transition-colors"
          onClick={() => navigate('/problems')}
        >
          DerpCode
        </h1>
        <Button variant="ghost" color="secondary" onPress={() => navigate('/problems')} className="font-medium">
          Problems
        </Button>
        {isAuthenticated && isAdmin && (
          <Button variant="ghost" color="primary" onPress={() => navigate('/problems/new')} className="font-medium">
            Create Problem
          </Button>
        )}
      </NavbarBrand>

      <NavbarContent justify="end">
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
