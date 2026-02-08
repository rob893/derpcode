import { jest } from '@jest/globals';

type AnyFunction = (...args: any[]) => any;

function mockFn(): jest.MockedFunction<AnyFunction> {
  return jest.fn() as unknown as jest.MockedFunction<AnyFunction>;
}

const mockApiClient = {
  get: mockFn(),
  patch: mockFn()
};

jest.mock('../axiosConfig', () => ({
  __esModule: true,
  default: mockApiClient
}));

describe('services/userPreferences', () => {
  beforeEach(() => {
    jest.resetModules();
    mockApiClient.get.mockReset();
    mockApiClient.patch.mockReset();
  });

  test('getUserPreferences calls the correct endpoint', async () => {
    const { userPreferencesApi } = await import('../userPreferences');

    mockApiClient.get.mockResolvedValue({ data: { id: 1 } });

    await userPreferencesApi.getUserPreferences(7);

    expect(mockApiClient.get).toHaveBeenCalledWith('/api/v1/users/7/preferences');
  });

  test('patchUserPreferences uses json-patch content type', async () => {
    const { userPreferencesApi } = await import('../userPreferences');

    mockApiClient.patch.mockResolvedValue({ data: { id: 1 } });

    const patchDocument = [{ op: 'replace', path: '/preferences/uiPreference/pageSize', value: 10 }];

    await userPreferencesApi.patchUserPreferences(7, 42, patchDocument as any);

    expect(mockApiClient.patch).toHaveBeenCalledWith('/api/v1/users/7/preferences/42', patchDocument, {
      headers: {
        'Content-Type': 'application/json-patch+json'
      }
    });
  });
});
