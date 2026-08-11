using FluentAssertions;
using SeniorCareManager.WebAPI.Infrastructure.Validation;

namespace SeniorCareManager.UnitTests;

public class OperationalMessageSanitizerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Módulo em manutenção programada, volta em breve.")]
    public void Validate_NullEmptyOrPlainText_ReturnsNoErrors(string? message)
    {
        var errors = OperationalMessageSanitizer.Validate(message);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_TooLong_ReturnsError()
    {
        var message = new string('a', 281);

        var errors = OperationalMessageSanitizer.Validate(message);

        errors.Should().ContainMatch("*limite*");
    }

    [Theory]
    [InlineData("<b>manutenção</b>")]
    [InlineData("valor > 10")]
    public void Validate_ContainsHtmlLikeCharacters_ReturnsError(string message)
    {
        var errors = OperationalMessageSanitizer.Validate(message);

        errors.Should().ContainMatch("*HTML*");
    }

    [Theory]
    [InlineData("Detalhes em https://example.com/status")]
    [InlineData("http://example.com")]
    public void Validate_ContainsUrl_ReturnsError(string message)
    {
        var errors = OperationalMessageSanitizer.Validate(message);

        errors.Should().ContainMatch("*URL*");
    }

    [Theory]
    [InlineData("Prontuário indisponível temporariamente")]
    [InlineData("dashboard fora do ar")]
    [InlineData("assinatura pendente de validação")]
    public void Validate_ContainsClinicalScopeTerm_ReturnsError(string message)
    {
        var errors = OperationalMessageSanitizer.Validate(message);

        errors.Should().ContainMatch("*clínico*");
    }
}
