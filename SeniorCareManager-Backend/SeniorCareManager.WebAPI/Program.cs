using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            ValidateConfiguration(host.Services.GetRequiredService<IConfiguration>());

            // Roda as migrations pendentes no boot — o container nunca chega a rodar sem
            // schema. Consistente com o `deploy.sh`/healthcheck: o serviço só fica "healthy"
            // depois que o banco está no schema esperado.
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                // Idempotente: só cria instituição + admin PROVISIONED na primeira vez (nenhuma
                // instituição ainda existe). O link de ativação só existe neste momento — não é
                // persistido em lugar nenhum, então só pode ser capturado aqui.
                var bootstrap = scope.ServiceProvider.GetRequiredService<IBootstrapService>();
                var result = bootstrap.RunAsync().GetAwaiter().GetResult();
                if (result.Created)
                {
                    Console.WriteLine("Bootstrap: instituição e administrador PROVISIONED criados.");
                    Console.WriteLine($"  Administrador: {result.AdminEmail}");
                    Console.WriteLine($"  Token de ativação (uso único, capture agora — não será reimpresso): {result.ActivationToken}");
                }
            }

            host.Run();
        }

        // Separado de ValidateConfiguration para ser testável sem derrubar o processo do
        // test runner (Environment.Exit mataria o xUnit inteiro se chamado num teste).
        public static List<string> GetMissingConfiguration(IConfiguration configuration)
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
                missing.Add("ConnectionStrings:DefaultConnection (variável de ambiente: ConnectionStrings__DefaultConnection)");

            // As três variáveis de bootstrap só fazem sentido juntas: config parcial indica
            // que o operador esqueceu uma delas, então falha cedo em vez de deixar a
            // instalação sem instituição silenciosamente (tarefa 2.5).
            var bootstrapKeys = new[] { "Bootstrap:InstitutionName", "Bootstrap:AdminEmail", "Bootstrap:AdminDisplayName" };
            var bootstrapValues = bootstrapKeys.Select(k => configuration[k]).ToList();
            var bootstrapProvided = bootstrapValues.Count(v => !string.IsNullOrWhiteSpace(v));
            if (bootstrapProvided > 0 && bootstrapProvided < bootstrapKeys.Length)
                missing.Add(
                    "Bootstrap__InstitutionName, Bootstrap__AdminEmail e Bootstrap__AdminDisplayName devem ser " +
                    "todas informadas juntas ou nenhuma (bootstrap parcialmente configurado)");

            return missing;
        }

        // Falha cedo e com mensagem clara quando falta configuração obrigatória, em vez de
        // deixar o Npgsql lançar uma exceção de conexão mais difícil de diagnosticar lá na
        // frente. Nunca ecoa o valor configurado — só o nome da chave ausente.
        private static void ValidateConfiguration(IConfiguration configuration)
        {
            var missing = GetMissingConfiguration(configuration);
            if (missing.Count == 0)
                return;

            Console.Error.WriteLine("Configuração obrigatória ausente — o processo não pode iniciar:");
            foreach (var key in missing)
                Console.Error.WriteLine($"  - {key}");
            Console.Error.WriteLine("Ver CONFIGURATION.md para o formato esperado de cada variável.");
            Environment.Exit(1);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}