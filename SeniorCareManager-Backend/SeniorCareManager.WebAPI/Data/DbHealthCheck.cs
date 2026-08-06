using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SeniorCareManager.WebAPI.Data
{
    // Checa a conexão com o Postgres — usado pelo HEALTHCHECK do container e pelo
    // deploy.sh para só considerar o serviço no ar depois que o banco responde.
    public class DbHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;

        public DbHealthCheck(AppDbContext db)
        {
            _db = db;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await _db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados.");
        }
    }
}
