namespace SeniorCareManager.WebAPI.Services.Interfaces;

// Limite por origem (IP), em memória — não sobrevive a reinício nem escala a múltiplas
// réplicas (limite conhecido, aceitável: este projeto não roda a API atrás de mais de uma
// instância por cliente). Complementa o bloqueio por conta (nativo do Identity).
public interface IOriginRateLimiter
{
    bool IsBlocked(string origin);

    void RecordFailure(string origin);
}
