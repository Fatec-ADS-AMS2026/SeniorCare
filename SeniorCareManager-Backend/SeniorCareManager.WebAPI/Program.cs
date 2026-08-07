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