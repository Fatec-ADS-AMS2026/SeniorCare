using FluentAssertions;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Entities;

namespace SeniorCareManager.UnitTests.Services;

public class PasswordPolicyServiceTests
{
    private readonly PasswordPolicyService _service = new();

    [Fact]
    public void GetEffectiveMinLength_WithoutMfa_NoInstitution_Returns15()
    {
        _service.GetEffectiveMinLength(null, mfaEnabled: false).Should().Be(15);
    }

    [Fact]
    public void GetEffectiveMinLength_WithMfa_NoInstitution_Returns8()
    {
        _service.GetEffectiveMinLength(null, mfaEnabled: true).Should().Be(8);
    }

    [Fact]
    public void GetEffectiveMinLength_InstitutionOverrideStrongerThanFloor_UsesOverride()
    {
        var institution = new Institution(Guid.NewGuid(), "ILPI Exemplo")
        {
            MinPasswordLengthWithoutMfaOverride = 20
        };

        _service.GetEffectiveMinLength(institution, mfaEnabled: false).Should().Be(20);
    }

    [Fact]
    public void GetEffectiveMinLength_InstitutionOverrideWeakerThanFloor_KeepsFloor()
    {
        // Defesa em profundidade: mesmo que um override inválido chegue até aqui sem
        // passar por ValidateInstitutionOverride, o piso nunca é enfraquecido.
        var institution = new Institution(Guid.NewGuid(), "ILPI Exemplo")
        {
            MinPasswordLengthWithoutMfaOverride = 5
        };

        _service.GetEffectiveMinLength(institution, mfaEnabled: false).Should().Be(15);
    }

    [Fact]
    public void ValidateInstitutionOverride_WeakerThanFloorWithoutMfa_ThrowsBusinessRuleException()
    {
        var act = () => _service.ValidateInstitutionOverride(10, null);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateInstitutionOverride_WeakerThanFloorWithMfa_ThrowsBusinessRuleException()
    {
        var act = () => _service.ValidateInstitutionOverride(null, 5);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ValidateInstitutionOverride_StrongerThanFloor_DoesNotThrow()
    {
        var act = () => _service.ValidateInstitutionOverride(20, 12);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateInstitutionOverride_NullValues_DoesNotThrow()
    {
        var act = () => _service.ValidateInstitutionOverride(null, null);

        act.Should().NotThrow();
    }
}
