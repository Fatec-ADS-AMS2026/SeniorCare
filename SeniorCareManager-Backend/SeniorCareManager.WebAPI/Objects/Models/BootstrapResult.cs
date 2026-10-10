namespace SeniorCareManager.WebAPI.Objects.Models
{
    public class BootstrapResult
    {
        public bool Created { get; init; }

        public string? AdminEmail { get; init; }

        public string? InitialPassword { get; init; }
    }
}
