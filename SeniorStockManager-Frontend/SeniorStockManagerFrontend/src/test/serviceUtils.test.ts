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
  const methods = generateGenericMethods<TestModel>('producttype');

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getAll', () => {
    it('returns success result with data array on 200', async () => {
      const payload = {
        success: true,
        message: 'OK',
        data: [{ id: 1, name: 'Medicamento' }],
      };
      mockApi.get.mockResolvedValueOnce({ data: payload });

      const result = await methods.getAll();

      expect(result.success).toBe(true);
      expect(result.data).toHaveLength(1);
      expect(result.data![0].name).toBe('Medicamento');
    });

    it('returns failure result on network error', async () => {
      mockApi.get.mockRejectedValueOnce(new Error('Network Error'));

      const result = await methods.getAll();

      expect(result.success).toBe(false);
      expect(result.message).toBe('Erro desconhecido');
    });
  });

  describe('getById', () => {
    it('returns success with entity when found', async () => {
      const payload = { success: true, message: 'OK', data: { id: 3, name: 'Higiene' } };
      mockApi.get.mockResolvedValueOnce({ data: payload });

      const result = await methods.getById(3);

      expect(result.success).toBe(true);
      expect(result.data?.name).toBe('Higiene');
    });
  });

  describe('create', () => {
    it('returns success after POST', async () => {
      const entity = { id: 0, name: 'Novo Tipo' };
      const payload = { success: true, message: 'Criado', data: { id: 5, name: 'Novo Tipo' } };
      mockApi.post.mockResolvedValueOnce({ data: payload });

      const result = await methods.create(entity);

      expect(result.success).toBe(true);
      expect(result.data?.id).toBe(5);
    });
  });

  describe('update', () => {
    it('returns success after PUT', async () => {
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

  it('returns validation errors when errors array is present', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {
          message: 'Dado inválido',
          errors: [{ field: 'name', message: 'obrigatório' }],
        },
        status: 422,
      },
      status: 422,
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Dado inválido');
    expect(result.errors).toHaveLength(1);
  });
});
