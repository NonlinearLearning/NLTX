using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>An evolving private tile overlay; it never commits into the world.</summary>
internal sealed class WiringTileOperation
{
  private static readonly TileWireMask[] Colors =
    [TileWireMask.Red, TileWireMask.Blue, TileWireMask.Green, TileWireMask.Yellow];

  private readonly WorldSnapshot _world;
  private readonly MechanismFactsCatalog _facts;
  private readonly Dictionary<TileCoordinate, WorldTile> _tiles = [];
  private readonly HashSet<TileCoordinate> _skip = [];
  private readonly Dictionary<TileCoordinate, byte> _pixelTriggers = [];
  private readonly List<TileCoordinate> _inPumps = [];
  private readonly List<TileCoordinate> _outPumps = [];
  private readonly List<WiringTileSquareEffect> _tileSquareEffects = [];

  internal WiringTileOperation(WorldSnapshot world, MechanismFactsCatalog facts)
  {
    _world = world;
    _facts = facts;
  }

  internal HashSet<TileCoordinate> Visited { get; } = [];

  internal IReadOnlyList<WiringTileSquareEffect> TileSquareEffects => _tileSquareEffects;

  internal string? Rejection { get; private set; }

  private bool Contains(TileCoordinate coordinate) =>
    _world.Header.Geometry.Contains(coordinate);

  internal WorldTile Read(TileCoordinate coordinate) =>
    _tiles.TryGetValue(coordinate, out WorldTile tile) ? tile : _world.GetTile(coordinate);

  internal void Write(TileCoordinate coordinate, WorldTileDelta delta)
  {
    if (!Contains(coordinate))
    {
      Rejection = "A wired device footprint extends outside the world.";
      return;
    }
    _tiles[coordinate] = delta.Apply(Read(coordinate));
  }

  internal IReadOnlyDictionary<TileCoordinate, WorldTileDelta> CaptureDeltas()
  {
    return _tiles.Where(pair => pair.Value != _world.GetTile(pair.Key))
      .ToDictionary(pair => pair.Key, pair =>
      {
        WorldTile tile = pair.Value;
        return new WorldTileDelta(Type: tile.Type, FrameX: tile.FrameX, FrameY: tile.FrameY,
          LiquidAmount: tile.LiquidAmount, LiquidType: tile.LiquidType);
      });
  }

  internal void Trip(TileCoordinate topLeft, int width, int height,
    IReadOnlyList<TileCoordinate> directions)
  {
    if (Rejection is not null)
    {
      return;
    }
    _pixelTriggers.Clear();
    foreach (TileWireMask color in Colors)
    {
      _skip.Clear();
      _inPumps.Clear();
      _outPumps.Clear();
      var queue = new Queue<(TileCoordinate Coordinate, int Direction)>();
      var toProcess = new Dictionary<TileCoordinate, int>();
      for (int x = topLeft.X; x < topLeft.X + width; x++)
      {
        for (int y = topLeft.Y; y < topLeft.Y + height; y++)
        {
          var seed = new TileCoordinate(x, y);
          if (!Contains(seed))
          {
            Rejection = "The switch footprint extends outside the world.";
            return;
          }
          if ((Read(seed).Wires & color) == 0)
          {
            continue;
          }
          queue.Enqueue((seed, 0));
          toProcess.Add(seed, 4);
          _skip.Add(seed);
        }
      }

      while (queue.TryDequeue(out var entry))
      {
        TileCoordinate coordinate = entry.Coordinate;
        Visited.Add(coordinate);
        if (!_skip.Contains(coordinate))
        {
          HitDevice(coordinate, color);
          if (Rejection is not null)
          {
            return;
          }
        }
        WorldTile tile = Read(coordinate);
        for (int direction = 0; direction < 4; direction++)
        {
          TileCoordinate offset = directions[direction];
          var neighbor = new TileCoordinate(coordinate.X + offset.X, coordinate.Y + offset.Y);
          if (neighbor.X < 2 || neighbor.Y < 2 ||
              neighbor.X >= _world.Header.Geometry.Width - 2 ||
              neighbor.Y >= _world.Header.Geometry.Height - 2 ||
              !AllowsDirection(tile, entry.Direction, direction))
          {
            continue;
          }
          if (tile.Type == 445)
          {
            _pixelTriggers.TryGetValue(coordinate, out byte previous);
            _pixelTriggers[coordinate] = (byte)(previous | (direction <= 1 ? 2 : 1));
          }
          WorldTile next = Read(neighbor);
          if ((next.Wires & color) == 0)
          {
            continue;
          }
          if (toProcess.TryGetValue(neighbor, out int remaining))
          {
            if (remaining == 1)
            {
              toProcess.Remove(neighbor);
            }
            else
            {
              toProcess[neighbor] = remaining - 1;
            }
            continue;
          }
          queue.Enqueue((neighbor, direction));
          if (next.Type is not (424 or 445))
          {
            toProcess.Add(neighbor, 3);
          }
        }
      }
      TransferLiquid();
    }
    foreach ((TileCoordinate coordinate, byte axes) in _pixelTriggers)
    {
      if (axes == 3)
      {
        Write(coordinate, new WorldTileDelta(
          FrameX: Read(coordinate).FrameX == 18 ? (short)0 : (short)18));
        CaptureTileSquare(coordinate, 1, 1);
      }
    }
  }

