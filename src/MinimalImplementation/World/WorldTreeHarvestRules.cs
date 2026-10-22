namespace MinimalImplementation.Server.World;

/// <summary>Frame and support checks for ordinary type-5 forest trees.</summary>
public static class WorldTreeHarvestRules {
  public const int WoodItemType = 9;
  private const int WoodPerTreeTile = 1;

  private const int MaximumTreeTiles = 128;

  public static int GetWoodDropStack(int treeTileCount, int bonusWoodDropCount) {
    if (treeTileCount is <= 0 or > MaximumTreeTiles) {
      throw new ArgumentOutOfRangeException(nameof(treeTileCount));
    }
    if (bonusWoodDropCount < 0 || bonusWoodDropCount > treeTileCount) {
      throw new ArgumentOutOfRangeException(nameof(bonusWoodDropCount));
    }
    return checked(treeTileCount * WoodPerTreeTile + bonusWoodDropCount);
  }

  public static bool TryCollectOrdinaryTree(WorldSpatialSnapshot snapshot,
      TileCoordinate target, out IReadOnlyList<TileCoordinate> treeTiles,
      out string rejection) {
    ArgumentNullException.ThrowIfNull(snapshot);
    treeTiles = Array.Empty<TileCoordinate>();
    rejection = "The requested tile is not part of a supported ordinary tree.";
    if (!snapshot.Header.Geometry.Contains(target)) {
      rejection = "The tree coordinate is outside the world.";
      return false;
    }

    WorldTile targetTile = snapshot.GetTile(target);
    if (!targetTile.Active || targetTile.Type != 5 || !HasOrdinaryTreeFrame(targetTile)) {
      rejection = "The requested tile does not have a supported ordinary tree frame.";
      return false;
    }

    var pending = new Stack<TileCoordinate>();
    var component = new HashSet<TileCoordinate>();
    pending.Push(target);
    while (pending.Count > 0) {
      TileCoordinate coordinate = pending.Pop();
      if (!Contains(snapshot.Bounds, coordinate)) {
        continue;
      }
      WorldTile tile = snapshot.GetTile(coordinate);
      if (!tile.Active || tile.Type != 5 || !component.Add(coordinate)) {
        continue;
      }
      if (!HasOrdinaryTreeFrame(tile)) {
        rejection = "The tree contains an unsupported type-5 frame.";
        return false;
      }
      if (component.Count > MaximumTreeTiles) {
        rejection = "The type-5 tree exceeds the supported structure bound.";
        return false;
      }
      if (IsOnBoundary(snapshot.Bounds, coordinate)) {
        rejection = "The captured tree reaches the structure scan boundary.";
        return false;
      }

      for (int offsetY = -1; offsetY <= 1; offsetY++) {
        for (int offsetX = -1; offsetX <= 1; offsetX++) {
          if (offsetX == 0 && offsetY == 0) {
            continue;
          }
          pending.Push(new TileCoordinate(coordinate.X + offsetX,
            coordinate.Y + offsetY));
        }
      }
    }

    if (!TryFindOrdinaryForestRoot(snapshot, target, targetTile, component,
        out TileCoordinate root, out rejection)) {
      return false;
    }

    int trunkHeight = 0;
    for (int y = root.Y; y >= snapshot.Bounds.OriginY; y--) {
      if (!component.Contains(new TileCoordinate(root.X, y))) {
        break;
      }
      trunkHeight++;
    }
    if (trunkHeight < 5) {
      rejection = "The type-5 structure does not contain a supported tree trunk.";
      return false;
    }

    treeTiles = component.OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X).ToArray();
    rejection = string.Empty;
    return true;
  }

  private static bool HasOrdinaryTreeFrame(WorldTile tile) {
    return tile.FrameX is >= 0 and <= 154 && tile.FrameX % 22 == 0 &&
      tile.FrameY is >= 0 and <= 242 && tile.FrameY % 22 == 0;
  }

  private static bool TryFindOrdinaryForestRoot(WorldSpatialSnapshot snapshot,
      TileCoordinate target, WorldTile targetTile, HashSet<TileCoordinate> component,
      out TileCoordinate root, out string rejection) {
    root = default;
    int rootColumn = target.X;
    int frameColumn = targetTile.FrameX / 22;
    int frameRow = targetTile.FrameY / 22;
    if (frameColumn == 3 && frameRow <= 2 || frameColumn == 2 && frameRow >= 9) {
      rootColumn++;
    } else if (frameColumn == 4 && frameRow is >= 3 and <= 5 ||
        frameColumn == 1 && frameRow is >= 6 and <= 8 ||
        frameColumn == 3 && frameRow >= 9) {
      rootColumn--;
    }

    WorldSectionBounds bounds = snapshot.Bounds;
    int y = target.Y;
    bool sawTrunk = false;
    while (y < bounds.OriginY + bounds.Height - 1) {
      var coordinate = new TileCoordinate(rootColumn, y);
      if (!Contains(bounds, coordinate)) {
        rejection = "The frame-derived tree trunk leaves the captured structure bounds.";
        return false;
      }

      WorldTile tile = snapshot.GetTile(coordinate);
      if (!tile.Active || tile.Type == 5) {
        sawTrunk |= tile.Active && tile.Type == 5;
        y++;
        continue;
      }

      if (!sawTrunk || !component.Contains(new TileCoordinate(rootColumn, y - 1))) {
        rejection = "The frame-derived tree trunk does not connect to its root.";
        return false;
      }
      if (!tile.Inactive && tile.Type is 2 or 477) {
        root = new TileCoordinate(rootColumn, y - 1);
        rejection = string.Empty;
        return true;
      }

      rejection = $"The ordinary tree is rooted on unsupported tile type {tile.Type}.";
      return false;
    }

    rejection = "The frame-derived tree trunk has no supported forest root.";
    return false;
  }

  private static bool Contains(WorldSectionBounds bounds, TileCoordinate coordinate) {
    return coordinate.X >= bounds.OriginX && coordinate.Y >= bounds.OriginY &&
      coordinate.X < bounds.OriginX + bounds.Width &&
      coordinate.Y < bounds.OriginY + bounds.Height;
  }

  private static bool IsOnBoundary(WorldSectionBounds bounds, TileCoordinate coordinate) {
    return coordinate.X == bounds.OriginX || coordinate.Y == bounds.OriginY ||
      coordinate.X == bounds.OriginX + bounds.Width - 1 ||
      coordinate.Y == bounds.OriginY + bounds.Height - 1;
  }
}
