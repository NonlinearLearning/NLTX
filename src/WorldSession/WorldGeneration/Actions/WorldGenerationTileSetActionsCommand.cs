using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationTileSetActionsCommand
{
  public enum OperationKind : byte
  {
    ClearTile,
    HalfBlock,
    SetTile,
    SetTileKeepWall,
    SetSlope,
    SetHalfTile,
    SwapSolidTile,
  }

  private WorldGenerationTileSetActionsCommand(
    OperationKind kind,
    TilePosition target,
    ushort tileType,
    bool value,
    int slope,
    bool frameSelf,
    bool frameNeighbors,
    bool clearBeforeSet)
  {
    Kind = kind;
    Target = target;
    TileType = tileType;
    Value = value;
    Slope = slope;
    FrameSelf = frameSelf;
    FrameNeighbors = frameNeighbors;
    ClearBeforeSet = clearBeforeSet;
  }

  public OperationKind Kind { get; }

  public TilePosition Target { get; }

  public ushort TileType { get; }

  public bool Value { get; }

  public int Slope { get; }

  public bool FrameSelf { get; }

  public bool FrameNeighbors { get; }

  public bool ClearBeforeSet { get; }

  public static WorldGenerationTileSetActionsCommand ClearTile(
    TilePosition target,
    bool frameNeighbors = false)
  {
    return Create(
      OperationKind.ClearTile,
      target,
      frameNeighbors: frameNeighbors);
  }

  public static WorldGenerationTileSetActionsCommand HalfBlock(
    TilePosition target,
    bool value = true)
  {
    return Create(OperationKind.HalfBlock, target, value: value);
  }

  public static WorldGenerationTileSetActionsCommand SetTile(
    TilePosition target,
    ushort tileType,
    bool frameSelf = false,
    bool frameNeighbors = true,
    bool clearBeforeSet = true)
  {
    return Create(
      OperationKind.SetTile,
      target,
      tileType: tileType,
      frameSelf: frameSelf,
      frameNeighbors: frameNeighbors,
      clearBeforeSet: clearBeforeSet);
  }

  public static WorldGenerationTileSetActionsCommand SetTileKeepWall(
    TilePosition target,
    ushort tileType,
    bool frameSelf = false,
    bool frameNeighbors = true)
  {
    return Create(
      OperationKind.SetTileKeepWall,
      target,
      tileType: tileType,
      frameSelf: frameSelf,
      frameNeighbors: frameNeighbors);
  }

  public static WorldGenerationTileSetActionsCommand SetSlope(
    TilePosition target,
    int slope)
  {
    return Create(OperationKind.SetSlope, target, slope: slope);
  }

  public static WorldGenerationTileSetActionsCommand SetHalfTile(
    TilePosition target,
    bool halfTile)
  {
    return Create(OperationKind.SetHalfTile, target, value: halfTile);
  }

  public static WorldGenerationTileSetActionsCommand SwapSolidTile(
    TilePosition target,
    ushort tileType)
  {
    return Create(OperationKind.SwapSolidTile, target, tileType: tileType);
  }

  public bool IsWellFormed =>
    Kind <= OperationKind.SwapSolidTile &&
    (Kind is not (OperationKind.SetSlope) || Slope >= 0);

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The tile command contains an unsupported operation or slope value.",
        nameof(Kind));
    }
  }

  private static WorldGenerationTileSetActionsCommand Create(
    OperationKind kind,
    TilePosition target,
    ushort tileType = 0,
    bool value = false,
    int slope = 0,
    bool frameSelf = false,
    bool frameNeighbors = false,
    bool clearBeforeSet = false)
  {
    return new WorldGenerationTileSetActionsCommand(
      kind,
      target,
      tileType,
      value,
      slope,
      frameSelf,
      frameNeighbors,
      clearBeforeSet);
  }
}
