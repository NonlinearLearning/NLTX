namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-720, P09-721, P09-722, P09-723
// crossSubsystemOwner: environment ticking and derived capability projections remain integration-review
public sealed class PlayerResourceStateComponent
{
  public const int DefaultBreathMax = 200;

  public const float MinimumLavaOpacity = 0.4f;

  public const float MaximumLavaOpacity = 1f;

  public int BreathMax { get; internal set; } = DefaultBreathMax;

  public int Breath { get; internal set; } = DefaultBreathMax;

  public int LavaMax { get; internal set; }

  public int LavaTime { get; internal set; }

  public bool IgnoreWater { get; internal set; }

  public bool LavaVision { get; internal set; }

  public float LavaOpacity { get; internal set; } = MaximumLavaOpacity;
}