  private static bool AllowsDirection(WorldTile tile, int incoming, int outgoing)
  {
    if (tile.Type == 445)
    {
      return incoming == outgoing;
    }
    if (tile.Type != 424)
    {
      return true;
    }
    return (tile.FrameX / 18) switch
    {
      0 => incoming == outgoing,
      1 => (incoming, outgoing) is (0, 3) or (3, 0) or (1, 2) or (2, 1),
      2 => (incoming, outgoing) is (0, 2) or (2, 0) or (1, 3) or (3, 1),
      _ => true
    };
  }

  private void HitDevice(TileCoordinate coordinate, TileWireMask color)
  {
    WorldTile tile = Read(coordinate);
    if (tile.Actuator)
    {
      Rejection = "Actuation requires authoritative CanKillTile and progression facts.";
      return;
    }
    if (!tile.Active)
    {
      return;
    }
    int x = coordinate.X;
    int y = coordinate.Y;
    TileCoordinate anchor;
    WorldTile anchorTile;
    switch (tile.Type)
    {
      case 15 when tile.FrameY / 40 is 1 or 20:
        Rejection = $"Wired tile {tile.Type} requires its authoritative device effect owner.";
        return;
      case 410:
      case 480:
      case 509:
      case 657:
      case 658:
      case 720:
      case 721:
      case 725:
      case 733:
        SwitchMonolith(coordinate, tile);
        return;
      case 35:
      case 139:
        SwitchMb(coordinate, tile);
        return;
      case 207:
        SwitchFountain(coordinate, tile);
        return;
      case 144:
        Rejection = "Timer 144 requires its authoritative timed activation owner.";
        return;
      case 421:
      case 422:
        Write(coordinate, new WorldTileDelta(Type: tile.Type == 421 ? (ushort)422 : (ushort)421));
        CaptureTileSquare(coordinate, 1, 1);
        return;
      case 419:
        // Logic lamps use the same 18-pixel frame progression as the source wiring pass.
        // Frame 36 is the terminal state and intentionally does not wrap to frame 0.
        short frameDelta = tile.FrameX >= 18 ? (short)-18 : (short)18;
        if (tile.FrameX == 36)
        {
          frameDelta = 0;
        }
        if (frameDelta != 0)
        {
          Write(coordinate, new WorldTileDelta(FrameX: (short)(tile.FrameX + frameDelta)));
          CaptureTileSquare(coordinate, 1, 1);
        }
        return;
      case >= 255 and <= 268:
        Write(coordinate, new WorldTileDelta(
          Type: (ushort)(tile.Type >= 262 ? tile.Type - 7 : tile.Type + 7)));
        CaptureTileSquare(coordinate, 1, 1);
        return;
      case 546:
      case 557:
        Write(coordinate, new WorldTileDelta(Type: tile.Type == 546 ? (ushort)557 : (ushort)546));
        CaptureTileSquare(coordinate, 1, 1);
        return;
      case 4:
        Write(coordinate, new WorldTileDelta(
          FrameX: (short)(tile.FrameX + (tile.FrameX < 66 ? 66 : -66))));
        CaptureTileSquare(coordinate, 1, 1);
        return;
      case 33:
      case 49:
      case 174:
      case 372:
      case 646:
        Write(coordinate, new WorldTileDelta(
          FrameX: (short)(tile.FrameX + (tile.FrameX > 0 ? -18 : 18))));
        CaptureTileSquare(new TileCoordinate(x - 1, y - 1), 3, 3);
        return;
      case 149:
        Write(coordinate, new WorldTileDelta(
          FrameX: (short)(tile.FrameX + (tile.FrameX >= 54 ? -54 : 54))));
        CaptureTileSquare(coordinate, 1, 1);
        return;
      case 429:
        int bit = color switch
        {
          TileWireMask.Red => 1,
          TileWireMask.Blue => 4,
          TileWireMask.Green => 2,
          TileWireMask.Yellow => 8,
          _ => throw new ArgumentOutOfRangeException(nameof(color))
        };
        Write(coordinate, new WorldTileDelta(
          FrameX: (short)(tile.FrameX + ((tile.FrameX / 18 & bit) == 0 ? 18 * bit : -18 * bit))));
        CaptureTileSquare(coordinate, 1, 1);
        return;
      case 406:
        anchor = Anchor(coordinate, tile, 3, 3);
        if (TryReadFootprint(anchor, 3, 3, out anchorTile))
        {
          ShiftFrames(anchor, 3, 3, 0, anchorTile.FrameY >= 108 ? -108 : 54);
        }
        return;
      case 452:
        anchor = Anchor(coordinate, tile, 3, 3);
        if (TryReadFootprint(anchor, 3, 3, out anchorTile))
        {
          ShiftFrames(anchor, 3, 3, anchorTile.FrameX >= 54 ? -54 : 54, 0);
        }
        return;
      case 411:
      case 565:
        anchor = Anchor(coordinate, tile, 2, 2);
        if (TryReadFootprint(anchor, 2, 2, out anchorTile))
        {
          ShiftFrames(anchor, 2, 2, anchorTile.FrameX >= 36 ? -36 : 36, 0);
        }
        return;
      case 42:
        anchor = new TileCoordinate(x, y - tile.FrameY / 18 % 2);
        ShiftFrames(anchor, 1, 2, tile.FrameX > 0 ? -18 : 18, 0);
        return;
      case 93:
        anchor = new TileCoordinate(x, y - tile.FrameY / 18 % 3);
        ShiftFrames(anchor, 1, 3, tile.FrameX > 0 ? -18 : 18, 0);
        return;
      case 92:
        anchor = new TileCoordinate(x, y - tile.FrameY / 18);
        ShiftFrames(anchor, 1, 6, tile.FrameX > 0 ? -18 : 18, 0);
        return;
      case 95:
      case 100:
      case 126:
      case 173:
      case 564:
        int column = tile.FrameX / 18;
        if (column > 1)
        {
          column -= 2;
        }
        anchor = new TileCoordinate(x - column, y - tile.FrameY / 18 % 2);
        if (TryReadFootprint(anchor, 2, 2, out anchorTile))
        {
          ShiftFrames(anchor, 2, 2, anchorTile.FrameX > 0 ? -36 : 36, 0);
        }
        return;
      case 593:
        ShiftFrames(coordinate, 1, 1, tile.FrameX != 0 ? -18 : 18, 0,
          animationType: (short)(tile.FrameX != 0 ? 3 : 4),
          animationTileType: tile.Type, animationSendCount: 2);
        return;
      case 594:
        int row = tile.FrameY / 18;
        while (row >= 2)
        {
          row -= 2;
        }
        column = tile.FrameX / 18;
        if (column > 1)
        {
          column -= 2;
        }
        anchor = new TileCoordinate(x - column, y - row);
        if (TryReadFootprint(anchor, 2, 2, out anchorTile))
        {
          ShiftFrames(anchor, 2, 2, anchorTile.FrameX != 0 ? -36 : 36, 0,
            animationType: (short)(anchorTile.FrameX != 0 ? 3 : 4),
            animationTileType: tile.Type, animationSendCount: 2);
        }
        return;
      case 34:
        column = tile.FrameX % 108 / 18;
        if (column > 2)
        {
          column -= 3;
        }
        anchor = new TileCoordinate(x - column, y - tile.FrameY / 18 % 3);
        if (TryReadFootprint(anchor, 3, 3, out anchorTile))
        {
          ShiftFrames(anchor, 3, 3, anchorTile.FrameX % 108 > 0 ? -54 : 54, 0);
        }
        return;
      case 405:
        anchor = Anchor(coordinate, tile, 3, 2);
        if (TryReadFootprint(anchor, 3, 2, out anchorTile))
        {
          ShiftFrames(anchor, 3, 2, anchorTile.FrameX >= 54 ? -54 : 54, 0);
        }
        return;
      case 215:
        anchor = Anchor(coordinate, tile, 3, 2);
        if (Matches(anchor, 3, 2, tile.Type))
        {
          ShiftFrames(anchor, 3, 2, 0, Read(anchor).FrameY >= 36 ? -36 : 36);
        }
        return;
      case 244:
        anchor = Anchor(coordinate, tile, 3, 3);
        if (TryReadFootprint(anchor, 3, 2, out anchorTile))
        {
          ShiftFrames(anchor, 3, 2, anchorTile.FrameX >= 54 ? -54 : 54, 0);
        }
        return;
      case 349:
        column = tile.FrameX / 18;
        while (column >= 2)
        {
          column -= 2;
        }
        anchor = new TileCoordinate(x - column, y - tile.FrameY / 18 % 3);
        if (TryReadFootprint(anchor, 2, 3, out anchorTile))
        {
          ShiftFrames(anchor, 2, 3, anchorTile.FrameX != 0 ? -216 : 216, 0,
            animationType: (short)(anchorTile.FrameX != 0 ? 1 : 0),
            animationTileType: tile.Type, animationSendCount: 1);
        }
        return;
      case 506:
        anchor = Anchor(coordinate, tile, 2, 3);
        if (Matches(anchor, 2, 3, tile.Type))
        {
          ShiftFrames(anchor, 2, 3, Read(anchor).FrameX >= 72 ? -72 : 72, 0);
        }
        return;
      case 142:
      case 143:
        column = tile.FrameX / 18;
        if (column > 1)
        {
          column -= 2;
        }
        anchor = new TileCoordinate(x - column, y - tile.FrameY / 18);
        AddPump(anchor, tile.Type == 142 ? _inPumps : _outPumps);
        return;
      case 135:
      case 136:
      case 132:
      case 423:
      case 424:
      case 428:
      case 440:
      case 441:
      case 442:
      case 445:
      case 468:
      case 476:
        return;
      default:
        if (_facts.Wiring.TryGetValue(tile.Type, out WiringFact? fact) && fact.IsMechanism)
        {
          Rejection = $"Wired tile {tile.Type} requires its authoritative device effect owner.";
        }
        return;
    }
  }

