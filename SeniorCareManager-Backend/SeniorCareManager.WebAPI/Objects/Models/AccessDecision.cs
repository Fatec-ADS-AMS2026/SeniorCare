namespace SeniorCareManager.WebAPI.Objects.Models
{
    public class AccessDecision
    {
        public bool Allowed { get; init; }

        public string DeterminingLayer { get; init; } = string.Empty;
    }
}
