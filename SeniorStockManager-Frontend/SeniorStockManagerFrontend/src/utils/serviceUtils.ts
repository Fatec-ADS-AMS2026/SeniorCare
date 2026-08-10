import { api } from '@/features/api';
import { FieldError } from '@/features/api/types';
import ServiceResult from '@/types/app/ServiceResult';
import { isAxiosError } from 'axios';

// Envelope de listagem paginada que os 10 catálogos (§3b/§9) realmente devolvem —
// {items,page,pageSize,totalCount} — nunca um array cru.
interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export default function generateGenericMethods<T extends { id: number }>(
  modelName: string
) {
  const modelEndpoint = `${modelName}/`;

  const getAll = async (): Promise<ServiceResult<T[]>> => {
    try {
      const res = await api.get<PagedResult<T>>(modelEndpoint);
      return { success: true, message: '', data: res.data.items };
    } catch (error) {
      return handleServiceError(error);
    }
  };

  const getById = async (id: number): Promise<ServiceResult<T>> => {
    try {
      const res = await api.get<T>(modelEndpoint + id);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  };

  const create = async (model: T): Promise<ServiceResult<T>> => {
    try {
      const res = await api.post<T>(modelEndpoint, model);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  };

  const update = async (id: number, model: T): Promise<ServiceResult<T>> => {
    try {
      const res = await api.put<T>(modelEndpoint + id, model);
      return { success: true, message: '', data: res.data };
    } catch (error) {
      return handleServiceError(error);
    }
  };

  const deleteById = async (id: number): Promise<ServiceResult<undefined>> => {
    try {
      await api.delete(modelEndpoint + id);
      return { success: true, message: 'Excluído com sucesso' };
    } catch (error) {
      return handleServiceError(error);
    }
  };

  return {
    getAll,
    getById,
    create,
    update,
    deleteById,
  };
}

// Formato real devolvido pelo backend (RFC 7807) desde a §3a — tanto pelo
// GlobalExceptionHandler (Detail) quanto pela validação automática de model do
// [ApiController] (Errors, um dicionário campo→mensagens). §10: nem todo erro do
// backend é Problem Details — AuthController devolve alguns 401/429 explícitos como
// MessageResponse{Message} (ex.: "Credenciais inválidas.", limitador de origem), sem
// passar pelo GlobalExceptionHandler. `message` cobre esse formato.
interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  correlationId?: string;
  message?: string;
}

export function handleServiceError<T>(error: unknown): ServiceResult<T> {
  if (!isAxiosError(error) || !error.response) {
    return {
      success: false,
      message: 'Erro desconhecido',
    };
  }

  const status = error.response.status;
  const problem = (error.response.data ?? {}) as ProblemDetails;

  // 401 do middleware de cookie (sessão ausente/expirada) não tem corpo — só nesse
  // caso usa a mensagem genérica. 401 explícito de AuthController.Login/LoginMfa
  // ("Credenciais inválidas.") tem corpo e cai no retorno final, abaixo.
  if (status === 401 && !problem.message) {
    return {
      success: false,
      message: 'Não autorizado',
    };
  }

  // Conflito de concorrência otimista (RowVersion desatualizado) — o
  // GlobalExceptionHandler já manda uma mensagem pronta em Detail.
  if (status === 409) {
    return {
      success: false,
      message:
        problem.detail ||
        'O recurso foi modificado por outra requisição. Recarregue e tente novamente.',
    };
  }

  if (problem.errors) {
    const errors: FieldError[] = Object.entries(problem.errors).flatMap(
      ([field, messages]) => messages.map((message) => ({ field, message }))
    );
    return {
      success: false,
      message: problem.title || 'Erro de validação',
      errors,
    };
  }

  return {
    success: false,
    message: problem.detail || problem.title || problem.message || 'Erro desconhecido',
  };
}