  private static TileCoordinate Anchor(TileCoordinate coordinate, WorldTile tile,
    int width, int height) =>
    new(coordinate.X - tile.FrameX / 18 % width, coordinate.Y - tile.FrameY / 18 % height);

  private void SwitchMb(TileCoordinate coordinate, WorldTile tile)
  {
    int row = tile.FrameY / 18;
    while (row >= 2)
    {
      row -= 2;
    }
    int column = tile.FrameX / 18;
    if (column >= 2)
    {
      column -= 2;
    }
    var anchor = new TileCoordinate(coordinate.X - column, coordinate.Y - row);
    if (!ValidateFootprint(anchor, 2, 2))
    {
      return;
    }

    for (int x = anchor.X; x < anchor.X + 2; x++)
    {
      for (int y = anchor.Y; y < anchor.Y + 2; y++)
      {
        var footprintTile = new TileCoordinate(x, y);
        _skip.Add(footprintTile);
        WorldTile current = Read(footprintTile);
        if (!current.Active || current.Type is not (35 or 139))
        {
          continue;
        }

        int frameX = current.FrameX + (current.FrameX < 36 ? 36 : -36);
        Write(footprintTile, new WorldTileDelta(FrameX: (short)frameX));
      }
    }
    CaptureTileSquare(anchor, 2, 2);
  }

