using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Data.Repositories;
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SeniorCareManager.WebAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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
        });

        services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
        {
            builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
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

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
      
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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
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
            app.UseExceptionHandler("/home/Error");
            app.UseHsts();
        }

        // app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors("MyPolicy");

        // app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}