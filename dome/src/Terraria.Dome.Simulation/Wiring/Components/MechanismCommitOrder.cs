namespace Terraria.Dome.Simulation.Wiring.Components;

public readonly record struct MechanismCommitOrder(
  long Sequence,
  bool ApplyTileChangesBeforeLiquid,
  bool ApplyLiquidBeforeEvents)
{
  public static MechanismCommitOrder Default(long sequence) => new(sequence, true, false);
}
