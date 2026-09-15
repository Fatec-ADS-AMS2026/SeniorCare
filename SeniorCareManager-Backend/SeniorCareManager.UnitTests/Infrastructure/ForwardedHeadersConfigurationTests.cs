using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SeniorCareManager.WebAPI.Infrastructure;

namespace SeniorCareManager.UnitTests.Infrastructure;

public sealed class ForwardedHeadersConfigurationTests
{
    [Fact]
    public void Create_TrustedCaddy_TrustsOnlyConfiguredProxy()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:TrustedProxies"] = "172.30.0.2",
        }).Build();

        var options = ForwardedHeadersConfiguration.Create(configuration);

        options.KnownProxies.Should().ContainSingle().Which.ToString().Should().Be("172.30.0.2");
        options.KnownNetworks.Should().BeEmpty();
    }

    [Fact]
    public void Create_InvalidProxy_FailsBeforeApplicationStarts()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:TrustedProxies"] = "not-an-ip",
        }).Build();

        var action = () => ForwardedHeadersConfiguration.Create(configuration);

        action.Should().Throw<InvalidOperationException>();
    }
}
