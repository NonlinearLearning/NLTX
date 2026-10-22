namespace MinimalImplementation.Server.World;

/// <summary>Packet 17 tile operation codes owned by the world tile store.</summary>
public enum WorldTileOperation : byte
{
  KillTile = 0,
  PlaceTile = 1,
  KillWall = 2,
  PlaceWall = 3,
  KillTileWithoutItem = 4,
  PlaceWireRed = 5,
  KillWireRed = 6,
  PoundTile = 7,
  PlaceActuator = 8,
  KillActuator = 9,
  PlaceWireBlue = 10,
  KillWireBlue = 11,
  PlaceWireGreen = 12,
  KillWireGreen = 13,
  SetSlope = 14,
  FrameTrack = 15,
  PlaceWireYellow = 16,
  KillWireYellow = 17,
  PokeLogicGate = 18,
  Actuate = 19,
  KillTileAndReport = 20,
  ReplaceTile = 21,
  ReplaceWall = 22,
  PoundTileIfPossible = 23
}

/// <summary>Additional effects that must be executed by the owning domain after tile commit.</summary>
public readonly record struct WorldTileOperationEffects(
    bool RequiresItemDrop,
    bool RequiresMechanismDispatch,
    bool RequiresTileCorrection,
    bool RequiresSectionResync);

/// <summary>
/// Result of a world operation. Item, mechanism and correction effects are returned as facts;
/// this type deliberately does not call P04/P06 or publish network bytes.
/// </summary>
public sealed class WorldTileOperationResult
{
  internal WorldTileOperationResult(
      bool accepted,
      WorldMutationResult mutation,
      WorldTileOperation operation,
      TileCoordinate coordinate,
      WorldTileOperationEffects effects,
      string? rejection)
  {
    Accepted = accepted;
    Mutation = mutation;
    Operation = operation;
    Coordinate = coordinate;
    Effects = effects;
    Rejection = rejection;
  }

  public bool Accepted { get; }

  public WorldMutationResult Mutation { get; }

  public WorldTileOperation Operation { get; }

  public TileCoordinate Coordinate { get; }

  public WorldTileOperationEffects Effects { get; }

  public string? Rejection { get; }
}

public sealed partial class WorldOwner
{
  /// <summary>Byte-oriented adapter for the Packet 17 dispatcher.</summary>
  public WorldTileOperationResult ApplyTileManipulation(
      ulong worldEpoch,
      byte action,
      TileCoordinate coordinate,
      short parameter,
      byte style,
      bool forced = false,
      bool producesImmediateBroadcast = true)
  {
    if (!Enum.IsDefined(typeof(WorldTileOperation), action))
    {
      return RejectedOperation(
          (WorldTileOperation)action, coordinate, "The tile operation is outside the Version4 range.");
    }

    return ApplyTileOperation(
        worldEpoch,
        (WorldTileOperation)action,
        coordinate,
        unchecked((ushort)parameter),
        style,
        forced,
        producesImmediateBroadcast);
  }

  /// <summary>
  /// Applies one Packet 17 operation through the authoritative tile owner. Cross-owner effects
  /// are represented by <see cref="WorldTileOperationEffects"/> and are not executed here.
  /// </summary>
  public WorldTileOperationResult ApplyTileOperation(
      ulong worldEpoch,
      WorldTileOperation operation,
      TileCoordinate coordinate,
      ushort parameter = 0,
      byte style = 0,
      bool forced = false,
      bool producesImmediateBroadcast = true)
  {
    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return RejectedOperation(operation, coordinate, "The world epoch is stale or unavailable.");
      }

      if (!_header!.Geometry.Contains(coordinate))
      {
        return RejectedOperation(operation, coordinate, "The tile coordinate is outside the world.");
      }

      const int inWorldMargin = 3;
      if (coordinate.X < inWorldMargin || coordinate.Y < inWorldMargin ||
          coordinate.X >= _header.Geometry.Width - inWorldMargin ||
          coordinate.Y >= _header.Geometry.Height - inWorldMargin)
      {
        return RejectedOperation(
            operation,
            coordinate,
            $"The tile coordinate does not satisfy the Version4 in-world margin of {inWorldMargin}.");
      }

