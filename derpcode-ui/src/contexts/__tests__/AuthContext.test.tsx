import { useContext, useEffect } from 'react';
import { act } from 'react';
import { createRoot, type Root } from 'react-dom/client';
import { jest } from '@jest/globals';

const mockAuthApi = {
  login: jest.fn() as any,
  loginWithGitHub: jest.fn() as any,
  loginWithGoogle: jest.fn() as any,
  register: jest.fn() as any,
  logout: jest.fn() as any,
  refreshToken: jest.fn() as any
};

const mockGetAccessToken = jest.fn() as any;
const mockClearAccessToken = jest.fn() as any;
const mockDecodeJwtToken = jest.fn() as any;

jest.mock('../../services/auth', () => {
  return {
    __esModule: true,
    authApi: mockAuthApi,
    getAccessToken: mockGetAccessToken,
    clearAccessToken: mockClearAccessToken
  };
});

jest.mock('../../utils/auth', () => {
  return {
    __esModule: true,
    decodeJwtToken: mockDecodeJwtToken
  };
});

async function flushEffects(): Promise<void> {
  await act(async () => {
    await Promise.resolve();
  });
}

describe('contexts/AuthContext', () => {
  let container: HTMLDivElement;
  let root: Root;

  beforeEach(() => {
    container = document.createElement('div');
    document.body.appendChild(container);
    root = createRoot(container);

    window.location.hash = '';

    mockAuthApi.login.mockReset();
    mockAuthApi.loginWithGitHub.mockReset();
    mockAuthApi.loginWithGoogle.mockReset();
    mockAuthApi.register.mockReset();
    mockAuthApi.logout.mockReset();
    mockAuthApi.refreshToken.mockReset();

    mockGetAccessToken.mockReset();
    mockClearAccessToken.mockReset();
    mockDecodeJwtToken.mockReset();
  });

  afterEach(() => {
    act(() => {
      root.unmount();
    });
    container.remove();
  });

  test('on mount: uses token to set user when decode succeeds', async () => {
    const tokenUser = { id: 1, userName: 'alice', email: 'a@a.com', roles: ['Admin'] };

    mockGetAccessToken.mockReturnValue('token-1');
    mockDecodeJwtToken.mockReturnValue(tokenUser);

    const { AuthProvider, AuthContext } = await import('../AuthContext');

    let observed: any;

    function Probe() {
      const ctx = useContext(AuthContext);
      useEffect(() => {
        observed = ctx;
      });
      return <div />;
    }

    act(() => {
      root.render(
        <AuthProvider>
          <Probe />
        </AuthProvider>
      );
    });

    await flushEffects();

    expect(observed?.isAuthenticated).toBe(true);
    expect(observed?.user).toEqual(tokenUser);
    expect(mockAuthApi.refreshToken).not.toHaveBeenCalled();
  });

  test('on mount: refreshes token when missing and then sets user', async () => {
    const tokenUser = { id: 2, userName: 'bob', email: 'b@b.com', roles: [] };

    mockGetAccessToken.mockReturnValueOnce(null).mockReturnValueOnce('token-2');
    mockDecodeJwtToken.mockReturnValue(tokenUser);
    mockAuthApi.refreshToken.mockResolvedValue({ token: 'token-2' });

    const { AuthProvider, AuthContext } = await import('../AuthContext');

    let observed: any;

    function Probe() {
      const ctx = useContext(AuthContext);
      useEffect(() => {
        observed = ctx;
      });
      return <div />;
    }

    act(() => {
      root.render(
        <AuthProvider>
          <Probe />
        </AuthProvider>
      );
    });

    await flushEffects();
    await flushEffects();

    expect(mockAuthApi.refreshToken).toHaveBeenCalledTimes(1);
    expect(observed?.isAuthenticated).toBe(true);
    expect(observed?.user).toEqual(tokenUser);
  });

  test('on mount: skips auth check during auth-flow routes', async () => {
    window.location.hash = '#/auth/callback';

    const { AuthProvider } = await import('../AuthContext');

    act(() => {
      root.render(<AuthProvider>child</AuthProvider>);
    });

    await flushEffects();

    expect(mockAuthApi.refreshToken).not.toHaveBeenCalled();
  });

  test('login: calls authApi.login and sets user from decoded token when available', async () => {
    const tokenUser = { id: 3, userName: 'cat', email: 'c@c.com', roles: [] };

    mockGetAccessToken.mockReturnValue('token-3');
    mockDecodeJwtToken.mockReturnValue(tokenUser);

    mockAuthApi.refreshToken.mockResolvedValue({ token: 'token-3' });
    mockAuthApi.login.mockResolvedValue({
      token: 'token-3',
      user: { id: 999, userName: 'server', email: 's@s.com', roles: [] }
    });

    const { AuthProvider, AuthContext } = await import('../AuthContext');

    let ctx: any;

    function Probe() {
      const value = useContext(AuthContext);
      useEffect(() => {
        ctx = value;
      }, [value]);
      return <div />;
    }

    act(() => {
      root.render(
        <AuthProvider>
          <Probe />
        </AuthProvider>
      );
    });

    await flushEffects();

    await act(async () => {
      await ctx.login({ username: 'cat', password: 'pw' });
    });

    expect(mockAuthApi.login).toHaveBeenCalledWith({ username: 'cat', password: 'pw' });
    expect(ctx.user).toEqual(tokenUser);
    expect(ctx.isAuthenticated).toBe(true);
  });

  test('logout: clears token and clears user even if logout endpoint fails', async () => {
    const errorSpy = jest.spyOn(console, 'error').mockImplementation(() => undefined);

    mockAuthApi.logout.mockRejectedValue(new Error('nope'));
    mockGetAccessToken.mockReturnValue('token-4');

    const { AuthProvider, AuthContext } = await import('../AuthContext');

    let ctx: any;

    function Probe() {
      const value = useContext(AuthContext);
      useEffect(() => {
        ctx = value;
      }, [value]);
      return <div />;
    }

    act(() => {
      root.render(
        <AuthProvider>
          <Probe />
        </AuthProvider>
      );
    });

    await flushEffects();

    await act(async () => {
      await ctx.logout();
    });

    expect(mockClearAccessToken).toHaveBeenCalledTimes(1);
    expect(ctx.user).toBeNull();
    expect(ctx.isAuthenticated).toBe(false);

    errorSpy.mockRestore();
  });
});
