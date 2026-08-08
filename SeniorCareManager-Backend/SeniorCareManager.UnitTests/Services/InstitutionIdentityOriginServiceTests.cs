using FluentAssertions;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Entities;

namespace SeniorCareManager.UnitTests.Services;

public class InstitutionIdentityOriginServiceTests
{
    private readonly InstitutionIdentityOriginService _service = new();

    [Fact]
    public void EnsureOriginAvailable_Local_DoesNotThrow()
    {
        var act = () => _service.EnsureOriginAvailable(IdentityOrigin.LOCAL);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureOriginAvailable_Ldap_ThrowsBusinessRuleException()
    {
        var act = () => _service.EnsureOriginAvailable(IdentityOrigin.LDAP);

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void EnsureOriginAvailable_Oidc_ThrowsBusinessRuleException()
    {
        var act = () => _service.EnsureOriginAvailable(IdentityOrigin.OIDC);

        act.Should().Throw<BusinessRuleException>();
    }
}
