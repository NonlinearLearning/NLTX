namespace MinimalImplementation.Server.World;

public sealed partial class WorldOwner
{
  /// <summary>
  /// Recomputes the server-visible frame invariant for a bounded tile neighborhood. Type-specific
  /// frame tables remain in WorldFacts and supplied frame-important values are preserved; ordinary
  /// tiles are normalized to the Version4 sentinel frame (-1,-1). All neighboring sections are
  /// returned dirty so a protocol adapter can request a complete frame projection.
  /// </summary>
  public WorldMutationResult ApplyTileFraming(
      ulong worldEpoch,
      IReadOnlyCollection<TileCoordinate> anchors,
      bool producesImmediateBroadcast = true)
  {
    ArgumentNullException.ThrowIfNull(anchors);
    if (anchors.Count > 65_536)
    {
      return WorldMutationResult.Rejected("The tile framing request exceeds its bounded area.");
    }

    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      var coordinates = new HashSet<TileCoordinate>();
      foreach (TileCoordinate anchor in anchors)
      {
        if (!_header!.Geometry.Contains(anchor))
        {
          return WorldMutationResult.Rejected("A framing anchor is outside the world.");
        }

        for (int y = anchor.Y - 1; y <= anchor.Y + 1; y++)
        {
          for (int x = anchor.X - 1; x <= anchor.X + 1; x++)
          {
            TileCoordinate coordinate = new(x, y);
            if (_header.Geometry.Contains(coordinate))
            {
              coordinates.Add(coordinate);
            }
          }
        }
      }

      var patches = new List<WorldTilePatch>(coordinates.Count);
      foreach (TileCoordinate coordinate in coordinates)
      {
        WorldTile current = _tiles![GetTileIndex(coordinate)];
        WorldTile next = current;
        if (!current.Active)
        {
          next = current with { FrameX = 0, FrameY = 0, Slope = 0, HalfBlock = false };
        }
        else if (!_facts.FrameImportant[current.Type])
        {
          next = current with { FrameX = -1, FrameY = -1 };
        }

        if (next != current)
        {
          patches.Add(new WorldTilePatch(coordinate, ToDelta(next, current)));
        }
      }

      if (patches.Count == 0)
      {
        var sections = new HashSet<WorldSectionCoordinate>();
        foreach (TileCoordinate coordinate in coordinates)
        {
          AddFramingSections(sections, coordinate, _header.Geometry);
        }

        return WorldMutationResult.Success(
            CurrentVersion,
            Array.Empty<TileCoordinate>(),
            sections,
            producesImmediateBroadcast);
      }

      return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast);
    }
  }
}
