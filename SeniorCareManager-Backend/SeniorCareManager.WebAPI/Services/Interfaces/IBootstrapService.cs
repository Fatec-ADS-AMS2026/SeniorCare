using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IBootstrapService
{
    // Idempotente: se já existir qualquer instituição, é no-op (Created = false). Só cria a
    // instituição + admin ACTIVE com senha aleatória exibida uma única vez quando não houver
    // nenhuma instituição e as variáveis de bootstrap estiverem presentes.
    Task<BootstrapResult> RunAsync(CancellationToken cancellationToken = default);
}
