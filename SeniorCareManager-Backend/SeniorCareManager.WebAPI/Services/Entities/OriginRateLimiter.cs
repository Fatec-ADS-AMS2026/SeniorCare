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

        TrimAndCleanup(origin, queue);
        return queue.Count >= MaxFailuresPerWindow;
    }

    public void RecordFailure(string origin)
    {
        var queue = _failures.GetOrAdd(origin, _ => new ConcurrentQueue<DateTime>());
        queue.Enqueue(DateTime.UtcNow);
        TrimAndCleanup(origin, queue);
    }

    // Sem isso, o dicionário cresce sem limite por origem única vista ao longo da vida do
    // processo — antes só importava em teoria (todo tráfego batia com o mesmo IP do proxy,
    // §7.8), mas agora que os cabeçalhos X-Forwarded-For são respeitados (Startup.cs), IPs
    // reais de clientes distintos chegam de fato. Remoção é best-effort: uma corrida
    // concorrente que recria a entrada logo após é inofensiva pra um limitador de taxa.
    private void TrimAndCleanup(string origin, ConcurrentQueue<DateTime> queue)
    {
        var cutoff = DateTime.UtcNow - Window;
        while (queue.TryPeek(out var oldest) && oldest < cutoff)
            queue.TryDequeue(out _);

        if (queue.IsEmpty)
            _failures.TryRemove(new KeyValuePair<string, ConcurrentQueue<DateTime>>(origin, queue));
    }
}
