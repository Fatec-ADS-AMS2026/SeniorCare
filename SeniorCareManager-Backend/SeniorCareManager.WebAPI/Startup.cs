using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Data.Repositories;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SeniorCareManager.WebAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SeniorCareManager.WebAPI.Objects.Models;


public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (env == "Production")
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
        }

        // AddIdentityCore (não AddIdentity) porque não usamos o RBAC padrão do Identity —
        // Role/Permission/PermissionGroup próprios chegam na §5. Todas as regras de
        // composição do Identity ficam desligadas: tamanho mínimo e bloqueio de senha comum
        // são responsabilidade exclusiva do InstitutionalPasswordPolicyValidator.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;
                options.User.RequireUniqueEmail = true;

                // O bloqueio por tentativas é orquestrado manualmente no login (§7.8), com
                // limite configurável por instituição — desliga o gatilho automático padrão
                // do Identity (5 tentativas/5 min fixos) para não haver dois mecanismos
                // decidindo a mesma coisa.
                options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddPasswordValidator<InstitutionalPasswordPolicyValidator>()
            // Habilita AuthenticatorTokenProvider/RecoveryCodeTokenProvider — MFA (§7) usa os
            // componentes prontos do Identity em vez de reimplementar TOTP/RFC 6238.
            .AddDefaultTokenProviders();

        // Cookie único de sessão (§7, decisão confirmada: sem bearer separado). Rotação e
        // detecção de reuso são manuais (SlidingExpiration=false) via OnValidatePrincipal —
        // não a renovação automática do ASP.NET Core, para controle preciso sobre reuso.
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = false;

                // Sem valor = cookie host-only (correto em dev/produção de origem única).
                // Setado = domínio pai (ex. ".exemplo.com.br"), compartilhado entre os
                // subdomínios dos dois front-ends (§7.4).
                var sessionCookieDomain = Configuration["SessionCookieDomain"];
                if (!string.IsNullOrWhiteSpace(sessionCookieDomain))
                    options.Cookie.Domain = sessionCookieDomain;

                // API, não MVC com views — devolve status HTTP em vez do redirect padrão.
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnValidatePrincipal = ValidateSessionPrincipalAsync;
            });
        services.AddAuthorization();

        //configuração do swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SeniorCareManager", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"Enter 'Bearer' [space] your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });
        
        //adiciona controllers e trata a serialização Json
        services.AddControllers(options =>
        {
            // Exige autenticação em todo controller por padrão (401 se anônimo) — os 4
            // endpoints de credencial em AuthController usam [AllowAnonymous] explicitamente.
            options.Filters.Add(new AuthorizeFilter());
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true; // Opcional, apenas para melhor legibilidade

            // Tarefa 3.6: o ID de um recurso é canônico pela rota — os *Request DTOs
            // deliberadamente não têm campo Id, então qualquer "id" (ou outro campo
            // desconhecido) no corpo é sinal de divergência do cliente. Por padrão o
            // System.Text.Json ignora campos desconhecidos silenciosamente; Disallow
            // rejeita com 400 em vez de aceitar e descartar sem avisar.
            options.JsonSerializerOptions.UnmappedMemberHandling =
                System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow;
        });

        // Problem Details (RFC 7807) centralizado — cobre tanto os 400 automáticos do
        // ModelState do [ApiController] quanto qualquer exceção não tratada (via
        // GlobalExceptionHandler abaixo), sempre com identificador de correlação.
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
            };
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // CORS_ALLOWED_ORIGINS (docker-compose de produção) sobrepõe os origins de dev —
        // sem isso, a imagem de produção nunca aceitaria requisição dos frontends reais.
        var corsOrigins = Configuration["CORS_ALLOWED_ORIGINS"]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:3001" };

        services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
        {
            builder.WithOrigins(corsOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }));

        /*
         //exemplo de correção da serialização Json com NewtonSoft.
        services.AddControllers()
            .AddNewtonsoftJson(opt =>
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

        */

        //Scoped Repositories and Interfaces repo
        services.AddScoped<IProductGroupService, ProductGroupService>();
        services.AddScoped<IProductTypeService, ProductTypeService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
        services.AddScoped<IHealthInsurancePlanService, HealthInsurancePlanService>();
        services.AddScoped<IManufacturerService, ManufacturerService>(); 
        services.AddScoped<ICarrierService, CarrierService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IReligionService,  ReligionService>();

        // Identidade e instituição (§4)
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
        services.AddScoped<IInstitutionIdentityOriginService, InstitutionIdentityOriginService>();
        services.AddScoped<IAccountTokenService, AccountTokenService>();
        services.AddScoped<IBootstrapService, BootstrapService>();
        services.AddSingleton<ICommonPasswordBlocklist, CommonPasswordBlocklist>();

        // Decisão de acesso (§5)
        services.AddScoped<IAccessDecisionService, AccessDecisionService>();

        // Configuração administrativa de acesso (§6)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminInvariantService, AdminInvariantService>();
        services.AddScoped<IInstitutionSecurityPolicyService, InstitutionSecurityPolicyService>();

        // Sessão, MFA e proteção contra abuso (§7)
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMfaPolicyService, MfaPolicyService>();
        services.AddSingleton<IOriginRateLimiter, OriginRateLimiter>();

        //Scoped Repositories and Interfaces repo
        services.AddScoped<IProductGroupRepository, ProductGroupRepository>();
        services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<IHealthInsurancePlanRepository, HealthInsurancePlanRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<ICarrierRepository, CarrierRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IReligionRepository, ReligionRepository>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // /health/live (vida) não checa dependências — só confirma que o processo está de
        // pé, mesmo com o banco fora. /health/ready (prontidão) inclui o check do banco,
        // marcado com a tag "ready" — é o que o HEALTHCHECK do container e o deploy.sh
        // esperam antes de considerar o deploy bem-sucedido.
        services.AddHealthChecks()
            .AddCheck<DbHealthCheck>("database", tags: new[] { "ready" });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Em produção a API só é alcançada via o nginx dos front-ends (infra/deploy/
        // docker-compose.yml), que envia X-Forwarded-For/X-Forwarded-Proto — sem isso,
        // HttpContext.Connection.RemoteIpAddress (usado por IOriginRateLimiter e gravado em
        // UserSession.IpAddress, §7.8) sempre vê o IP do container nginx: o limitador por
        // origem vira global (falhas de qualquer cliente bloqueiam todo mundo) e o IP
        // registrado na sessão perde valor diagnóstico. Precisa ser a primeira coisa no
        // pipeline — antes de qualquer coisa que leia o IP da conexão. KnownNetworks/
        // KnownProxies limpos = confia no cabeçalho de qualquer immediate hop; seguro só
        // enquanto a API não for alcançável fora da rede Docker (hoje o compose também
        // publica 8080 no host, o que permite spoofing direto — ver observação no PR).
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        // Property initializer não limpa a lista pré-populada (só loopback por padrão) —
        // precisa de Clear() explícito para de fato confiar no immediate hop.
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);

        // Centralizado e igual em todo ambiente — em dev, UseDeveloperExceptionPage()
        // vazaria stack trace no corpo da resposta (proibido pela 3.3); o
        // GlobalExceptionHandler é o único a converter exceção em resposta HTTP.
        app.UseExceptionHandler();

        // O documento OpenAPI (JSON) fica disponível em todo ambiente — é o contrato
        // que a tarefa 3.8 valida contra os dois front-ends via teste automatizado, não
        // só uma conveniência de desenvolvimento. Só a SwaggerUI (interativa) é dev-only.
        app.UseSwagger();

        if (env.IsDevelopment())
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SeniorCareManager Web API V1");
                // Adicione essas linhas para habilitar o botão "Authorize"
                c.DocExpansion(DocExpansion.None);
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableValidator();
                c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete);
                c.OAuthClientId("swagger-ui");
                c.OAuthAppName("Swagger UI");
            });
        }
        else
        {
            app.UseHsts();
        }

        // app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors("MyPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            // Vida: processo em execução, não avalia nenhuma dependência.
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
            });
            // Prontidão: inclui os checks marcados "ready" (hoje só o banco).
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
            });
        });
    }

    // Roda em toda requisição autenticada (§7.3): rotaciona a chave de sessão dentro da
    // janela de acesso curto, detecta reuso de uma chave já rotacionada (sinal de roubo —
    // revoga a sessão inteira) e rejeita sessões expiradas/revogadas. Não decide autorização
    // — só se o cookie apresentado ainda representa uma sessão válida.
    private static async Task ValidateSessionPrincipalAsync(CookieValidatePrincipalContext context)
    {
        var sessionIdClaim = context.Principal?.FindFirst(SeniorCareClaimTypes.SessionId)?.Value;
        var sessionKeyClaim = context.Principal?.FindFirst(SeniorCareClaimTypes.SessionKey)?.Value;
        if (!Guid.TryParse(sessionIdClaim, out var sessionId) || string.IsNullOrEmpty(sessionKeyClaim))
        {
            context.RejectPrincipal();
            return;
        }

        var sessionService = context.HttpContext.RequestServices.GetRequiredService<ISessionService>();
        var result = await sessionService.ValidateAndRotateAsync(sessionId, sessionKeyClaim);

        switch (result.Outcome)
        {
            case SessionValidationOutcome.Valid:
                return;

            case SessionValidationOutcome.Rotated:
                var identity = new ClaimsIdentity(context.Principal!.Claims, context.Scheme.Name);
                var oldClaim = identity.FindFirst(SeniorCareClaimTypes.SessionKey);
                if (oldClaim != null) identity.RemoveClaim(oldClaim);
                identity.AddClaim(new Claim(SeniorCareClaimTypes.SessionKey, result.NewRawKey!));
                context.ReplacePrincipal(new ClaimsPrincipal(identity));
                context.ShouldRenew = true;
                return;

            case SessionValidationOutcome.Reused:
            case SessionValidationOutcome.Rejected:
            default:
                // Não chama SignOutAsync aqui (reentrância no próprio pipeline de
                // autenticação) — RejectPrincipal já é suficiente: a sessão já está
                // revogada no banco, então o cookie (mesmo que o cliente o mantenha) nunca
                // mais volta a validar. O logout explícito (§7.1) é quem limpa o cookie.
                context.RejectPrincipal();
                return;
        }
    }
}