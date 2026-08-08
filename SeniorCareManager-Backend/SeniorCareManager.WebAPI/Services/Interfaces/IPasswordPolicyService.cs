using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IPasswordPolicyService
{
    int GetEffectiveMinLength(Institution? institution, bool mfaEnabled);

    void ValidateInstitutionOverride(int? minLengthWithoutMfaOverride, int? minLengthWithMfaOverride);
}
