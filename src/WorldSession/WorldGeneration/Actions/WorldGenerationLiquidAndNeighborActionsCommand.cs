using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationLiquidAndNeighborActionsCommand
{
  public enum OperationKind : byte
  {
    SetLiquid,
    Smooth,
  }

  private WorldGenerationLiquidAndNeighborActionsCommand(
    OperationKind kind,
    TilePosition target,
    int liquidType,
    byte liquidLevel,
    bool applyToNeighbors)
  {
    Kind = kind;
    Target = target;
    LiquidType = liquidType;
    LiquidLevel = liquidLevel;
    ApplyToNeighbors = applyToNeighbors;
  }

  public OperationKind Kind { get; }

  public TilePosition Target { get; }

  public int LiquidType { get; }

  public byte LiquidLevel { get; }

  public bool ApplyToNeighbors { get; }

  public static WorldGenerationLiquidAndNeighborActionsCommand SetLiquid(
    TilePosition target,
    int liquidType,
    byte liquidLevel = byte.MaxValue)
  {
    return new WorldGenerationLiquidAndNeighborActionsCommand(
      OperationKind.SetLiquid,
      target,
      liquidType,
      liquidLevel,
      false);
  }

  public static WorldGenerationLiquidAndNeighborActionsCommand Smooth(
    TilePosition target,
    bool applyToNeighbors = false)
  {
    return new WorldGenerationLiquidAndNeighborActionsCommand(
      OperationKind.Smooth,
      target,
      0,
      0,
      applyToNeighbors);
  }

  public bool IsWellFormed => Kind <= OperationKind.Smooth;

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The liquid or neighbor command contains an unsupported operation.",
        nameof(Kind));
    }
  }
}
