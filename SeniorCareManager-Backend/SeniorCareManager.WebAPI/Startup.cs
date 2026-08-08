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
        services.AddControllers().AddJsonOptions(options =>
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
        // Centralizado e igual em todo ambiente — em dev, UseDeveloperExceptionPage()
        // vazaria stack trace no corpo da resposta (proibido pela 3.3); o
        // GlobalExceptionHandler é o único a converter exceção em resposta HTTP.
        app.UseExceptionHandler();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
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

        // app.UseAuthorization();

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
}