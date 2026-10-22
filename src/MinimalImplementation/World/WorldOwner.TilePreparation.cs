using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

public sealed partial class WorldOwner {
  /// <summary>
  /// Stages actual tile patches and liquid/dirty effects without mutating the published world.
  /// expectedVersion must come from the snapshot used to check the TE placement/destruction.
  /// </summary>
  public bool TryPrepareTilePatches(
      ulong worldEpoch, CommitVersion expectedVersion, IReadOnlyList<WorldTilePatch> patches,
      bool producesImmediateBroadcast, out WorldTileMutationPreparation? preparation,
      out WorldValidationResult validation) {
    ArgumentNullException.ThrowIfNull(patches);
    lock (_gate) {
      preparation = null;
      if (!IsCurrentWorld(worldEpoch) || expectedVersion != CurrentVersion) {
        validation = WorldValidationResult.Invalid(["The world epoch/version is stale."]);
        return false;
      }
      const int maximumPreparedTiles = 262_144;
      if (patches.Count > maximumPreparedTiles) {
        validation = WorldValidationResult.Invalid(["The prepared tile batch exceeds its budget."]);
        return false;
      }
      var candidate = new Dictionary<int, WorldTile>();
      foreach (WorldTilePatch patch in patches) {
        if (!_header!.Geometry.Contains(patch.Coordinate)) {
          validation = WorldValidationResult.Invalid(["A tile patch is outside the world."]);
          return false;
        }
        int index = GetTileIndex(patch.Coordinate);
        WorldTile current = candidate.TryGetValue(index, out WorldTile staged)
            ? staged : _tiles![index];
        if (!TryStageTileDelta(current, patch.ToDelta(), out WorldTile next)) {
          validation = WorldValidationResult.Invalid(["A tile patch contains invalid facts."]);
          return false;
        }
        candidate[index] = next;
      }
      var updates = candidate.Where(pair => _tiles![pair.Key] != pair.Value)
          .ToDictionary(pair => pair.Key, pair => pair.Value);
      if (updates.Count > 0 && _commitVersion == long.MaxValue) {
        validation = WorldValidationResult.Invalid(["The world commit version is exhausted."]);
        return false;
      }
      CommitVersion version = updates.Count == 0 ? CurrentVersion : NextVersion();
      TileCoordinate[] changedTiles = updates.Keys.Select(GetCoordinate).ToArray();
      var changedSectionSet = new HashSet<WorldSectionCoordinate>();
      foreach (TileCoordinate coordinate in changedTiles)
      {
        AddFramingSections(changedSectionSet, coordinate, _header!.Geometry);
      }
      WorldSectionCoordinate[] changedSections = changedSectionSet.ToArray();
      var expectedDirtySections = new Dictionary<WorldSectionCoordinate, CommitVersion>(
          _dirtySections);
      var expectedDirtyLiquids = new Dictionary<TileCoordinate, CommitVersion>(_dirtyLiquidTiles);
      Dictionary<WorldSectionCoordinate, CommitVersion> nextDirtySections =
          WithDirtySections(expectedDirtySections, changedSections, version);
      var nextDirtyLiquids = new Dictionary<TileCoordinate, CommitVersion>(expectedDirtyLiquids);
      LiquidSimulation sourceLiquid = _liquidSimulation;
      LiquidSimulation expectedLiquid = sourceLiquid.Clone();
      LiquidSimulation nextLiquid = expectedLiquid.Clone();
      foreach (var pair in updates) {
        TileCoordinate coordinate = GetCoordinate(pair.Key);
        if (!nextLiquid.EnqueueAround(coordinate, _header!.Geometry)) {
          validation = WorldValidationResult.Invalid(["The prepared liquid work queue is full."]);
          return false;
        }
        WorldTile previous = _tiles![pair.Key];
        if (previous.LiquidAmount != pair.Value.LiquidAmount ||
            previous.LiquidType != pair.Value.LiquidType) {
          nextDirtyLiquids[coordinate] = version;
        }
      }
      WorldMutationResult result = WorldMutationResult.Success(
          version, changedTiles, changedSections, producesImmediateBroadcast);
      preparation = new WorldTileMutationPreparation(
          worldEpoch, expectedVersion, result,
          updates.Select(pair => new KeyValuePair<TileCoordinate, WorldTile>(
              GetCoordinate(pair.Key), pair.Value)).ToArray(),
          ValidatePreparation, CommitPreparation);
      validation = WorldValidationResult.Valid();
      return true;

      WorldValidationResult ValidatePreparation() {
        lock (_gate) {
          if (!IsCurrentWorld(worldEpoch) || CurrentVersion != expectedVersion) {
            return WorldValidationResult.Invalid(["The prepared world revision is stale."]);
          }
          if (!ReferenceEquals(sourceLiquid, _liquidSimulation) ||
              !sourceLiquid.HasSameState(expectedLiquid) ||
              !SameDirtyEntries(_dirtySections, expectedDirtySections) ||
              !SameDirtyEntries(_dirtyLiquidTiles, expectedDirtyLiquids)) {
            return WorldValidationResult.Invalid(["The liquid queue or dirty state changed."]);
          }
          return WorldValidationResult.Valid();
        }
      }

      void CommitPreparation() {
        lock (_gate) {
          if (updates.Count == 0) {
            return;
          }
          foreach (var pair in updates) {
            _tiles![pair.Key] = pair.Value;
          }
          _liquidSimulation = nextLiquid;
          _liquidSimulation.InvalidatePanicContinuation();
          _dirtySections = nextDirtySections;
          _dirtyLiquidTiles = nextDirtyLiquids;
          _commitVersion = version.Value;
        }
      }
    }
  }

  private bool TryStageTileDelta(WorldTile current, WorldTileDelta delta, out WorldTile next) {
    next = delta.Apply(current);
    if (!_facts.IsValidTileType(next.Type) || next.Wall >= WorldFacts.WallCount ||
        !Enum.IsDefined(next.LiquidType) ||
        (((byte)next.Wires) & 0xf0) != 0 || next.Slope > 4 || next.TileColor > 31 ||
        next.WallColor > 31 || (next.TileHeader3 & 0x1f) != 0 ||
        (next.WallHeader & 0x7f) != 0) {
      return false;
    }
    if (next.Active && !_facts.FrameImportant[next.Type]) {
      next = next with { FrameX = -1, FrameY = -1 };
    }
    return true;
  }

  private static bool SameDirtyEntries<TKey>(
      Dictionary<TKey, CommitVersion> current, Dictionary<TKey, CommitVersion> expected)
      where TKey : notnull {
    return current.Count == expected.Count && expected.All(pair =>
        current.TryGetValue(pair.Key, out CommitVersion value) && value == pair.Value);
  }
}
