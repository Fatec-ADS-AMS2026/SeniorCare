namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

// Só a resposta de criação (§10.6) leva o token — não existe serviço de e-mail no
// projeto, então o admin precisa poder ver/copiar o token de ativação pra repassar
// por fora. GetAll/GetById continuam devolvendo AdminUserDTO puro, sem o campo, pra
// não expor um token de ativação indefinidamente a qualquer leitura.
public class AdminUserCreatedDTO : AdminUserDTO
{
    public string ActivationToken { get; set; } = string.Empty;
}
