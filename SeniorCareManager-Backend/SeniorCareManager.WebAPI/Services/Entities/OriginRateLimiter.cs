using System;
using System.Collections.Concurrent;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class OriginRateLimiter : IOriginRateLimiter
{
    public const int MaxFailuresPerWindow = 20;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _failures = new();

    public bool IsBlocked(string origin)
    {
        if (!_failures.TryGetValue(origin, out var queue))
            return false;

        Trim(queue);
        return queue.Count >= MaxFailuresPerWindow;
    }

    public void RecordFailure(string origin)
    {
        var queue = _failures.GetOrAdd(origin, _ => new ConcurrentQueue<DateTime>());
        queue.Enqueue(DateTime.UtcNow);
        Trim(queue);
    }

    private static void Trim(ConcurrentQueue<DateTime> queue)
    {
        var cutoff = DateTime.UtcNow - Window;
        while (queue.TryPeek(out var oldest) && oldest < cutoff)
            queue.TryDequeue(out _);
    }
}