      WorldTile current = _tiles![GetTileIndex(coordinate)];
      if (parameter == 1 && operation is WorldTileOperation.KillTile or
          WorldTileOperation.KillWall or WorldTileOperation.KillTileWithoutItem) {
        return NoOpOperation(operation, coordinate, producesImmediateBroadcast);
      }
      WorldTile? next = null;
      WorldTileOperationEffects effects = default;
      switch (operation)
      {
        case WorldTileOperation.KillTile:
          if (!current.Active)
          {
            return NoOpOperation(operation, coordinate, producesImmediateBroadcast);
          }
          next = ClearTile(current);
          effects = new(true, false, false, false);
          break;
        case WorldTileOperation.KillTileWithoutItem:
          if (!current.Active)
          {
            return NoOpOperation(operation, coordinate, producesImmediateBroadcast);
          }
          next = ClearTile(current);
          break;
        case WorldTileOperation.KillTileAndReport:
          if (!current.Active)
          {
            return AcceptedOperation(
                operation,
                coordinate,
                WorldMutationResult.Success(
                    CurrentVersion,
                    [],
                    [],
                    producesImmediateBroadcast),
                new WorldTileOperationEffects(false, false, true, false));
          }
          next = ClearTile(current);
          effects = new(false, false, true, false);
          break;
        case WorldTileOperation.PlaceTile:
        case WorldTileOperation.ReplaceTile:
          if (!_facts.IsValidTileType(parameter))
          {
            return RejectedOperation(operation, coordinate, "The tile type is outside the Version4 facts table.");
          }
          if (operation == WorldTileOperation.PlaceTile && !forced && current.Active)
          {
            return RejectedOperation(
                operation,
                coordinate,
                "A non-forced placement cannot replace an occupied tile.");
          }
          next = PlaceTile(current, parameter, style);
          effects = new(false, false, operation == WorldTileOperation.ReplaceTile, false);
          break;
        case WorldTileOperation.KillWall:
          if (current.Wall == 0)
          {
            return NoOpOperation(operation, coordinate, producesImmediateBroadcast);
          }
          next = current with { Wall = 0, WallColor = 0, WallHeader = 0 };
          break;
        case WorldTileOperation.PlaceWall:
        case WorldTileOperation.ReplaceWall:
          if (parameter == 0)
          {
            return RejectedOperation(operation, coordinate, "A wall type of zero is not placeable.");
          }
          next = current with { Wall = parameter };
          break;
        case WorldTileOperation.PlaceWireRed:
        case WorldTileOperation.PlaceWireBlue:
        case WorldTileOperation.PlaceWireGreen:
        case WorldTileOperation.PlaceWireYellow:
        case WorldTileOperation.KillWireRed:
        case WorldTileOperation.KillWireBlue:
        case WorldTileOperation.KillWireGreen:
        case WorldTileOperation.KillWireYellow:
          next = current with { Wires = UpdateWire(current.Wires, operation) };
          effects = new(false, true, false, false);
          break;
        case WorldTileOperation.PlaceActuator:
          next = current with { Actuator = true };
          effects = new(false, true, false, false);
          break;
        case WorldTileOperation.KillActuator:
          next = current with { Actuator = false };
          effects = new(false, true, false, false);
          break;
        case WorldTileOperation.PoundTile:
        case WorldTileOperation.PoundTileIfPossible:
        case WorldTileOperation.SetSlope:
          if (!current.Active || !_facts.Solid[current.Type])
          {
            return RejectedOperation(operation, coordinate, "The tile cannot be sloped.");
          }
          byte slope = operation switch
          {
            WorldTileOperation.SetSlope => (byte)parameter,
            WorldTileOperation.PoundTileIfPossible => (byte)parameter,
            _ => (byte)((current.Slope + 1) % 5)
          };
          if (slope > 4)
          {
            return RejectedOperation(operation, coordinate, "The slope is outside the Version4 range.");
          }
          next = current with { Slope = slope, HalfBlock = slope == 0 && current.HalfBlock };
          break;
        case WorldTileOperation.FrameTrack:
          effects = new(false, true, false, true);
          return AcceptedOperation(operation, coordinate,
              WorldMutationResult.Success(CurrentVersion, [], [], producesImmediateBroadcast: false),
              effects);
        case WorldTileOperation.Actuate:
          effects = new(false, true, false, false);
          return AcceptedOperation(operation, coordinate,
              WorldMutationResult.Success(CurrentVersion, [], [], producesImmediateBroadcast),
              effects);
        case WorldTileOperation.PokeLogicGate:
          effects = new(false, true, false, false);
          return AcceptedOperation(operation, coordinate,
              WorldMutationResult.Success(CurrentVersion, [], [], producesImmediateBroadcast: false),
              effects);
        default:
          return RejectedOperation(operation, coordinate, "The tile operation is unsupported.");
      }

