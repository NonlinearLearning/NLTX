namespace Terraria.Player;

public sealed class PlayerDpsTelemetrySystem : IPlayerDpsTelemetryCommitPort
{
  private readonly HashSet<Guid> _acceptedEventIds = [];
  private readonly PlayerDpsTelemetryComponent _component;
  private readonly IPlayerTelemetryClock _clock;

  public PlayerDpsTelemetrySystem(
    PlayerDpsTelemetryComponent component,
    IPlayerTelemetryClock clock)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(clock);

    _component = component;
    _clock = clock;
  }

  public bool AcceptCommittedDamage(in PlayerCommittedDamageEvent damageEvent)
  {
    if (damageEvent.EventId == Guid.Empty || damageEvent.Damage <= 0)
    {
      return false;
    }

    if (!_acceptedEventIds.Add(damageEvent.EventId))
    {
      return false;
    }

    if (!_component.DpsStarted)
    {
      _component.DpsStart = damageEvent.CommittedAt;
      _component.DpsDamage = damageEvent.Damage;
      _component.DpsStarted = true;
    }
    else
    {
      _component.DpsDamage = SaturatingAdd(
        _component.DpsDamage,
        damageEvent.Damage);
    }

    _component.DpsEnd = damageEvent.CommittedAt;
    _component.DpsLastHit = damageEvent.CommittedAt;
    return true;
  }

  public bool ReconcileCapability(bool isAvailable)
  {
    return isAvailable ? false : Stop();
  }

  public bool Stop()
  {
    if (!_component.DpsStarted)
    {
      return false;
    }

    DateTimeOffset stoppedAt = _clock.UtcNow;
    _component.DpsStarted = false;
    _component.DpsEnd = stoppedAt;
    return true;
  }

  public void Reset()
  {
    _component.Reset();
    _acceptedEventIds.Clear();
  }

  private static int SaturatingAdd(int currentDamage, int additionalDamage)
  {
    return currentDamage > int.MaxValue - additionalDamage
      ? int.MaxValue
      : currentDamage + additionalDamage;
  }
}
