namespace Terraria.WorldSession.Components;

public sealed class WorldMilestoneStateComponent {
  public bool AnyMechBossDowned { get; internal set; }
  public bool ShadowOrbSmashed { get; internal set; }
  public byte ShadowOrbCount { get; internal set; }
  public int AltarCount { get; internal set; }
}
