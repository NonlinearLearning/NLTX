using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationWallMutationActionsCommand
{
  public enum OperationKind : byte
  {
    ClearWall,
    SetWall,
    PlaceWall,
  }

  private WorldGenerationWallMutationActionsCommand(
    OperationKind kind,
    TilePosition target,
    ushort wallType,
    bool frameSelf,
    bool frameNeighbors,
    bool clearTile,
    bool placeNeighbors)
  {
    Kind = kind;
    Target = target;
    WallType = wallType;
    FrameSelf = frameSelf;
    FrameNeighbors = frameNeighbors;
    ClearTile = clearTile;
    PlaceNeighbors = placeNeighbors;
  }

  public OperationKind Kind { get; }

  public TilePosition Target { get; }

  public ushort WallType { get; }

  public bool FrameSelf { get; }

  public bool FrameNeighbors { get; }

  public bool ClearTile { get; }

  public bool PlaceNeighbors { get; }

  public static WorldGenerationWallMutationActionsCommand ClearWall(
    TilePosition target,
    bool frameNeighbors = false)
  {
    return Create(
      OperationKind.ClearWall,
      target,
      frameNeighbors: frameNeighbors);
  }

  public static WorldGenerationWallMutationActionsCommand SetWall(
    TilePosition target,
    ushort wallType,
    bool frameSelf = false,
    bool frameNeighbors = true,
    bool clearTile = true)
  {
    return Create(
      OperationKind.SetWall,
      target,
      wallType,
      frameSelf,
      frameNeighbors,
      clearTile);
  }

  public static WorldGenerationWallMutationActionsCommand PlaceWall(
    TilePosition target,
    ushort wallType,
    bool placeNeighbors = true)
  {
    return Create(
      OperationKind.PlaceWall,
      target,
      wallType,
      placeNeighbors: placeNeighbors);
  }

  public bool IsWellFormed => Kind <= OperationKind.PlaceWall;

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The wall command contains an unsupported operation.",
        nameof(Kind));
    }
  }

  private static WorldGenerationWallMutationActionsCommand Create(
    OperationKind kind,
    TilePosition target,
    ushort wallType = 0,
    bool frameSelf = false,
    bool frameNeighbors = false,
    bool clearTile = false,
    bool placeNeighbors = false)
  {
    return new WorldGenerationWallMutationActionsCommand(
      kind,
      target,
      wallType,
      frameSelf,
      frameNeighbors,
      clearTile,
      placeNeighbors);
  }
}
