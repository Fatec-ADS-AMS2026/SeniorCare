using FluentAssertions;
using SeniorCareManager.WebAPI.Infrastructure.Validation;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.UnitTests;

public class ModuleDefinitionValidatorTests
{
    private static readonly Guid ValidPermissionId = Guid.NewGuid();
    private static readonly IReadOnlySet<Guid> ExistingPermissionIds = new HashSet<Guid> { ValidPermissionId };

    private static ModuleDefinition ValidCandidate() => new(
        1, "care", "Assistência", "Cuidado e acompanhamento dos residentes",
        "HeartStraight", "/care", ValidPermissionId);

    [Fact]
    public void Validate_ValidCandidate_ReturnsNoErrors()
    {
        var errors = ModuleDefinitionValidator.Validate(ValidCandidate(), ExistingPermissionIds);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Care")]
    [InlineData("care_module")]
    [InlineData("1care")]
    [InlineData("c")]
    public void Validate_InvalidKey_ReturnsError(string key)
    {
        var candidate = ValidCandidate();
        candidate.Key = key;

        var errors = ModuleDefinitionValidator.Validate(candidate, ExistingPermissionIds);

        errors.Should().ContainMatch("Key*");
    }

    [Fact]
    public void Validate_IconNotInAllowlist_ReturnsError()
    {
        var candidate = ValidCandidate();
        candidate.Icon = "<script>alert(1)</script>";

        var errors = ModuleDefinitionValidator.Validate(candidate, ExistingPermissionIds);

        errors.Should().ContainMatch("Icon*");
    }

    [Theory]
    [InlineData("https://evil.example/phish")]
    [InlineData("//evil.example")]
    [InlineData("care")]
    [InlineData("/admin-other")]
    public void Validate_PathNotAllowed_ReturnsError(string path)
    {
        var candidate = ValidCandidate();
        candidate.Path = path;

        var errors = ModuleDefinitionValidator.Validate(candidate, ExistingPermissionIds);

        errors.Should().ContainMatch("Path*");
    }

    [Fact]
    public void Validate_PermissionDoesNotExist_ReturnsError()
    {
        var candidate = ValidCandidate();
        candidate.RequiredPermissionId = Guid.NewGuid();

        var errors = ModuleDefinitionValidator.Validate(candidate, ExistingPermissionIds);

        errors.Should().ContainMatch("RequiredPermissionId*");
    }
}
