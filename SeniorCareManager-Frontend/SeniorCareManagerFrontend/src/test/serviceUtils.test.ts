import { describe, it, expect, vi, beforeEach } from 'vitest';
import generateGenericMethods, { handleServiceError } from '@/utils/serviceUtils';

// Mock the api module so no real HTTP requests are made
vi.mock('@/features/api', () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  },
}));

import { api } from '@/features/api';

const mockApi = api as unknown as {
  get: ReturnType<typeof vi.fn>;
  post: ReturnType<typeof vi.fn>;
  put: ReturnType<typeof vi.fn>;
  delete: ReturnType<typeof vi.fn>;
};

interface TestModel {
  id: number;
  name: string;
}

describe('generateGenericMethods', () => {
  const methods = generateGenericMethods<TestModel>('religion');

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getAll', () => {
    it('returns success result with data array on 200', async () => {
      const payload = { success: true, message: 'OK', data: [{ id: 1, name: 'Católica' }] };
      mockApi.get.mockResolvedValueOnce({ data: payload });

      const result = await methods.getAll();

      expect(result.success).toBe(true);
      expect(result.data).toHaveLength(1);
      expect(result.data![0].name).toBe('Católica');
    });

    it('returns failure result on network error', async () => {
      mockApi.get.mockRejectedValueOnce(new Error('Network Error'));

      const result = await methods.getAll();

      expect(result.success).toBe(false);
      expect(result.message).toBe('Erro desconhecido');
    });
  });

  describe('getById', () => {
    it('returns success result with single entity on 200', async () => {
      const payload = { success: true, message: 'OK', data: { id: 2, name: 'Budista' } };
      mockApi.get.mockResolvedValueOnce({ data: payload });

      const result = await methods.getById(2);

      expect(result.success).toBe(true);
      expect(result.data?.name).toBe('Budista');
    });
  });

  describe('create', () => {
    it('returns success result after POST', async () => {
      const entity = { id: 0, name: 'Nova' };
      const payload = { success: true, message: 'Criado', data: { id: 10, name: 'Nova' } };
      mockApi.post.mockResolvedValueOnce({ data: payload });

      const result = await methods.create(entity);

      expect(result.success).toBe(true);
      expect(result.data?.id).toBe(10);
    });
  });

  describe('update', () => {
    it('returns success result after PUT', async () => {
      const entity = { id: 1, name: 'Atualizado' };
      const payload = { success: true, message: 'Atualizado', data: entity };
      mockApi.put.mockResolvedValueOnce({ data: payload });

      const result = await methods.update(1, entity);

      expect(result.success).toBe(true);
    });
  });

  describe('deleteById', () => {
    it('returns success: true on 200', async () => {
      mockApi.delete.mockResolvedValueOnce({ data: {} });

      const result = await methods.deleteById(1);

      expect(result.success).toBe(true);
    });

    it('returns failure result on error', async () => {
      const axiosError = {
        isAxiosError: true,
        response: { data: { message: 'Não encontrado', errors: [] }, status: 404 },
        status: 404,
      };
      mockApi.delete.mockRejectedValueOnce(axiosError);

      const result = await methods.deleteById(99);

      expect(result.success).toBe(false);
    });
  });
});

describe('handleServiceError', () => {
  it('returns generic error message for non-axios errors', () => {
    const result = handleServiceError(new Error('any'));

    expect(result.success).toBe(false);
    expect(result.message).toBe('Erro desconhecido');
  });

  it('returns "Não autorizado" for 401 status', () => {
    const axiosError = {
      isAxiosError: true,
      response: { data: {}, status: 401 },
      status: 401,
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Não autorizado');
  });

  it('returns validation message when errors array is present', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {
          message: 'Erro de validação',
          errors: [{ field: 'name', message: 'obrigatório' }],
        },
        status: 422,
      },
      status: 422,
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Erro de validação');
    expect(result.errors).toHaveLength(1);
  });
});
