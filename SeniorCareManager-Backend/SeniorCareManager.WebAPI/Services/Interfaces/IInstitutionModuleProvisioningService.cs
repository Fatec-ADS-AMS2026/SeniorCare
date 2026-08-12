namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IInstitutionModuleProvisioningService
{
    // Idempotente: para cada instituição existente × cada ModuleDefinition ativa, cria o
    // InstitutionModule que faltar (DISABLED, IsEnabled=false) e não toca no que já existe.
    // Retorna quantas linhas foram criadas nesta execução (0 = já estava tudo provisionado).
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
