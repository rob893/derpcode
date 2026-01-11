import { jest } from '@jest/globals';

type AnyFunction = (...args: any[]) => any;

function mockFn(): jest.MockedFunction<AnyFunction> {
  return jest.fn() as unknown as jest.MockedFunction<AnyFunction>;
}

const mockApiClient = {
  get: mockFn(),
  post: mockFn(),
  put: mockFn(),
  delete: mockFn()
};

jest.mock('../axiosConfig', () => ({
  __esModule: true,
  default: mockApiClient
}));

describe('services/user', () => {
  beforeEach(() => {
    jest.resetModules();
    mockApiClient.get.mockReset();
    mockApiClient.post.mockReset();
    mockApiClient.put.mockReset();
    mockApiClient.delete.mockReset();
  });

  test('userApi.getUserById calls correct endpoint and returns dto', async () => {
    const { userApi } = await import('../user');

    mockApiClient.get.mockResolvedValue({
      data: {
        id: 123,
        userName: 'bob',
        email: 'bob@example.com',
        emailConfirmed: true,
        created: '2025-01-01T00:00:00Z',
        roles: ['Admin'],
        linkedAccounts: [],
        lastPasswordChange: '2025-01-01T00:00:00Z',
        lastEmailChange: '2025-01-01T00:00:00Z',
        lastUsernameChange: '2025-01-01T00:00:00Z'
      }
    });

    const dto = await userApi.getUserById(123);

    expect(mockApiClient.get).toHaveBeenCalledWith('/api/v1/users/123');
    expect(dto.id).toBe(123);
    expect(dto.userName).toBe('bob');
  });

  test('userApi.updateUsername calls correct endpoint and returns updated dto', async () => {
    const { userApi } = await import('../user');

    mockApiClient.put.mockResolvedValue({
      data: {
        id: 1,
        userName: 'newname',
        email: 'x@example.com',
        emailConfirmed: false,
        created: '2025-01-01T00:00:00Z',
        roles: [],
        linkedAccounts: [],
        lastPasswordChange: '2025-01-01T00:00:00Z',
        lastEmailChange: '2025-01-01T00:00:00Z',
        lastUsernameChange: '2025-01-01T00:00:00Z'
      }
    });

    const dto = await userApi.updateUsername(1, { newUsername: 'newname' });

    expect(mockApiClient.put).toHaveBeenCalledWith('/api/v1/users/1/username', { newUsername: 'newname' });
    expect(dto.userName).toBe('newname');
  });

  test('userApi.resendEmailConfirmation posts to correct endpoint', async () => {
    const { userApi } = await import('../user');

    mockApiClient.post.mockResolvedValue({ data: null });

    await userApi.resendEmailConfirmation(9);

    expect(mockApiClient.post).toHaveBeenCalledWith('/api/v1/users/9/emailConfirmations');
  });

  test('userApi.deleteLinkedAccount calls correct endpoint', async () => {
    const { userApi } = await import('../user');

    mockApiClient.delete.mockResolvedValue({ data: null });

    await userApi.deleteLinkedAccount(3, 'GitHub');

    expect(mockApiClient.delete).toHaveBeenCalledWith('/api/v1/users/3/linkedAccounts/GitHub');
  });
});
