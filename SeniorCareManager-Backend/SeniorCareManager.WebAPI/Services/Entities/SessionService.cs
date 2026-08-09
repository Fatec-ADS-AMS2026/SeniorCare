using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class SessionService : ISessionService
{
    public const int DefaultAccessTokenDurationMinutes = 15;
    public const int DefaultRefreshTokenDurationDays = 7;

    private readonly AppDbContext _dbContext;

    public SessionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(Guid SessionId, string RawKey)> CreateAsync(
        Guid userId, string? userAgent, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId, cancellationToken);
        var institution = await _dbContext.Institutions.FindAsync(new object[] { user.InstitutionId }, cancellationToken);
        var refreshDurationDays = institution?.RefreshTokenDurationDays ?? DefaultRefreshTokenDurationDays;

        var rawKey = GenerateRawKey();
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAtUtc = now,
            LastSeenAtUtc = now,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CurrentKeyHash = Hash(rawKey),
            PreviousKeyHash = null,
            ExpiresAtUtc = now.AddDays(refreshDurationDays),
            LastRotatedAtUtc = now,
        };
        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (session.Id, rawKey);
    }

    public async Task<SessionValidationResult> ValidateAndRotateAsync(
        Guid sessionId, string presentedRawKey, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions.SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session == null || session.RevokedAtUtc != null)
            return Result(SessionValidationOutcome.Rejected);

        var now = DateTime.UtcNow;
        if (session.ExpiresAtUtc < now)
            return Result(SessionValidationOutcome.Rejected);

        var presentedHash = Hash(presentedRawKey);

        if (presentedHash == session.PreviousKeyHash)
        {
            // Chave já rotacionada sendo reapresentada — sinal de roubo. Revoga a sessão
            // inteira, não só a chave.
            session.RevokedAtUtc = now;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result(SessionValidationOutcome.Reused);
        }

        if (presentedHash != session.CurrentKeyHash)
            return Result(SessionValidationOutcome.Rejected);

        session.LastSeenAtUtc = now;

        var user = await _dbContext.Users.SingleAsync(u => u.Id == session.UserId, cancellationToken);
        var institution = await _dbContext.Institutions.FindAsync(new object[] { user.InstitutionId }, cancellationToken);
        var accessDurationMinutes = institution?.AccessTokenDurationMinutes ?? DefaultAccessTokenDurationMinutes;

        if (now - session.LastRotatedAtUtc < TimeSpan.FromMinutes(accessDurationMinutes))
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result(SessionValidationOutcome.Valid);
        }

        var newRawKey = GenerateRawKey();
        session.PreviousKeyHash = session.CurrentKeyHash;
        session.CurrentKeyHash = Hash(newRawKey);
        session.LastRotatedAtUtc = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result(SessionValidationOutcome.Rotated, newRawKey);
    }

    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.UserSessions.SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session != null && session.RevokedAtUtc == null)
        {
            session.RevokedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var activeSessions = await _dbContext.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var session in activeSessions)
            session.RevokedAtUtc = now;

        if (activeSessions.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SessionValidationResult Result(SessionValidationOutcome outcome, string? newRawKey = null) =>
        new() { Outcome = outcome, NewRawKey = newRawKey };

    private static string GenerateRawKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string Hash(string rawKey)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToBase64String(bytes);
    }
}
