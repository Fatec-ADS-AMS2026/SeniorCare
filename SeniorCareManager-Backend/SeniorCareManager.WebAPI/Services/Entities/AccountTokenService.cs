using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class AccountTokenService : IAccountTokenService
{
    public static readonly TimeSpan ActivationTokenValidity = TimeSpan.FromDays(7);
    public static readonly TimeSpan RecoveryTokenValidity = TimeSpan.FromHours(1);

    private readonly AppDbContext _dbContext;

    public AccountTokenService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> IssueAsync(Guid userId, AccountTokenPurpose purpose, TimeSpan validity, CancellationToken cancellationToken = default)
    {
        var rawToken = GenerateRawToken();
        var now = DateTime.UtcNow;

        _dbContext.AccountTokens.Add(new AccountToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Purpose = purpose,
            TokenHash = Hash(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(validity)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return rawToken;
    }

    public async Task<bool> ConsumeAsync(Guid userId, AccountTokenPurpose purpose, string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawToken))
            return false;

        var tokenHash = Hash(rawToken);
        var token = await _dbContext.AccountTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash && t.UserId == userId && t.Purpose == purpose,
            cancellationToken);

        if (token == null || token.UsedAtUtc != null || token.ExpiresAtUtc < DateTime.UtcNow)
            return false;

        token.UsedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Guid?> ValidateAsync(AccountTokenPurpose purpose, string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawToken))
            return null;

        var tokenHash = Hash(rawToken);
        var token = await _dbContext.AccountTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash && t.Purpose == purpose,
            cancellationToken);

        if (token == null || token.UsedAtUtc != null || token.ExpiresAtUtc < DateTime.UtcNow)
            return null;

        return token.UserId;
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
