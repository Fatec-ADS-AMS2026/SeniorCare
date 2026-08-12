import { FieldError } from '@/features/api/types';
import ServiceResult from '@/types/app/ServiceResult';
import { isAxiosError } from 'axios';

// Formato real devolvido pelo backend (RFC 7807) — tanto pelo
// GlobalExceptionHandler (Detail) quanto pela validação automática de model do
// [ApiController] (Errors, um dicionário campo→mensagens). Nem todo erro do
// backend é Problem Details — AuthController devolve alguns 401/429 explícitos
// como MessageResponse{Message} (ex.: "Credenciais inválidas."), sem passar pelo
// GlobalExceptionHandler. `message` cobre esse formato.
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