  private void SwitchMonolith(TileCoordinate coordinate, WorldTile tile)
  {
    int width = tile.Type == 733 ? 3 : 2;
    int column = tile.FrameX / 18;
    while (column >= width)
    {
      column -= width;
    }
    int row = tile.FrameY / 18;
    while (row >= 3)
    {
      row -= 3;
    }
    ToggleVerticalFrames(new TileCoordinate(coordinate.X - column, coordinate.Y - row),
      width, 3, fountain: false);
  }

  private void SwitchFountain(TileCoordinate coordinate, WorldTile tile)
  {
    int column = tile.FrameX / 18;
    while (column >= 2)
    {
      column -= 2;
    }
    int row = tile.FrameY / 18;
    if (row >= 4)
    {
      row -= 4;
    }
    ToggleVerticalFrames(new TileCoordinate(coordinate.X - column, coordinate.Y - row),
      2, 4, fountain: true);
  }

  private void ToggleVerticalFrames(TileCoordinate anchor, int width, int height, bool fountain)
  {
    if (!ValidateFootprint(anchor, width, height))
    {
      return;
    }
    for (int x = anchor.X; x < anchor.X + width; x++)
    {
      for (int y = anchor.Y; y < anchor.Y + height; y++)
      {
        var coordinate = new TileCoordinate(x, y);
        _skip.Add(coordinate);
        WorldTile tile = Read(coordinate);
        if (!tile.Active)
        {
          continue;
        }
        int frameY = tile.FrameY;
        if (fountain)
        {
          if (tile.Type != 207)
          {
            continue;
          }
          frameY += frameY < 72 ? 72 : -72;
        }
        else if (tile.Type == 658)
        {
          frameY = (short)(frameY + 54);
          if (frameY >= 162)
          {
            frameY -= 162;
          }
        }
        else
        {
          int change = tile.Type switch
          {
            410 => 56,
            480 or 509 or 657 or 720 or 721 or 725 or 733 => 54,
            _ => 0
          };
          if (change == 0)
          {
            continue;
          }
          frameY += frameY < change ? change : -change;
        }
        Write(coordinate, new WorldTileDelta(FrameY: (short)frameY));
      }
    }
    CaptureTileSquare(anchor, width, height);
  }

