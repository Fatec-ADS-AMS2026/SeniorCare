using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace SeniorCareManager.WebAPI.Infrastructure;

public static class ForwardedHeadersConfiguration
{
    public static ForwardedHeadersOptions Create(IConfiguration configuration)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        var trustedProxies = configuration["ForwardedHeaders:TrustedProxies"];
        if (string.IsNullOrWhiteSpace(trustedProxies)) return options;

        foreach (var value in trustedProxies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!IPAddress.TryParse(value, out var address))
                throw new InvalidOperationException($"ForwardedHeaders:TrustedProxies contém IP inválido: {value}");

            options.KnownProxies.Add(address);
        }

        return options;
    }
}
