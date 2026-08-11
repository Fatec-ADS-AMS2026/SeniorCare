using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Controllers;
using SeniorCareManager.WebAPI.Infrastructure;

namespace SeniorCareManager.UnitTests;

/// <summary>
/// §13.3 (aceite da mudança): prova, por reflection sobre o assembly real da API — não
/// por amostragem de rota — as duas garantias que a spec `automated-quality-gates` e a
/// tarefa de aceite exigem: nenhum endpoint administrativo aceita acesso anônimo, e toda
/// escrita exige permissão efetiva. Complementa (não substitui)
/// `EndpointAuthorizationTests`/os `Admin*ControllerTests` de integração, que provam o
/// comportamento HTTP real de uma amostra; estes testes provam o MECANISMO em si, em
/// todo controller do assembly, sem precisar de banco/HTTP — rápidos e determinísticos.
///
/// A checagem por reflection é possível porque a autorização desta API é 100% declarativa
/// (atributos), sem lógica condicional escondida em código imperativo — se um atributo
/// está ausente, o comportamento em runtime também está ausente.
/// </summary>
public class EndpointAuthorizationInvariantsTests
{
    private static IEnumerable<(Type Controller, MethodInfo Action)> AllControllerActions()
    {
        var controllerTypes = typeof(AuthController).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t));

        foreach (var controllerType in controllerTypes)
        {
            foreach (var method in controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // Só métodos de ação de verdade (têm um atributo HTTP verb) — ignora
                // helpers públicos como o `ToDto` que alguns controllers expõem.
                var hasHttpVerb = method.GetCustomAttributes()
                    .Any(a => a is HttpGetAttribute or HttpPostAttribute or HttpPutAttribute
                        or HttpDeleteAttribute or HttpPatchAttribute);
                if (hasHttpVerb)
                    yield return (controllerType, method);
            }
        }
    }

    [Fact]
    public void NenhumaAcaoAnonimaExisteForaDoAuthController()
    {
        // O único lugar do sistema onde faz sentido não exigir sessão é o próprio
        // bootstrap de identidade (login, MFA, ativação, recuperação, troca de senha,
        // logout) — tudo isso vive em AuthController. Nenhum outro controller,
        // principalmente nenhum Admin*, deve ter [AllowAnonymous] em lugar nenhum.
        var anonymousActions = AllControllerActions()
            .Where(x => x.Action.GetCustomAttribute<AllowAnonymousAttribute>() != null)
            .ToList();

        anonymousActions
            .Select(x => x.Controller)
            .Distinct()
            .Should().Equal(new[] { typeof(AuthController) },
                "só AuthController deve ter ações [AllowAnonymous] — qualquer outro controller " +
                "com uma ação anônima é um endpoint administrativo/de negócio acessível sem sessão");

        // 8 endpoints de identidade hoje: login, login/mfa, mfa/enroll, mfa/confirm,
        // activate, recover, reset-password, change-password. Uma mudança nesse número
        // deve ser uma decisão consciente (atualizar este teste), não um efeito colateral.
        anonymousActions.Should().HaveCount(8,
            "o conjunto de endpoints de identidade sem sessão é conhecido e estável — se " +
            "esse número mudou, confirme que é intencional antes de ajustar este teste");
    }

    [Fact]
    public void TodaAcaoDeEscritaExigePermissaoEfetiva()
    {
        // "Escrita" = POST/PUT/DELETE/PATCH. Os únicos POSTs sem RequirePermission
        // aceitáveis são os de identidade do AuthController (login, ativação etc. — não
        // fazem sentido gateados por uma permissão de recurso, são o que CONCEDE sessão).
        var writeActions = AllControllerActions()
            .Where(x => x.Action.GetCustomAttributes()
                .Any(a => a is HttpPostAttribute or HttpPutAttribute or HttpDeleteAttribute or HttpPatchAttribute))
            .Where(x => x.Controller != typeof(AuthController))
            .ToList();

        writeActions.Should().NotBeEmpty("deveria haver ações de escrita nos controllers de catálogo/administração");

        var withoutPermission = writeActions
            .Where(x => x.Action.GetCustomAttribute<RequirePermissionAttribute>() == null)
            .Select(x => $"{x.Controller.Name}.{x.Action.Name}")
            .ToList();

        withoutPermission.Should().BeEmpty(
            "toda ação de escrita fora do AuthController deve exigir uma permissão efetiva via [RequirePermission]");
    }
}
