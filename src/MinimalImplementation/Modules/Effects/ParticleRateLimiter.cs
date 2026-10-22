using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Effects;

/// <summary>Bounds per-session particle fan-out with a one-second sliding request window.</summary>
internal sealed class ParticleRateLimiter
{
  private const int MaximumTrackedSessions = byte.MaxValue;
  private const int MaximumRequestsPerWindow = 60;
  private static readonly TimeSpan Window = TimeSpan.FromSeconds(1);

  private readonly Dictionary<SessionEpoch, SessionWindow> _sessions = new();

  internal bool TryConsume(SessionEpoch epoch, DateTimeOffset receivedAt)
  {
    if (!epoch.IsValid)
    {
      return false;
    }

    if (!_sessions.TryGetValue(epoch, out SessionWindow? session))
    {
      if (_sessions.Count >= MaximumTrackedSessions)
      {
        return false;
      }

      session = new SessionWindow();
      _sessions.Add(epoch, session);
    }

    if (session.LastSeen is DateTimeOffset lastSeen && receivedAt < lastSeen)
    {
      return false;
    }

    session.LastSeen = receivedAt;
    DateTimeOffset windowStart = receivedAt <= DateTimeOffset.MinValue + Window
      ? DateTimeOffset.MinValue
      : receivedAt - Window;
    while (session.RequestTimes.Count > 0 && session.RequestTimes.Peek() <= windowStart)
    {
      session.RequestTimes.Dequeue();
    }

    if (session.RequestTimes.Count >= MaximumRequestsPerWindow)
    {
      return false;
    }

    session.RequestTimes.Enqueue(receivedAt);
    return true;
  }

  internal void RetireEpoch(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    _sessions.Remove(epoch);
  }

  private sealed class SessionWindow
  {
    internal Queue<DateTimeOffset> RequestTimes { get; } = new();

    internal DateTimeOffset? LastSeen { get; set; }
  }
}
