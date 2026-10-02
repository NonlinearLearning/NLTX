namespace Terraria.Player;

public sealed class PlayerSummonCapacityState
{
  public float MaximumMinionSlots { get; set; }

  public float UsedMinionSlots { get; set; }

  public int MinionCount { get; set; }

  public int MaximumTurrets { get; set; }

  public int PreviousMaximumTurrets { get; set; }

  public MinionFeatureFlags MinionFeatureFlags { get; set; }

  public float RemainingMinionSlots => MaximumMinionSlots - UsedMinionSlots;

  public bool HasMinionCapacity => RemainingMinionSlots > 0;

  public bool RequiresTurretTrim => MaximumTurrets < PreviousMaximumTurrets;
}
