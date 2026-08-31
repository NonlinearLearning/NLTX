using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidFootprintMutation
{
  public const string Source = "worldgen.Pyramid.footprint";
  public const int SourceLine = 28392;

  private const int MaximumTopOffsetExclusive = 7;
  private const int MinimumTunnelWidth = 9;
  private const int MaximumTunnelWidthExclusive = 13;
  private const int WallScanPadding = 5;
  private const int WallNeighborhoodPadding = 1;
  private const int MinimumOriginY = MaximumTopOffsetExclusive - 1;
  private const int MinimumWallScanY = 2;
  private const int RandomDrawCount = 3;

  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPyramidStructureRequest request,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    out LegacyPyramidFootprintMutationResult result,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    result = default;
    failureReason = null;
    if (!TryValidateBounds(snapshot, request, out failureReason) ||
        state.IsComplete ||
        random.SampleCount > long.MaxValue - RandomDrawCount ||
        !TryGetMaximumCommandCount(request, out long maximumCommandCount) ||
        maximumCommandCount > long.MaxValue - state.NextSequence)
    {
      failureReason ??= "Pyramid footprint input could not be validated.";
      return false;
    }

    WorldGenerationStateComponent workingState = state;
    if (workingState.Stage < WorldGenerationStage.Structure &&
        !workingState.TryAdvance(WorldGenerationStage.Structure))
    {
      failureReason = "Pyramid footprint could not enter the structure stage.";
      return false;
    }

    int topOffset = random.Next(0, MaximumTopOffsetExclusive);
    int tunnelWidth = random.Next(MinimumTunnelWidth, MaximumTunnelWidthExclusive);
    int depth = random.Next(request.PyramidMinDepth, request.PyramidMaxDepth);
    int topY = checked(request.OriginY - topOffset);
    int bottomYExclusive = checked(request.OriginY + depth);
    int rowCount = bottomYExclusive - topY;
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    List<TileChangeCommand> pendingCommands = new();
    for (int row = 0; row < rowCount; row++)
    {
      int halfWidth = row + 1;
      for (int x = request.OriginX - halfWidth;
           x < request.OriginX + halfWidth - 1;
           x++)
      {
        AppendPillarCommands(
          snapshot,
          request,
          x,
          topY + row,
          projectedTiles,
          pendingCommands,
          ref workingState);
      }
    }

    int finalHalfWidth = rowCount + 1;
    int wallTileCount = 0;
    for (int x = request.OriginX - finalHalfWidth - WallScanPadding;
         x <= request.OriginX + finalHalfWidth + WallScanPadding;
         x++)
    {
      for (int y = request.OriginY - 1; y <= bottomYExclusive + 1; y++)
      {
        if (!HasPyramidNeighborhood(snapshot, projectedTiles, x, y, request.TileType))
        {
          continue;
        }

        TileChangeCommand command = new(
          workingState.ReserveSequence(),
          x,
          y,
          TileChangeKind.SetWall,
          TileType: 0,
          WallType: request.WallType,
          Source: Source);
        pendingCommands.Add(command);
        (int X, int Y) coordinates = (x, y);
        WorldTile current = GetProjectedTile(snapshot, projectedTiles, x, y);
        projectedTiles[coordinates] = TileMutationProjection.Apply(current, command);
        wallTileCount++;
      }
    }

    commands.AddRange(pendingCommands);
    state = workingState;
    result = new LegacyPyramidFootprintMutationResult(
      topY,
      bottomYExclusive,
      tunnelWidth,
      rowCount * rowCount,
      wallTileCount);
    return true;
  }

  private static void AppendPillarCommands(
    WorldGridSnapshot snapshot,
    LegacyPyramidStructureRequest request,
    int x,
    int y,
    Dictionary<(int X, int Y), WorldTile> projectedTiles,
    List<TileChangeCommand> commands,
    ref WorldGenerationStateComponent state)
  {
    TileChangeCommand typeCommand = new(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.UpdateTileType,
      request.TileType,
      Source: Source,
      IsActive: true);
    commands.Add(typeCommand);
    WorldTile current = GetProjectedTile(snapshot, projectedTiles, x, y);
    WorldTile projected = TileMutationProjection.Apply(current, typeCommand);
    TileChangeCommand shapeCommand = new(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.UpdateTileShape,
      TileType: 0,
      IsHalfBrick: false,
      Slope: 0,
      Source: Source);
    commands.Add(shapeCommand);
    projectedTiles[(x, y)] = TileMutationProjection.Apply(projected, shapeCommand);
  }

  private static bool HasPyramidNeighborhood(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<(int X, int Y), WorldTile> projectedTiles,
    int x,
    int y,
    ushort pyramidTileType)
  {
    for (int neighborX = x - WallNeighborhoodPadding;
         neighborX <= x + WallNeighborhoodPadding;
         neighborX++)
    {
      for (int neighborY = y - WallNeighborhoodPadding;
           neighborY <= y + WallNeighborhoodPadding;
           neighborY++)
      {
        WorldTile tile = GetProjectedTile(
          snapshot,
          projectedTiles,
          neighborX,
          neighborY);
        if (!tile.IsActive || tile.Type != pyramidTileType)
        {
          return false;
        }
      }
    }

    return true;
  }

  private static WorldTile GetProjectedTile(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<(int X, int Y), WorldTile> projectedTiles,
    int x,
    int y)
  {
    return projectedTiles.TryGetValue((x, y), out WorldTile projectedTile)
      ? projectedTile
      : snapshot.GetTile(x, y);
  }

  private static bool TryValidateBounds(
    WorldGridSnapshot snapshot,
    LegacyPyramidStructureRequest request,
    out string? failureReason)
  {
    failureReason = null;
    if (request.PyramidMinDepth <= 0 ||
        request.PyramidMaxDepth < request.PyramidMinDepth)
    {
      failureReason = "Pyramid depth range was invalid.";
      return false;
    }

    if (request.OriginX < 0 || request.OriginY < 0 ||
        request.OriginX >= snapshot.Metadata.Width ||
        request.OriginY >= snapshot.Metadata.Height)
    {
      failureReason = "Pyramid origin was outside the world.";
      return false;
    }

    long maximumRowCount = checked(
      (long)request.PyramidMaxDepth + MaximumTopOffsetExclusive - 1);
    long horizontalMargin = checked(
      maximumRowCount + 1 + WallScanPadding + WallNeighborhoodPadding);
    if (request.OriginX < horizontalMargin ||
        (long)request.OriginX + horizontalMargin >= snapshot.Metadata.Width)
    {
      failureReason = "Pyramid footprint or wall envelope crossed the world edge.";
      return false;
    }

    if (request.OriginY < MinimumOriginY || request.OriginY < MinimumWallScanY ||
        (long)request.OriginY + request.PyramidMaxDepth +
          WallNeighborhoodPadding + 1 >= snapshot.Metadata.Height)
    {
      failureReason = "Pyramid vertical footprint or wall envelope crossed the world edge.";
      return false;
    }

    return true;
  }

  private static bool TryGetMaximumCommandCount(
    LegacyPyramidStructureRequest request,
    out long maximumCommandCount)
  {
    maximumCommandCount = 0;
    try
    {
      long maximumRowCount = checked(
        (long)request.PyramidMaxDepth + MaximumTopOffsetExclusive - 1);
      long maximumPillarTiles = checked(maximumRowCount * maximumRowCount);
      long maximumPillarCommands = checked(maximumPillarTiles * 2);
      long maximumWallColumns = checked(maximumRowCount * 2 + 13);
      long maximumWallRows = checked((long)request.PyramidMaxDepth + 3);
      long maximumWallCommands = checked(maximumWallColumns * maximumWallRows);
      maximumCommandCount = checked(maximumPillarCommands + maximumWallCommands);
      return true;
    }
    catch (OverflowException)
    {
      return false;
    }
  }
}
