using System;
using Terraria.WorldGeneration.Actions;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Routes one validated action to exactly one explicit owner port.
/// </summary>
public sealed class WorldGenerationActionCommitRouter : IWorldGenerationActionCommitPort
{
  public interface ITileSetPort
  {
    WorldGenerationActionCommitResult Commit(
      long generationId,
      long sequence,
      in WorldGenerationTileSetActionsCommand command);
  }

  public interface IWallMutationPort
  {
    WorldGenerationActionCommitResult Commit(
      long generationId,
      long sequence,
      in WorldGenerationWallMutationActionsCommand command);
  }

  public interface ITilePlacementAndPaintPort
  {
    WorldGenerationActionCommitResult Commit(
      long generationId,
      long sequence,
      in WorldGenerationTilePlacementAndPaintActionsCommand command);
  }

  public interface ILiquidAndNeighborPort
  {
    WorldGenerationActionCommitResult Commit(
      long generationId,
      long sequence,
      in WorldGenerationLiquidAndNeighborActionsCommand command);
  }

  public interface ITileScanAndControlPort
  {
    WorldGenerationActionCommitResult Commit(
      long generationId,
      long sequence,
      in WorldGenerationTileScanAndControlActionsCommand command);
  }

  public interface ITileFramingAndDebugPort
  {
    WorldGenerationActionCommitResult Commit(
      long generationId,
      long sequence,
      in WorldGenerationTileFramingAndDebugActionsCommand command);
  }

  public sealed class Ports
  {
    public Ports(
      ITileSetPort tileSet,
      IWallMutationPort wallMutation,
      ITilePlacementAndPaintPort tilePlacementAndPaint,
      ILiquidAndNeighborPort liquidAndNeighbor,
      ITileScanAndControlPort tileScanAndControl,
      ITileFramingAndDebugPort tileFramingAndDebug)
    {
      ArgumentNullException.ThrowIfNull(tileSet);
      ArgumentNullException.ThrowIfNull(wallMutation);
      ArgumentNullException.ThrowIfNull(tilePlacementAndPaint);
      ArgumentNullException.ThrowIfNull(liquidAndNeighbor);
      ArgumentNullException.ThrowIfNull(tileScanAndControl);
      ArgumentNullException.ThrowIfNull(tileFramingAndDebug);

      TileSet = tileSet;
      WallMutation = wallMutation;
      TilePlacementAndPaint = tilePlacementAndPaint;
      LiquidAndNeighbor = liquidAndNeighbor;
      TileScanAndControl = tileScanAndControl;
      TileFramingAndDebug = tileFramingAndDebug;
    }

    public ITileSetPort TileSet { get; }

    public IWallMutationPort WallMutation { get; }

    public ITilePlacementAndPaintPort TilePlacementAndPaint { get; }

    public ILiquidAndNeighborPort LiquidAndNeighbor { get; }

    public ITileScanAndControlPort TileScanAndControl { get; }

    public ITileFramingAndDebugPort TileFramingAndDebug { get; }
  }

  private readonly Ports _ports;

  public WorldGenerationActionCommitRouter(Ports ports)
  {
    ArgumentNullException.ThrowIfNull(ports);
    _ports = ports;
  }

  public WorldGenerationActionCommitResult Commit(in WorldGenerationAction action)
  {
    action.Validate();
    WorldGenerationActionPayload payload = action.Payload;
    switch (payload.Kind)
    {
      case WorldGenerationActionPayloadKind.TileSet:
        WorldGenerationTileSetActionsCommand tileSet = payload.TileSet!.Value;
        return _ports.TileSet.Commit(
          action.GenerationId,
          action.Sequence,
          in tileSet);
      case WorldGenerationActionPayloadKind.WallMutation:
        WorldGenerationWallMutationActionsCommand wallMutation =
          payload.WallMutation!.Value;
        return _ports.WallMutation.Commit(
          action.GenerationId,
          action.Sequence,
          in wallMutation);
      case WorldGenerationActionPayloadKind.TilePlacementAndPaint:
        WorldGenerationTilePlacementAndPaintActionsCommand placementAndPaint =
          payload.TilePlacementAndPaint!.Value;
        return _ports.TilePlacementAndPaint.Commit(
          action.GenerationId,
          action.Sequence,
          in placementAndPaint);
      case WorldGenerationActionPayloadKind.LiquidAndNeighbor:
        WorldGenerationLiquidAndNeighborActionsCommand liquidAndNeighbor =
          payload.LiquidAndNeighbor!.Value;
        return _ports.LiquidAndNeighbor.Commit(
          action.GenerationId,
          action.Sequence,
          in liquidAndNeighbor);
      case WorldGenerationActionPayloadKind.TileScanAndControl:
        WorldGenerationTileScanAndControlActionsCommand tileScanAndControl =
          payload.TileScanAndControl!.Value;
        return _ports.TileScanAndControl.Commit(
          action.GenerationId,
          action.Sequence,
          in tileScanAndControl);
      case WorldGenerationActionPayloadKind.TileFramingAndDebug:
        WorldGenerationTileFramingAndDebugActionsCommand tileFramingAndDebug =
          payload.TileFramingAndDebug!.Value;
        return _ports.TileFramingAndDebug.Commit(
          action.GenerationId,
          action.Sequence,
          in tileFramingAndDebug);
      default:
        throw new ArgumentException(
          "The action payload kind is not supported.",
          nameof(action));
    }
  }
}
