namespace Terraria.Player;

public sealed class PlayerDpsTelemetryComponent
{
  public DateTimeOffset DpsStart { get; internal set; }

  public DateTimeOffset DpsEnd { get; internal set; }

  public DateTimeOffset DpsLastHit { get; internal set; }

  public int DpsDamage { get; internal set; }

  public bool DpsStarted { get; internal set; }

  internal void Reset()
  {
    DpsStart = default;
    DpsEnd = default;
    DpsLastHit = default;
    DpsDamage = 0;
    DpsStarted = false;
  }
}
