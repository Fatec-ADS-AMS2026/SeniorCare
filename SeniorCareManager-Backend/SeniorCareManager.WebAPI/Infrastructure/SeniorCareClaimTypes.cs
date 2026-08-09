namespace SeniorCareManager.WebAPI.Infrastructure;

public static class SeniorCareClaimTypes
{
    // UserId vai em ClaimTypes.NameIdentifier (padrão do ASP.NET Core Identity).
    public const string InstitutionId = "institution_id";

    // §7: identificam a UserSession e a chave opaca atual — usados só pelo hook de
    // rotação/detecção de reuso (Startup.cs), nunca pela decisão de autorização em si.
    public const string SessionId = "session_id";
    public const string SessionKey = "session_key";
}