  private bool Matches(TileCoordinate anchor, int width, int height, ushort type)
  {
    if (!ValidateFootprint(anchor, width, height))
    {
      return false;
    }
    for (int x = anchor.X; x < anchor.X + width; x++)
    {
      for (int y = anchor.Y; y < anchor.Y + height; y++)
      {
        var coordinate = new TileCoordinate(x, y);
        if (!Read(coordinate).Active || Read(coordinate).Type != type)
        {
          return false;
        }
      }
    }
    return true;
  }

  private bool TryReadFootprint(TileCoordinate anchor, int width, int height,
    out WorldTile tile)
  {
    tile = default;
    if (!ValidateFootprint(anchor, width, height))
    {
      return false;
    }
    tile = Read(anchor);
    return true;
  }

  private bool ValidateFootprint(TileCoordinate anchor, int width, int height)
  {
    if (Contains(anchor) &&
        Contains(new TileCoordinate(anchor.X + width - 1, anchor.Y + height - 1)))
    {
      return true;
    }
    Rejection = "A wired device footprint extends outside the world.";
    return false;
  }

  internal void ShiftFrames(TileCoordinate anchor, int width, int height,
    int frameX, int frameY, bool markSkipped = true, bool switchTilesOnly = false,
    short? animationType = null, ushort animationTileType = 0, int animationSendCount = 0)
  {
    if (!ValidateFootprint(anchor, width, height))
    {
      return;
    }
    for (int x = anchor.X; x < anchor.X + width; x++)
    {
      for (int y = anchor.Y; y < anchor.Y + height; y++)
      {
        var coordinate = new TileCoordinate(x, y);
        if (markSkipped)
        {
          _skip.Add(coordinate);
        }
        WorldTile tile = Read(coordinate);
        if (!switchTilesOnly || tile.Type is 132 or 411)
        {
          Write(coordinate, new WorldTileDelta(FrameX: (short)(tile.FrameX + frameX),
            FrameY: (short)(tile.FrameY + frameY)));
        }
      }
    }
    CaptureTileSquare(anchor, width, height,
      animationType, animationTileType, animationSendCount);
  }

