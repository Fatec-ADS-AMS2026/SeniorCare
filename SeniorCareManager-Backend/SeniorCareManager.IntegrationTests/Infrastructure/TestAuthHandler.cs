using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SeniorCareManager.IntegrationTests.Infrastructure;

/// <summary>
/// Esquema de autenticação só de teste: monta um ClaimsPrincipal a partir do header
/// <see cref="UserIdHeader"/>, sem senha/cookie/token real. Usado via
/// <c>HttpClient.AsUser(userId)</c>. Registrado como esquema adicional (não padrão) — uma
/// requisição sem o header continua anônima, exatamente como em produção.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userIdValues.ToString()) };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
