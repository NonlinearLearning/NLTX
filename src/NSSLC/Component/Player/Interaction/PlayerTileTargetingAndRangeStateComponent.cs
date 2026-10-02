namespace Terraria.Player.Interaction;

// status: implemented
// componentId: PLAYER.COMP.TILE_TARGETING_AND_RANGE_STATE
// source-members: P08-1140, P08-1141, P08-1152
// crossSubsystemOwner: integration-review
public sealed class PlayerTileTargetingAndRangeStateComponent
{
  public int LastTileRangeX { get; internal set; }

  public int LastTileRangeY { get; internal set; }

  public bool[] AdjacentTiles { get; } = Array.Empty<bool>();
}