  internal void CaptureTileSquare(TileCoordinate anchor, int width, int height,
    short? animationType = null, ushort animationTileType = 0, int animationSendCount = 0)
  {
    var tiles = new Dictionary<TileCoordinate, WorldTile>();
    for (int x = anchor.X; x < anchor.X + width; x++)
    {
      for (int y = anchor.Y; y < anchor.Y + height; y++)
      {
        var coordinate = new TileCoordinate(x, y);
        tiles.Add(coordinate, Read(coordinate));
      }
    }
    _tileSquareEffects.Add(new WiringTileSquareEffect(
      new WorldSectionBounds(anchor.X, anchor.Y, width, height),
      tiles, animationType, animationTileType, animationSendCount));
  }

  private void AddPump(TileCoordinate anchor, List<TileCoordinate> pumps)
  {
    if (!ValidateFootprint(anchor, 2, 2))
    {
      return;
    }
    // Source order: bottom left, bottom right, top left, top right; capacity is 19 tiles.
    for (int row = 1; row >= 0; row--)
    {
      for (int column = 0; column < 2; column++)
      {
        var coordinate = new TileCoordinate(anchor.X + column, anchor.Y + row);
        _skip.Add(coordinate);
        if (pumps.Count < 19)
        {
          pumps.Add(coordinate);
        }
      }
    }
  }

  private void TransferLiquid()
  {
    foreach (TileCoordinate source in _inPumps)
    {
      foreach (TileCoordinate target in _outPumps)
      {
        WorldTile input = Read(source);
        if (input.LiquidAmount == 0)
        {
          break;
        }
        WorldTile output = Read(target);
        if (output.LiquidAmount == 255 ||
            output.LiquidAmount > 0 && output.LiquidType != input.LiquidType)
        {
          continue;
        }
        int amount = Math.Min(input.LiquidAmount, 255 - output.LiquidAmount);
        Write(target, new WorldTileDelta(LiquidAmount: (byte)(output.LiquidAmount + amount),
          LiquidType: input.LiquidType));
        Write(source, new WorldTileDelta(LiquidAmount: (byte)(input.LiquidAmount - amount),
          LiquidType: input.LiquidAmount == amount ? LiquidType.Water : input.LiquidType));
      }
    }
  }
}
