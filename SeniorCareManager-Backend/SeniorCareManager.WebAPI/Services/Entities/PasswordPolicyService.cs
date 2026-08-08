using System;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class PasswordPolicyService : IPasswordPolicyService
{
    public const int MinLengthWithoutMfa = 15;
    public const int MinLengthWithMfa = 8;

    // >=64 é o mínimo aceito pela spec; 256 é uma proteção técnica contra abuso de hashing,
    // não um limite de negócio.
    public const int MaxLength = 256;

    public int GetEffectiveMinLength(Institution? institution, bool mfaEnabled)
    {
        var floor = mfaEnabled ? MinLengthWithMfa : MinLengthWithoutMfa;
        if (institution == null)
            return floor;

        var overrideValue = mfaEnabled
            ? institution.MinPasswordLengthWithMfaOverride
            : institution.MinPasswordLengthWithoutMfaOverride;

        return overrideValue.HasValue ? Math.Max(floor, overrideValue.Value) : floor;
    }

    public void ValidateInstitutionOverride(int? minLengthWithoutMfaOverride, int? minLengthWithMfaOverride)
    {
        if (minLengthWithoutMfaOverride.HasValue && minLengthWithoutMfaOverride.Value < MinLengthWithoutMfa)
            throw new BusinessRuleException(
                $"Configuração institucional não pode reduzir o piso de senha sem MFA abaixo de {MinLengthWithoutMfa} caracteres.");

        if (minLengthWithMfaOverride.HasValue && minLengthWithMfaOverride.Value < MinLengthWithMfa)
            throw new BusinessRuleException(
                $"Configuração institucional não pode reduzir o piso de senha com MFA abaixo de {MinLengthWithMfa} caracteres.");
    }
}