      WorldMutationResult mutation = ApplyTileDeltas(
          worldEpoch,
          [new KeyValuePair<TileCoordinate, WorldTileDelta>(
              coordinate, ToDelta(next!.Value, current))],
          producesImmediateBroadcast);
      if (!mutation.Applied)
      {
        return RejectedOperation(operation, coordinate, mutation.Rejection ?? "The tile mutation was rejected.");
      }

      return AcceptedOperation(operation, coordinate, mutation, effects);
    }
  }

  /// <summary>Applies the full Packet 20 rectangle while preserving omitted field presence.</summary>
  public WorldMutationResult ApplyAreaTileChanges(
      ulong worldEpoch,
      WorldSectionBounds rectangle,
      IReadOnlyList<WorldTilePatch> patches,
      byte changeType,
      bool producesImmediateBroadcast = true)
  {
    ArgumentNullException.ThrowIfNull(patches);
    _ = changeType;
    if (rectangle.Width <= 0 || rectangle.Height <= 0 ||
        rectangle.Width > byte.MaxValue || rectangle.Height > byte.MaxValue)
    {
      return WorldMutationResult.Rejected("The area tile rectangle is outside the wire bounds.");
    }

    if (patches.Count != checked(rectangle.Width * rectangle.Height))
    {
      return WorldMutationResult.Rejected("The area tile rectangle does not contain every wire tile.");
    }

    if (rectangle.OriginX < 0 || rectangle.OriginY < 0)
    {
      return WorldMutationResult.Rejected("The area tile rectangle starts outside the world.");
    }

    var expected = new HashSet<TileCoordinate>();
    foreach (WorldTilePatch patch in patches)
    {
      if (!rectangle.Contains(patch.Coordinate) || !expected.Add(patch.Coordinate))
      {
        return WorldMutationResult.Rejected("The area tile rectangle contains a duplicate or out-of-range tile.");
      }
    }

    return ApplyTilePatches(worldEpoch, patches, producesImmediateBroadcast);
  }

  /// <summary>Named adapter used by the AreaTileChange packet handler.</summary>
  public WorldMutationResult ApplyAreaTileChange(
      ulong worldEpoch,
      int originX,
      int originY,
      byte width,
      byte height,
      IReadOnlyList<WorldTilePatch> patches,
      byte changeType,
      bool producesImmediateBroadcast = true)
  {
    return ApplyAreaTileChanges(
        worldEpoch,
        new WorldSectionBounds(originX, originY, width, height),
        patches,
        changeType,
        producesImmediateBroadcast);
  }

  /// <summary>Packet 48 legacy liquid path: zero amount is an immediate producer event.</summary>
  public WorldMutationResult ApplyLegacyLiquidUpdate(
      ulong worldEpoch, TileCoordinate coordinate, byte amount, LiquidType type)
  {
    if (!Enum.IsDefined(type))
    {
      return WorldMutationResult.Rejected("The liquid type is outside the Version4 range.");
    }

    return ApplyTileDeltas(
        worldEpoch,
        [new KeyValuePair<TileCoordinate, WorldTileDelta>(
            coordinate, new WorldTileDelta(LiquidAmount: amount, LiquidType: type))],
        producesImmediateBroadcast: amount == 0);
  }

  /// <summary>Compatibility name for the packet 48 world mutation boundary.</summary>
  public WorldMutationResult ApplyLiquidUpdate(
      ulong worldEpoch, TileCoordinate coordinate, byte amount, LiquidType type)
  {
    return ApplyLegacyLiquidUpdate(worldEpoch, coordinate, amount, type);
  }

  private static WorldTile ClearTile(WorldTile current)
  {
    return current with {
      Active = false,
      Type = 0,
      FrameX = 0,
      FrameY = 0,
      HalfBlock = false,
      Slope = 0,
      Actuator = false,
      Inactive = false
    };
  }

  private WorldTile PlaceTile(WorldTile current, ushort type, byte style)
  {
    bool frameImportant = _facts.FrameImportant[type];
    return current with {
      Active = true,
      Type = type,
      FrameX = frameImportant ? (short)(style * 18) : (short)-1,
      FrameY = frameImportant ? (short)0 : (short)-1,
      HalfBlock = false,
      Slope = 0,
      Inactive = false
    };
  }

  private static TileWireMask UpdateWire(TileWireMask current, WorldTileOperation operation)
  {
    TileWireMask mask = operation switch
    {
      WorldTileOperation.PlaceWireRed or WorldTileOperation.KillWireRed => TileWireMask.Red,
      WorldTileOperation.PlaceWireBlue or WorldTileOperation.KillWireBlue => TileWireMask.Blue,
      WorldTileOperation.PlaceWireGreen or WorldTileOperation.KillWireGreen => TileWireMask.Green,
      _ => TileWireMask.Yellow
    };
    bool place = operation is WorldTileOperation.PlaceWireRed or WorldTileOperation.PlaceWireBlue or
        WorldTileOperation.PlaceWireGreen or WorldTileOperation.PlaceWireYellow;
    return place ? current | mask : current & ~mask;
  }

  private static WorldTileDelta ToDelta(WorldTile next, WorldTile current)
  {
    return new WorldTileDelta(
        Active: next.Active != current.Active ? next.Active : null,
        Type: next.Type != current.Type ? next.Type : null,
        Wall: next.Wall != current.Wall ? next.Wall : null,
        FrameX: next.FrameX != current.FrameX ? next.FrameX : null,
        FrameY: next.FrameY != current.FrameY ? next.FrameY : null,
        LiquidAmount: next.LiquidAmount != current.LiquidAmount ? next.LiquidAmount : null,
        LiquidType: next.LiquidType != current.LiquidType ? next.LiquidType : null,
        TileColor: next.TileColor != current.TileColor ? next.TileColor : null,
        WallColor: next.WallColor != current.WallColor ? next.WallColor : null,
        Wires: next.Wires != current.Wires ? next.Wires : null,
        Actuator: next.Actuator != current.Actuator ? next.Actuator : null,
        Inactive: next.Inactive != current.Inactive ? next.Inactive : null,
        HalfBlock: next.HalfBlock != current.HalfBlock ? next.HalfBlock : null,
        Slope: next.Slope != current.Slope ? next.Slope : null,
        TileHeader3: next.TileHeader3 != current.TileHeader3 ? next.TileHeader3 : null,
        WallHeader: next.WallHeader != current.WallHeader ? next.WallHeader : null);
  }

  private WorldTileOperationResult AcceptedOperation(
      WorldTileOperation operation,
      TileCoordinate coordinate,
      WorldMutationResult mutation,
      WorldTileOperationEffects effects)
  {
    return new WorldTileOperationResult(true, mutation, operation, coordinate, effects, null);
  }

  private WorldTileOperationResult NoOpOperation(
      WorldTileOperation operation, TileCoordinate coordinate, bool producesImmediateBroadcast)
  {
    return AcceptedOperation(operation, coordinate,
        WorldMutationResult.Success(CurrentVersion, [], [], producesImmediateBroadcast),
        default);
  }

  private static WorldTileOperationResult RejectedOperation(
      WorldTileOperation operation, TileCoordinate coordinate, string rejection)
  {
    return new WorldTileOperationResult(
        false, WorldMutationResult.Rejected(rejection), operation, coordinate, default, rejection);
  }
}
