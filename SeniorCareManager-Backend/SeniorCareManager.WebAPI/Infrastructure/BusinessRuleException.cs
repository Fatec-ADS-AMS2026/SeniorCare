namespace SeniorCareManager.WebAPI.Infrastructure;

// Regra de negócio violada por um payload sintaticamente válido (ex.: referência
// a um id de FK inexistente) — mapeada para 422, distinta de erro de validação de
// forma (400, tratado pelo ModelState do [ApiController]) e de recurso ausente (404).
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
