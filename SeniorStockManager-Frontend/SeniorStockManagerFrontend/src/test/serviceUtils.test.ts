import { describe, it, expect, vi, beforeEach } from 'vitest';
import generateGenericMethods, {
  handleServiceError,
} from '@/utils/serviceUtils';

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
    it('returns success result with items from PagedResult on 200', async () => {
      // O backend nunca devolve um array cru — sempre {items,page,pageSize,totalCount}
      // (§3b/§9, PagedResult<T>).
      const pagedResult = {
        items: [{ id: 1, name: 'Medicamento' }],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      };
      mockApi.get.mockResolvedValueOnce({ data: pagedResult });

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
    it('returns success with the raw entity when found', async () => {
      // Recurso individual vem sem envelope — o próprio DTO (design.md decisão 3).
      const entity = { id: 3, name: 'Higiene' };
      mockApi.get.mockResolvedValueOnce({ data: entity });

      const result = await methods.getById(3);

      expect(result.success).toBe(true);
      expect(result.data?.name).toBe('Higiene');
    });
  });

  describe('create', () => {
    it('returns success after POST with the created entity', async () => {
      const entity = { id: 0, name: 'Novo Tipo' };
      const created = { id: 5, name: 'Novo Tipo' };
      mockApi.post.mockResolvedValueOnce({ data: created });

      const result = await methods.create(entity);

      expect(result.success).toBe(true);
      expect(result.data?.id).toBe(5);
    });
  });

  describe('update', () => {
    it('returns success after PUT with the updated entity', async () => {
      const entity = { id: 1, name: 'Atualizado' };
      mockApi.put.mockResolvedValueOnce({ data: entity });

      const result = await methods.update(1, entity);

      expect(result.success).toBe(true);
      expect(result.data?.name).toBe('Atualizado');
    });
  });

  describe('deleteById', () => {
    it('returns success: true on 204 (sem corpo)', async () => {
      mockApi.delete.mockResolvedValueOnce({ data: undefined });

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
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Não autorizado');
  });

  it('returns a friendly message for 409 (conflito de concorrência)', () => {
    // Formato real que o GlobalExceptionHandler manda pra DbUpdateConcurrencyException.
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 409,
        data: {
          type: 'https://seniorcare.dev/erros/conflito-concorrencia',
          title: 'O recurso foi modificado por outra requisição desde a última leitura.',
          status: 409,
          detail: 'Releia o recurso (GET) para obter a versão atual antes de tentar novamente.',
        },
      },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe(
      'Releia o recurso (GET) para obter a versão atual antes de tentar novamente.'
    );
  });

  it('maps ValidationProblemDetails.errors (dict) to FieldError[] on 400', () => {
    // Formato real que a validação automática de model do [ApiController] gera.
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 400,
        data: {
          type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
          title: 'One or more validation errors occurred.',
          status: 400,
          errors: { Description: ['The Description field is required.'] },
        },
      },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('One or more validation errors occurred.');
    expect(result.errors).toEqual([
      { field: 'Description', message: 'The Description field is required.' },
    ]);
  });

  it('returns Detail as message for other Problem Details statuses (ex.: 422)', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 422,
        data: {
          type: 'https://seniorcare.dev/erros/regra-de-negocio',
          title: 'Regra de negócio violada.',
          status: 422,
          detail: 'ProductTypeId 99999 não referencia um tipo de produto ativo.',
        },
      },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe(
      'ProductTypeId 99999 não referencia um tipo de produto ativo.'
    );
  });
});
