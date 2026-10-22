using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

public sealed partial class WorldOwner {
  /// <summary>
  /// Applies Packet 63 paint or fullbright coating through the world owner.
  /// </summary>
  public WorldMutationResult ApplyTilePaintOrCoating(
      ulong worldEpoch, TileCoordinate coordinate, byte paintOrCoating, byte coatingMode) {
    return ApplyPaintOrCoating(worldEpoch, coordinate, paintOrCoating, coatingMode, wall: false);
  }

  /// <summary>
  /// Applies Packet 64 paint or fullbright coating through the world owner.
  /// </summary>
  public WorldMutationResult ApplyWallPaintOrCoating(
      ulong worldEpoch, TileCoordinate coordinate, byte paintOrCoating, byte coatingMode) {
    return ApplyPaintOrCoating(worldEpoch, coordinate, paintOrCoating, coatingMode, wall: true);
  }

  // Same-version WorldGen.paintTile/paintWall and paintCoatTile/paintCoatWall, 44727-44882.
  // MessageBuffer 63/64 treats every nonzero mode as coating and broadcasts even a no-op.
  private WorldMutationResult ApplyPaintOrCoating(
      ulong worldEpoch, TileCoordinate coordinate, byte paintOrCoating, byte coatingMode,
      bool wall) {
    lock (_gate) {
      if (!IsCurrentWorld(worldEpoch)) {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }
      if (!_header!.Geometry.Contains(coordinate)) {
        return WorldMutationResult.Rejected($"Tile coordinate {coordinate} is outside the world.");
      }
      if (coatingMode == 0 && paintOrCoating > 31) {
        return WorldMutationResult.Rejected("The paint color exceeds the supported 5-bit range.");
      }

      int index = GetTileIndex(coordinate);
      WorldTile current = _tiles![index];
      WorldTile next = current;
      bool hasSurface = wall ? current.Wall != 0 : current.Active;
      if (hasSurface) {
        if (coatingMode == 0) {
          next = wall
              ? current with { WallColor = paintOrCoating }
              : current with { TileColor = paintOrCoating };
        } else {
          const byte fullbrightMask = 0x80;
          switch (paintOrCoating) {
            case 0:
              next = wall
                  ? current with { WallHeader = (byte)(current.WallHeader & ~fullbrightMask),
                      TileHeader3 = (byte)(current.TileHeader3 & ~0x40) }
                  : current with { TileHeader3 = (byte)(current.TileHeader3 & ~0xA0) };
              break;
            case 1:
              next = wall
                  ? current with { WallHeader = (byte)(current.WallHeader | fullbrightMask) }
                  : current with { TileHeader3 = (byte)(current.TileHeader3 | fullbrightMask) };
              break;
            case 2:
              next = wall
                  ? current with { TileHeader3 = (byte)(current.TileHeader3 | 0x40) }
                  : current with { TileHeader3 = (byte)(current.TileHeader3 | 0x20) };
              break;
            default:
              return WorldMutationResult.Rejected("The coating id is outside the Version4 range.");
          }
        }
      }

      if (next == current) {
        return WorldMutationResult.Success(
            CurrentVersion, Array.Empty<TileCoordinate>(), Array.Empty<WorldSectionCoordinate>(),
            producesImmediateBroadcast: true);
      }
      if (_commitVersion == long.MaxValue) {
        return WorldMutationResult.Rejected("The world commit version is exhausted.");
      }

      CommitVersion version = NextVersion();
      var changedSectionSet = new HashSet<WorldSectionCoordinate>();
      AddFramingSections(changedSectionSet, coordinate, _header.Geometry);
      WorldSectionCoordinate[] changedSections = changedSectionSet.ToArray();
      Dictionary<WorldSectionCoordinate, CommitVersion> nextDirtySections =
          WithDirtySections(_dirtySections, changedSections, version);
      WorldMutationResult result = WorldMutationResult.Success(
          version, [coordinate], changedSections, producesImmediateBroadcast: true);

      // Finish allocations before publication. Preserve frames, liquid work and liquid dirty.
      _tiles![index] = next;
      _liquidSimulation.RebasePanicTileAppearance(index, next);
      _dirtySections = nextDirtySections;
      _commitVersion = version.Value;
      return result;
    }
  }
}
