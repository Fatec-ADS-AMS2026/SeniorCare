using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SeniorCareManager.WebAPI.Infrastructure;

// Único ponto que converte exceção não tratada em resposta HTTP — garante que
// nenhum controller precise vazar ex.Message/stack trace no corpo da resposta.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, type, detail) = MapException(exception);

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro não tratado ao processar {Method} {Path}",
                Sanitize(httpContext.Request.Method),
                Sanitize(httpContext.Request.Path.Value));
        }

        httpContext.Response.StatusCode = status;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Type = type,
                Detail = detail,
            },
        });
    }

    // Method/Path vêm da requisição (logo, do cliente) — sem isso, um path com
    // \r\n poderia forjar entradas de log falsas (CWE-117 log injection).
    private static string Sanitize(string? value) =>
        (value ?? string.Empty).Replace('\r', '_').Replace('\n', '_');

    // Detail só carrega a mensagem da exceção quando ela é um tipo de domínio que
    // nós mesmos lançamos com texto pensado para o cliente (KeyNotFoundException,
    // BusinessRuleException) — nunca a mensagem de uma exceção genérica/de terceiros.
    private static (int Status, string Title, string Type, string? Detail) MapException(Exception exception) => exception switch
    {
        KeyNotFoundException => (
            StatusCodes.Status404NotFound,
            "Recurso não encontrado.",
            "https://seniorcare.dev/erros/nao-encontrado",
            exception.Message),
        BusinessRuleException => (
            StatusCodes.Status422UnprocessableEntity,
            "Regra de negócio violada.",
            "https://seniorcare.dev/erros/regra-de-negocio",
            exception.Message),
        AccessDeniedException => (
            StatusCodes.Status403Forbidden,
            "Acesso negado.",
            "https://seniorcare.dev/erros/acesso-negado",
            "A identidade autenticada não tem permissão efetiva para esta operação."),
        DbUpdateConcurrencyException => (
            StatusCodes.Status409Conflict,
            "O recurso foi modificado por outra requisição desde a última leitura.",
            "https://seniorcare.dev/erros/conflito-concorrencia",
            "Releia o recurso (GET) para obter a versão atual antes de tentar novamente."),
        _ => (
            StatusCodes.Status500InternalServerError,
            "Ocorreu um erro inesperado ao processar a requisição.",
            "https://seniorcare.dev/erros/interno",
            null),
    };
}
