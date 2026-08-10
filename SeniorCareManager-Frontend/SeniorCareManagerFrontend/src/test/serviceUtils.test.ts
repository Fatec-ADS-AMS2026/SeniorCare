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
  const methods = generateGenericMethods<TestModel>('religion');

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getAll', () => {
    it('returns success result with items from PagedResult on 200', async () => {
      // O backend nunca devolve um array cru — sempre {items,page,pageSize,totalCount}
      // (§3b/§9, PagedResult<T>).
      const pagedResult = {
        items: [{ id: 1, name: 'Católica' }],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      };
      mockApi.get.mockResolvedValueOnce({ data: pagedResult });

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
    it('returns success with the raw entity when found', async () => {
      // Recurso individual vem sem envelope — o próprio DTO (design.md decisão 3).
      const entity = { id: 2, name: 'Budista' };
      mockApi.get.mockResolvedValueOnce({ data: entity });

      const result = await methods.getById(2);

      expect(result.success).toBe(true);
      expect(result.data?.name).toBe('Budista');
    });
  });

  describe('create', () => {
    it('returns success after POST with the created entity', async () => {
      const entity = { id: 0, name: 'Nova' };
      const created = { id: 10, name: 'Nova' };
      mockApi.post.mockResolvedValueOnce({ data: created });

      const result = await methods.create(entity);

      expect(result.success).toBe(true);
      expect(result.data?.id).toBe(10);
    });
  });

  describe('update', () => {
    it('returns success after PUT with the updated entity', async () => {
      const entity = { id: 1, name: 'Atualizado' };
      mockApi.put.mockResolvedValueOnce({ data: entity });

      const result = await methods.update(1, entity);

      expect(result.success).toBe(true);
    });
  });

  describe('deleteById', () => {
    it('returns success: true on 204 (sem corpo)', async () => {
      mockApi.delete.mockResolvedValueOnce({ data: undefined });

      const result = await methods.deleteById(1);

      expect(result.success).toBe(true);
    });

    it('returns failure result on error', async () => {
      const axiosError = {
        isAxiosError: true,
        response: {
          data: {
            type: 'https://seniorcare.dev/erros/nao-encontrado',
            title: 'Recurso não encontrado.',
            status: 404,
            detail: 'Registro não encontrado.',
          },
          status: 404,
        },
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

  it('returns "Não autorizado" for a bodyless 401 (cookie de sessão ausente/expirado)', () => {
    const axiosError = {
      isAxiosError: true,
      response: { data: {}, status: 401 },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Não autorizado');
  });

  it('returns the real message for a 401 with body (AuthController.Login "Credenciais inválidas.")', () => {
    // AuthController devolve Unauthorized(new MessageResponse{...}) direto — não passa
    // pelo GlobalExceptionHandler, então não é Problem Details, mas tem corpo real.
    const axiosError = {
      isAxiosError: true,
      response: { status: 401, data: { message: 'Credenciais inválidas.' } },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Credenciais inválidas.');
  });

  it('returns the real message for a 429 (limitador de origem)', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 429,
        data: { message: 'Muitas tentativas. Tente novamente mais tarde.' },
      },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe(
      'Muitas tentativas. Tente novamente mais tarde.'
    );
  });

  it('returns a friendly message for 409 (conflito de concorrência)', () => {
    // Formato real que o GlobalExceptionHandler manda pra DbUpdateConcurrencyException.
    const axiosError = {
      isAxiosError: true,
      response: {
        status: 409,
        data: {
          type: 'https://seniorcare.dev/erros/conflito-concorrencia',
          title:
            'O recurso foi modificado por outra requisição desde a última leitura.',
          status: 409,
          detail:
            'Releia o recurso (GET) para obter a versão atual antes de tentar novamente.',
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
          errors: { Name: ['The Name field is required.'] },
        },
      },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('One or more validation errors occurred.');
    expect(result.errors).toEqual([
      { field: 'Name', message: 'The Name field is required.' },
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
          detail: 'Nome já cadastrado.',
        },
      },
    };

    const result = handleServiceError(axiosError);

    expect(result.success).toBe(false);
    expect(result.message).toBe('Nome já cadastrado.');
  });
});
