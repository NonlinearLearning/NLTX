namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
// crossSubsystemOwner: integration-review for crossover content definition and source mapping
// source-members: deadCellsMushroomBoiMinion, palworldCattivaMinion, palworldFoxsparksMinion
public sealed class PlayerCrossoverMinionCapabilityComponent
{
  public bool DeadCellsMushroomBoiMinion { get; internal set; }

  public bool PalworldCattivaMinion { get; internal set; }

  public bool PalworldFoxsparksMinion { get; internal set; }
}
