namespace MinimalImplementation.Server.World;

/// <summary>Packet 19 operations. Values intentionally match the Version4 wire action.</summary>
public enum WorldDoorOperation : byte
{
  OpenDoor = 0,
  CloseDoor = 1,
  OpenTrapdoor = 2,
  CloseTrapdoor = 3,
  OpenTallGate = 4,
  CloseTallGate = 5
}

public sealed partial class WorldOwner
{
  /// <summary>Byte-oriented adapter for the Packet 19 dispatcher.</summary>
  public WorldMutationResult ApplyToggleDoor(
      ulong worldEpoch,
      byte action,
      TileCoordinate coordinate,
      byte direction,
      bool forced = true)
  {
    if (!Enum.IsDefined(typeof(WorldDoorOperation), action))
    {
      return WorldMutationResult.Rejected("The door operation is outside the Version4 range.");
    }

    return ApplyDoorOperation(
        worldEpoch,
        (WorldDoorOperation)action,
        coordinate,
        direction == 0 ? -1 : 1,
        forced);
  }

  /// <summary>
  /// Applies a door, trapdoor or tall-gate transition as one tile-owner commit. The transition
  /// returns a normal tile mutation so the caller can send packet 19 after the commit.
  /// </summary>
  public WorldMutationResult ApplyDoorOperation(
      ulong worldEpoch,
      WorldDoorOperation operation,
      TileCoordinate coordinate,
      int direction = 1,
      bool forced = true)
  {
    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      return operation switch
      {
        WorldDoorOperation.OpenDoor => ApplyOpenDoorLocked(worldEpoch, coordinate, direction),
        WorldDoorOperation.CloseDoor => ApplyCloseDoorLocked(worldEpoch, coordinate),
        WorldDoorOperation.OpenTrapdoor => ApplyTrapdoorLocked(
            worldEpoch, coordinate, open: true, playerAbove: direction > 0),
        WorldDoorOperation.CloseTrapdoor => ApplyTrapdoorLocked(
            worldEpoch, coordinate, open: false, playerAbove: direction > 0),
        WorldDoorOperation.OpenTallGate => ApplyTallGateLocked(worldEpoch, coordinate, open: true, forced),
        WorldDoorOperation.CloseTallGate => ApplyTallGateLocked(worldEpoch, coordinate, open: false, forced),
        _ => WorldMutationResult.Rejected("The door operation is unsupported.")
      };
    }
  }

  private WorldMutationResult ApplyOpenDoorLocked(
      ulong worldEpoch, TileCoordinate coordinate, int direction)
  {
    if (direction is not (-1 or 1))
    {
      return WorldMutationResult.Rejected("Door direction must be -1 or 1.");
    }

    if (!TryGetTile(coordinate, out WorldTile tile) || !tile.Active || tile.Type != 10)
    {
      return WorldMutationResult.Rejected("The target is not a closed door.");
    }

    int topY = coordinate.Y - PositiveFrameRemainder(tile.FrameY, 54) / 18;
    int baseX = direction < 0 ? coordinate.X - 1 : coordinate.X;
    if (!ContainsFootprint(baseX, topY, 2, 3))
    {
      return WorldMutationResult.Rejected("The open door footprint is outside the world.");
    }

    var patches = new List<WorldTilePatch>(6);
    for (int row = 0; row < 3; row++)
    {
      TileCoordinate closedCoordinate = new(coordinate.X, topY + row);
      WorldTile closed = _tiles![GetTileIndex(closedCoordinate)];
      if (!closed.Active || closed.Type != 10)
      {
        return WorldMutationResult.Rejected("The closed door footprint is incomplete.");
      }

      for (int column = 0; column < 2; column++)
      {
        TileCoordinate target = new(baseX + column, topY + row);
        WorldTile existing = _tiles[GetTileIndex(target)];
        if (target != closedCoordinate && existing.Active)
        {
          return WorldMutationResult.Rejected("The door opening is occupied.");
        }

        WorldTile opened = existing with {
          Active = true,
          Type = 11,
          FrameX = (short)((direction < 0 ? 36 : 0) + column * 18),
          FrameY = (short)(row * 18),
          TileColor = closed.TileColor,
          TileHeader3 = closed.TileHeader3,
          HalfBlock = false,
          Slope = 0,
          Inactive = false
        };
        patches.Add(new WorldTilePatch(target, ToDelta(opened, existing)));
      }
    }

    return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast: true);
  }

  private WorldMutationResult ApplyCloseDoorLocked(ulong worldEpoch, TileCoordinate coordinate)
  {
    if (!TryGetTile(coordinate, out WorldTile tile) || !tile.Active || tile.Type != 11)
    {
      return WorldMutationResult.Rejected("The target is not an open door.");
    }

    int frameColumn = PositiveFrameRemainder(tile.FrameX, 72) / 18;
    int baseX = frameColumn is 1 or 3 ? coordinate.X - 1 : coordinate.X;
    int topY = coordinate.Y - PositiveFrameRemainder(tile.FrameY, 54) / 18;
    if (!ContainsFootprint(baseX, topY, 2, 3))
    {
      return WorldMutationResult.Rejected("The open door footprint is outside the world.");
    }

    var patches = new List<WorldTilePatch>(6);
    for (int row = 0; row < 3; row++)
    {
      for (int column = 0; column < 2; column++)
      {
        TileCoordinate target = new(baseX + column, topY + row);
        WorldTile existing = _tiles![GetTileIndex(target)];
        if (!existing.Active || existing.Type != 11)
        {
          return WorldMutationResult.Rejected("The open door footprint is incomplete.");
        }

        WorldTile closed = column == 0
            ? existing with {
                Active = true,
                Type = 10,
                FrameX = (short)(frameColumn >= 2 ? 36 : 0),
                FrameY = (short)(row * 18),
                HalfBlock = false,
                Slope = 0,
                Inactive = false
              }
            : ClearTile(existing);
        patches.Add(new WorldTilePatch(target, ToDelta(closed, existing)));
      }
    }

    return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast: true);
  }

  private WorldMutationResult ApplyTrapdoorLocked(
      ulong worldEpoch, TileCoordinate coordinate, bool open, bool playerAbove)
  {
    if (!TryGetTile(coordinate, out WorldTile tile))
    {
      return WorldMutationResult.Rejected("The trapdoor coordinate is outside the world.");
    }

    if (open && tile.Type == 386)
    {
      int topY = coordinate.Y - PositiveFrameRemainder(tile.FrameY, 36) / 18;
      int leftX = coordinate.X - PositiveFrameRemainder(tile.FrameX, 36) / 18;
      if (!ContainsFootprint(leftX, topY, 2, 2))
      {
        return WorldMutationResult.Rejected("The trapdoor footprint is outside the world.");
      }

      var patches = new List<WorldTilePatch>(4);
      for (int column = 0; column < 2; column++)
      {
        TileCoordinate closed = new(leftX + column, topY);
        TileCoordinate opened = new(leftX + column, topY + 1);
        WorldTile closedTile = _tiles![GetTileIndex(closed)];
        WorldTile openedTile = _tiles[GetTileIndex(opened)];
        if (!closedTile.Active || closedTile.Type != 386 || openedTile.Active)
        {
          return WorldMutationResult.Rejected("The closed trapdoor footprint is blocked.");
        }

        WorldTile openedValue = openedTile with {
          Active = true,
          Type = 387,
          FrameX = (short)(column * 18),
          FrameY = (short)(PositiveFrameRemainder(tile.FrameY, 36)),
          TileColor = closedTile.TileColor,
          TileHeader3 = closedTile.TileHeader3,
          HalfBlock = false,
          Slope = 0
        };
        patches.Add(new WorldTilePatch(closed, ToDelta(ClearTile(closedTile), closedTile)));
        patches.Add(new WorldTilePatch(opened, ToDelta(openedValue, openedTile)));
      }

      return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast: true);
    }

    if (!open && tile.Type == 387)
    {
      int leftX = coordinate.X - PositiveFrameRemainder(tile.FrameX, 36) / 18;
      int topY = coordinate.Y;
      int sourceY = playerAbove ? topY : topY - 1;
      if (!ContainsFootprint(leftX, sourceY, 2, 2))
      {
        return WorldMutationResult.Rejected("The open trapdoor footprint is outside the world.");
      }

      var patches = new List<WorldTilePatch>(4);
      for (int column = 0; column < 2; column++)
      {
        TileCoordinate opened = new(leftX + column, sourceY);
        TileCoordinate closed = new(leftX + column, sourceY + (playerAbove ? 1 : -1));
        WorldTile openedTile = _tiles![GetTileIndex(opened)];
        WorldTile closedTile = _tiles[GetTileIndex(closed)];
        if (!openedTile.Active || openedTile.Type != 387 || closedTile.Active)
        {
          return WorldMutationResult.Rejected("The open trapdoor footprint is blocked.");
        }

        WorldTile closedValue = closedTile with {
          Active = true,
          Type = 386,
          FrameX = (short)(column * 18),
          FrameY = (short)(PositiveFrameRemainder(tile.FrameY, 36)),
          TileColor = openedTile.TileColor,
          TileHeader3 = openedTile.TileHeader3,
          HalfBlock = false,
          Slope = 0
        };
        patches.Add(new WorldTilePatch(opened, ToDelta(ClearTile(openedTile), openedTile)));
        patches.Add(new WorldTilePatch(closed, ToDelta(closedValue, closedTile)));
      }

      return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast: true);
    }

    return WorldMutationResult.Rejected("The target is not a trapdoor in the requested state.");
  }

  private WorldMutationResult ApplyTallGateLocked(
      ulong worldEpoch, TileCoordinate coordinate, bool open, bool forced)
  {
    _ = forced;
    ushort expected = open ? (ushort)389 : (ushort)388;
    ushort replacement = open ? (ushort)388 : (ushort)389;
    if (!TryGetTile(coordinate, out WorldTile tile) || !tile.Active || tile.Type != expected)
    {
      return WorldMutationResult.Rejected("The target is not a tall gate in the requested state.");
    }

    int topY = coordinate.Y;
    while (topY > 0 && _tiles![GetTileIndex(new TileCoordinate(coordinate.X, topY - 1))].Active &&
        _tiles[GetTileIndex(new TileCoordinate(coordinate.X, topY - 1))].Type == expected)
    {
      topY--;
    }

    int height = 0;
    while (topY + height < _header!.Geometry.Height &&
        _tiles[GetTileIndex(new TileCoordinate(coordinate.X, topY + height))].Active &&
        _tiles[GetTileIndex(new TileCoordinate(coordinate.X, topY + height))].Type == expected)
    {
      height++;
    }

    if (height == 0 || height > 8)
    {
      return WorldMutationResult.Rejected("The tall gate footprint is invalid.");
    }

    var patches = new List<WorldTilePatch>(height);
    for (int row = 0; row < height; row++)
    {
      TileCoordinate target = new(coordinate.X, topY + row);
      WorldTile current = _tiles[GetTileIndex(target)];
      WorldTile next = current with { Type = replacement, Active = true };
      patches.Add(new WorldTilePatch(target, ToDelta(next, current)));
    }

    return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast: true);
  }

  private bool TryGetTile(TileCoordinate coordinate, out WorldTile tile)
  {
    if (_header is null || _tiles is null || !_header.Geometry.Contains(coordinate))
    {
      tile = default;
      return false;
    }

    tile = _tiles[GetTileIndex(coordinate)];
    return true;
  }

  private bool ContainsFootprint(int originX, int originY, int width, int height)
  {
    return _header is not null && originX >= 0 && originY >= 0 &&
        (long)originX + width <= _header.Geometry.Width &&
        (long)originY + height <= _header.Geometry.Height;
  }

  private static int PositiveFrameRemainder(short value, int divisor)
  {
    int remainder = value % divisor;
    return remainder < 0 ? remainder + divisor : remainder;
  }
}
