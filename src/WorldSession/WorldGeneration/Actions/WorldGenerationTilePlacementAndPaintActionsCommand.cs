using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationTilePlacementAndPaintActionsCommand
{
  public enum OperationKind : byte
  {
    SetTilePaint,
    SetWallPaint,
    SetTileAndWallPaint,
    PlaceTile,
  }

  private WorldGenerationTilePlacementAndPaintActionsCommand(
    OperationKind kind,
    TilePosition target,
    byte paintId,
    ushort tileType,
    int style)
  {
    Kind = kind;
    Target = target;
    PaintId = paintId;
    TileType = tileType;
    Style = style;
  }

  public OperationKind Kind { get; }

  public TilePosition Target { get; }

  public byte PaintId { get; }

  public ushort TileType { get; }

  public int Style { get; }

  public static WorldGenerationTilePlacementAndPaintActionsCommand SetTilePaint(
    TilePosition target,
    byte paintId)
  {
    return new WorldGenerationTilePlacementAndPaintActionsCommand(
      OperationKind.SetTilePaint,
      target,
      paintId,
      0,
      0);
  }

  public static WorldGenerationTilePlacementAndPaintActionsCommand SetWallPaint(
    TilePosition target,
    byte paintId)
  {
    return new WorldGenerationTilePlacementAndPaintActionsCommand(
      OperationKind.SetWallPaint,
      target,
      paintId,
      0,
      0);
  }

  public static WorldGenerationTilePlacementAndPaintActionsCommand SetTileAndWallPaint(
    TilePosition target,
    byte paintId)
  {
    return new WorldGenerationTilePlacementAndPaintActionsCommand(
      OperationKind.SetTileAndWallPaint,
      target,
      paintId,
      0,
      0);
  }

  public static WorldGenerationTilePlacementAndPaintActionsCommand PlaceTile(
    TilePosition target,
    ushort tileType,
    int style = 0)
  {
    return new WorldGenerationTilePlacementAndPaintActionsCommand(
      OperationKind.PlaceTile,
      target,
      0,
      tileType,
      style);
  }

  public bool IsWellFormed =>
    Kind <= OperationKind.PlaceTile &&
    (Kind != OperationKind.PlaceTile || Style >= 0);

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The placement or paint command contains an unsupported operation or style.",
        nameof(Kind));
    }
  }
}
