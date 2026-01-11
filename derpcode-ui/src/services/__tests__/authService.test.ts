import { jest } from '@jest/globals';

type AnyFunction = (...args: any[]) => any;

function mockFn(): jest.MockedFunction<AnyFunction> {
  return jest.fn() as unknown as jest.MockedFunction<AnyFunction>;
}

const mockApiClient = {
  post: mockFn()
};

jest.mock('../axiosConfig', () => ({
  __esModule: true,
  default: mockApiClient
}));

describe('services/auth', () => {
  beforeEach(() => {
    jest.resetModules();
    localStorage.clear();
    mockApiClient.post.mockReset();
  });

  test('getDeviceId returns existing device id from localStorage', async () => {
    localStorage.setItem('device_id', 'device-123');

    const { getDeviceId } = await import('../auth');

    expect(getDeviceId()).toBe('device-123');
  });

  test('getDeviceId creates and stores device id when missing', async () => {
    const { getDeviceId } = await import('../auth');

    const id = getDeviceId();

    expect(id).toBeTruthy();
    expect(localStorage.getItem('device_id')).toBe(id);

    // should be stable across calls (cached/localStorage)
    expect(getDeviceId()).toBe(id);
  });

  test('authApi.login posts to login endpoint and sets access token', async () => {
    localStorage.setItem('device_id', 'device-abc');

    const { authApi, getAccessToken } = await import('../auth');

    mockApiClient.post.mockResolvedValue({
      data: {
        token: 'token-1',
        user: { id: 1, userName: 'alice', email: 'alice@example.com', roles: [] }
      }
    });

    const result = await authApi.login({ username: 'alice', password: 'pw' });

    expect(mockApiClient.post).toHaveBeenCalledWith('/api/v1/auth/login', {
      username: 'alice',
      password: 'pw',
      deviceId: 'device-abc'
    });

    expect(result.token).toBe('token-1');
    expect(getAccessToken()).toBe('token-1');
  });

  test('authApi.logout clears access token even if endpoint fails', async () => {
    const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => undefined);

    const { authApi, setAccessToken, getAccessToken } = await import('../auth');

    setAccessToken('token-x');
    mockApiClient.post.mockRejectedValue(new Error('nope'));

    await authApi.logout();

    expect(getAccessToken()).toBeNull();
    warnSpy.mockRestore();
  });
});
