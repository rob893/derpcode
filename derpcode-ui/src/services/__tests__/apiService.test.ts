import { jest } from '@jest/globals';
import { OrderByDirection, ProblemOrderBy } from '../../types/models';

type AnyFunction = (...args: any[]) => any;

function mockFn(): jest.MockedFunction<AnyFunction> {
  return jest.fn() as unknown as jest.MockedFunction<AnyFunction>;
}

const mockApiClient = {
  get: mockFn(),
  post: mockFn(),
  patch: mockFn()
};

jest.mock('../axiosConfig', () => ({
  __esModule: true,
  default: mockApiClient
}));

describe('services/api', () => {
  beforeEach(() => {
    jest.resetModules();
    mockApiClient.get.mockReset();
    mockApiClient.post.mockReset();
    mockApiClient.patch.mockReset();
  });

  test('problemsApi.getProblems builds querystring for cursor + filters', async () => {
    const { problemsApi } = await import('../api');

    mockApiClient.get.mockResolvedValue({ data: { nodes: [], pageInfo: {} } });

    await problemsApi.getProblems({
      first: 10,
      after: 'cursor-1',
      includeUnpublished: true,
      searchTerm: '  hello  ',
      difficulties: [1, 3],
      tags: ['arrays', 'dp'],
      orderBy: ProblemOrderBy.Name,
      orderByDirection: OrderByDirection.Descending
    } as any);

    expect(mockApiClient.get).toHaveBeenCalledWith(
      '/api/v1/problems?first=10&after=cursor-1&includeUnpublished=true&searchTerm=hello&difficulties=1&difficulties=3&tags=arrays&tags=dp&orderBy=Name&orderByDirection=Descending'
    );
  });

  test('problemsApi.patchProblem uses json-patch content type', async () => {
    const { problemsApi } = await import('../api');

    mockApiClient.patch.mockResolvedValue({ data: { id: 1 } });

    const patchDocument = [{ op: 'replace', path: '/name', value: 'new' }];

    await problemsApi.patchProblem(12, patchDocument as any);

    expect(mockApiClient.patch).toHaveBeenCalledWith('/api/v1/problems/12', patchDocument, {
      headers: {
        'Content-Type': 'application/json-patch+json'
      }
    });
  });

  test('driverTemplatesApi.getDriverTemplates returns nodes when present', async () => {
    const { driverTemplatesApi } = await import('../api');

    mockApiClient.get.mockResolvedValue({ data: { nodes: [{ id: 1 }, { id: 2 }], edges: [] } });

    const templates = await driverTemplatesApi.getDriverTemplates();

    expect(mockApiClient.get).toHaveBeenCalledWith('/api/v1/driverTemplates');
    expect(templates).toHaveLength(2);
    expect(templates[0].id).toBe(1);
  });

  test('driverTemplatesApi.getDriverTemplates falls back to edges', async () => {
    const { driverTemplatesApi } = await import('../api');

    mockApiClient.get.mockResolvedValue({ data: { nodes: null, edges: [{ node: { id: 7 } }] } });

    const templates = await driverTemplatesApi.getDriverTemplates();

    expect(templates).toEqual([{ id: 7 }]);
  });
});
