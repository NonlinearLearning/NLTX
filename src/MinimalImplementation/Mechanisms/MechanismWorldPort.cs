using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// The narrow P06 view of the P03 world owner.
///
/// Mechanism rules read immutable snapshots and submit deltas through this port. They do not
/// retain a second tile map or mutate a snapshot in place.
/// </summary>
public sealed class MechanismWorldPort
{
  private readonly WorldOwner _owner;

  public MechanismWorldPort(WorldOwner owner)
  {
    ArgumentNullException.ThrowIfNull(owner);
    _owner = owner;
  }

  public WorldSnapshot CaptureSnapshot() => _owner.CaptureSnapshot();

  public bool TryPrepareTilePatches(
    ulong worldEpoch,
    CommitVersion expectedVersion,
    IReadOnlyList<WorldTilePatch> patches,
    bool producesImmediateBroadcast,
    out WorldTileMutationPreparation? preparation,
    out WorldValidationResult validation)
  {
    return _owner.TryPrepareTilePatches(
      worldEpoch, expectedVersion, patches, producesImmediateBroadcast,
      out preparation, out validation);
  }

  public WorldMutationResult ApplyTileDeltas(
    ulong worldEpoch,
    IReadOnlyList<KeyValuePair<TileCoordinate, WorldTileDelta>> deltas,
    bool producesImmediateBroadcast)
  {
    return _owner.ApplyTileDeltas(worldEpoch, deltas, producesImmediateBroadcast);
  }

  public bool TryPrepareTileDeltas(
    ulong worldEpoch,
    CommitVersion expectedVersion,
    IReadOnlyList<KeyValuePair<TileCoordinate, WorldTileDelta>> deltas,
    bool producesImmediateBroadcast,
    out WorldTileMutationPreparation? preparation,
    out WorldValidationResult validation)
  {
    ArgumentNullException.ThrowIfNull(deltas);
    var patches = new WorldTilePatch[deltas.Count];
    for (int index = 0; index < deltas.Count; index++)
    {
      (TileCoordinate coordinate, WorldTileDelta delta) = deltas[index];
      patches[index] = new WorldTilePatch(
        coordinate, delta.Active, delta.Type, delta.Wall, delta.FrameX, delta.FrameY,
        delta.LiquidAmount, delta.LiquidType, delta.TileColor, delta.WallColor,
        delta.Wires, delta.Actuator, delta.Inactive, delta.HalfBlock, delta.Slope,
        delta.TileHeader3, delta.WallHeader);
    }
    return TryPrepareTilePatches(
      worldEpoch, expectedVersion, patches, producesImmediateBroadcast,
      out preparation, out validation);
  }
}
