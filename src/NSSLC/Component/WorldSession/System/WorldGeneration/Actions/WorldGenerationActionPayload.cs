using System;

namespace Terraria.WorldGeneration.Actions;

public enum WorldGenerationActionPayloadKind : byte
{
  None,
  TileSet,
  WallMutation,
  TilePlacementAndPaint,
  LiquidAndNeighbor,
  TileScanAndControl,
  TileFramingAndDebug,
}

public readonly record struct WorldGenerationActionPayload
{
  private WorldGenerationActionPayload(
    WorldGenerationActionPayloadKind kind,
    WorldGenerationTileSetActionsCommand? tileSet,
    WorldGenerationWallMutationActionsCommand? wallMutation,
    WorldGenerationTilePlacementAndPaintActionsCommand? tilePlacementAndPaint,
    WorldGenerationLiquidAndNeighborActionsCommand? liquidAndNeighbor,
    WorldGenerationTileScanAndControlActionsCommand? tileScanAndControl,
    WorldGenerationTileFramingAndDebugActionsCommand? tileFramingAndDebug)
  {
    Kind = kind;
    TileSet = tileSet;
    WallMutation = wallMutation;
    TilePlacementAndPaint = tilePlacementAndPaint;
    LiquidAndNeighbor = liquidAndNeighbor;
    TileScanAndControl = tileScanAndControl;
    TileFramingAndDebug = tileFramingAndDebug;
  }

  public WorldGenerationActionPayloadKind Kind { get; }

  public WorldGenerationTileSetActionsCommand? TileSet { get; }

  public WorldGenerationWallMutationActionsCommand? WallMutation { get; }

  public WorldGenerationTilePlacementAndPaintActionsCommand?
    TilePlacementAndPaint { get; }

  public WorldGenerationLiquidAndNeighborActionsCommand? LiquidAndNeighbor { get; }

  public WorldGenerationTileScanAndControlActionsCommand? TileScanAndControl { get; }

  public WorldGenerationTileFramingAndDebugActionsCommand? TileFramingAndDebug { get; }

  public static WorldGenerationActionPayload FromTileSet(
    WorldGenerationTileSetActionsCommand command)
  {
    return new WorldGenerationActionPayload(
      WorldGenerationActionPayloadKind.TileSet,
      command,
      null,
      null,
      null,
      null,
      null);
  }

  public static WorldGenerationActionPayload FromWallMutation(
    WorldGenerationWallMutationActionsCommand command)
  {
    return new WorldGenerationActionPayload(
      WorldGenerationActionPayloadKind.WallMutation,
      null,
      command,
      null,
      null,
      null,
      null);
  }

  public static WorldGenerationActionPayload FromTilePlacementAndPaint(
    WorldGenerationTilePlacementAndPaintActionsCommand command)
  {
    return new WorldGenerationActionPayload(
      WorldGenerationActionPayloadKind.TilePlacementAndPaint,
      null,
      null,
      command,
      null,
      null,
      null);
  }

  public static WorldGenerationActionPayload FromLiquidAndNeighbor(
    WorldGenerationLiquidAndNeighborActionsCommand command)
  {
    return new WorldGenerationActionPayload(
      WorldGenerationActionPayloadKind.LiquidAndNeighbor,
      null,
      null,
      null,
      command,
      null,
      null);
  }

  public static WorldGenerationActionPayload FromTileScanAndControl(
    WorldGenerationTileScanAndControlActionsCommand command)
  {
    return new WorldGenerationActionPayload(
      WorldGenerationActionPayloadKind.TileScanAndControl,
      null,
      null,
      null,
      null,
      command,
      null);
  }

  public static WorldGenerationActionPayload FromTileFramingAndDebug(
    WorldGenerationTileFramingAndDebugActionsCommand command)
  {
    return new WorldGenerationActionPayload(
      WorldGenerationActionPayloadKind.TileFramingAndDebug,
      null,
      null,
      null,
      null,
      null,
      command);
  }

  public bool IsWellFormed =>
    Kind switch
    {
      WorldGenerationActionPayloadKind.TileSet =>
        TileSet.HasValue &&
        !WallMutation.HasValue &&
        !TilePlacementAndPaint.HasValue &&
        !LiquidAndNeighbor.HasValue &&
        !TileScanAndControl.HasValue &&
        !TileFramingAndDebug.HasValue,
      WorldGenerationActionPayloadKind.WallMutation =>
        !TileSet.HasValue &&
        WallMutation.HasValue &&
        !TilePlacementAndPaint.HasValue &&
        !LiquidAndNeighbor.HasValue &&
        !TileScanAndControl.HasValue &&
        !TileFramingAndDebug.HasValue,
      WorldGenerationActionPayloadKind.TilePlacementAndPaint =>
        !TileSet.HasValue &&
        !WallMutation.HasValue &&
        TilePlacementAndPaint.HasValue &&
        !LiquidAndNeighbor.HasValue &&
        !TileScanAndControl.HasValue &&
        !TileFramingAndDebug.HasValue,
      WorldGenerationActionPayloadKind.LiquidAndNeighbor =>
        !TileSet.HasValue &&
        !WallMutation.HasValue &&
        !TilePlacementAndPaint.HasValue &&
        LiquidAndNeighbor.HasValue &&
        !TileScanAndControl.HasValue &&
        !TileFramingAndDebug.HasValue,
      WorldGenerationActionPayloadKind.TileScanAndControl =>
        !TileSet.HasValue &&
        !WallMutation.HasValue &&
        !TilePlacementAndPaint.HasValue &&
        !LiquidAndNeighbor.HasValue &&
        TileScanAndControl.HasValue &&
        !TileFramingAndDebug.HasValue,
      WorldGenerationActionPayloadKind.TileFramingAndDebug =>
        !TileSet.HasValue &&
        !WallMutation.HasValue &&
        !TilePlacementAndPaint.HasValue &&
        !LiquidAndNeighbor.HasValue &&
        !TileScanAndControl.HasValue &&
        TileFramingAndDebug.HasValue,
      _ => false,
    };

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "A world-generation action must contain exactly one valid payload.",
        nameof(Kind));
    }

    switch (Kind)
    {
      case WorldGenerationActionPayloadKind.TileSet:
        TileSet!.Value.Validate();
        break;
      case WorldGenerationActionPayloadKind.WallMutation:
        WallMutation!.Value.Validate();
        break;
      case WorldGenerationActionPayloadKind.TilePlacementAndPaint:
        TilePlacementAndPaint!.Value.Validate();
        break;
      case WorldGenerationActionPayloadKind.LiquidAndNeighbor:
        LiquidAndNeighbor!.Value.Validate();
        break;
      case WorldGenerationActionPayloadKind.TileScanAndControl:
        TileScanAndControl!.Value.Validate();
        break;
      case WorldGenerationActionPayloadKind.TileFramingAndDebug:
        TileFramingAndDebug!.Value.Validate();
        break;
      default:
        throw new ArgumentException(
          "The action payload kind is not supported.",
          nameof(Kind));
    }
  }
}
