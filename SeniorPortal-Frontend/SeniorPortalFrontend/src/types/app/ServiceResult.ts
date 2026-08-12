import { FieldError } from '@/features/api/types';

export default interface ServiceResult<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: FieldError[];
  // §5.3 — identificador de correlação do ProblemDetails (RFC 7807), quando o
  // backend o fornece, pra exibir junto de uma falha recuperável.
  correlationId?: string;
}
