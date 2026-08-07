using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
                httpContext.Request.Method,
                httpContext.Request.Path);
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
        _ => (
            StatusCodes.Status500InternalServerError,
            "Ocorreu um erro inesperado ao processar a requisição.",
            "https://seniorcare.dev/erros/interno",
            null),
    };
}
