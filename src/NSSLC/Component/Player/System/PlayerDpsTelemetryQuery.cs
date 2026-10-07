namespace Terraria.Player;

public sealed class PlayerDpsTelemetryQuery : IPlayerDpsTelemetryQuery
{
  public PlayerDpsTelemetrySnapshot Snapshot(
    PlayerDpsTelemetryComponent component,
    DateTimeOffset observationTime)
  {
    ArgumentNullException.ThrowIfNull(component);

    if (!component.DpsStarted && component.DpsDamage == 0)
    {
      return new PlayerDpsTelemetrySnapshot(
        HasWindow: false,
        IsActive: false,
        StartedAt: null,
        EndedAt: null,
        LastHitAt: null,
        Damage: 0,
        Duration: TimeSpan.Zero,
        DamagePerSecond: 0d);
    }

    DateTimeOffset durationEnd = component.DpsStarted
      ? observationTime
      : component.DpsEnd;
    TimeSpan duration = CalculateNonNegativeDuration(
      component.DpsStart,
      durationEnd);
    double damagePerSecond = duration > TimeSpan.Zero
      ? component.DpsDamage / duration.TotalSeconds
      : 0d;

    return new PlayerDpsTelemetrySnapshot(
      HasWindow: true,
      IsActive: component.DpsStarted,
      StartedAt: component.DpsStart,
      EndedAt: component.DpsStarted ? null : component.DpsEnd,
      LastHitAt: component.DpsLastHit,
      Damage: component.DpsDamage,
      Duration: duration,
      DamagePerSecond: damagePerSecond);
  }

  private static TimeSpan CalculateNonNegativeDuration(
    DateTimeOffset start,
    DateTimeOffset end)
  {
    return end <= start ? TimeSpan.Zero : end - start;
  }
}
