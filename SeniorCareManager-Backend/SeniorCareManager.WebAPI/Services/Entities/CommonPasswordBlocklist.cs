using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

// Lista local embutida (Data/CommonPasswords.txt) em vez de um serviço externo tipo HIBP:
// mantém CI/testes 100% offline. Carregada uma vez por processo (singleton).
public class CommonPasswordBlocklist : ICommonPasswordBlocklist
{
    private const string ResourceName = "SeniorCareManager.WebAPI.Data.CommonPasswords.txt";

    private readonly HashSet<string> _blockedPasswords;

    public CommonPasswordBlocklist()
    {
        _blockedPasswords = Load();
    }

    public bool IsCommon(string password)
    {
        return _blockedPasswords.Contains(password.ToLowerInvariant());
    }

    private static HashSet<string> Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Recurso embutido não encontrado: {ResourceName}");
        using var reader = new StreamReader(stream);

        var blocked = new HashSet<string>();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
                blocked.Add(trimmed.ToLowerInvariant());
        }

        return blocked;
    }
}
