namespace Terraria.Player;

public sealed class PlayerAbilityComponent
{
  public float MaximumMinionSlots { get; set; } = 1;

  public float UsedMinionSlots { get; set; }

  public int MinionCount { get; set; }

  public int MaximumTurrets { get; set; } = 1;

  public int PreviousMaximumTurrets { get; set; } = 1;

  public MinionFeatureFlags FeatureFlags { get; set; }
}
