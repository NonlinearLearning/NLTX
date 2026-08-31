using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidTunnelOpening
{
  public const string Source = "worldgen.Pyramid.tunnel-opening";
  public const int SourceLine = 28429;

  private const ushort PyramidTileType = 151;
  private const ushort PyramidWallType = 34;
  private const ushort SandTileType = 53;
  private const int MinimumTunnelWidth = 9;
  private const int MaximumTunnelWidthExclusive = 13;
  private const int MinimumTunnelHeight = 5;
  private const int MaximumTunnelHeightExclusive = 8;
  private const int MinimumDelay = 20;
  private const int MaximumDelayExclusive = 30;
  private const int RandomDrawCount = 3;
  private const int MaximumCommandsPerCell = 5;

  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPyramidStructureRequest request,
    int tunnelWidth,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    out LegacyPyramidTunnelOpeningResult result,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    result = default;
    failureReason = null;
    if (!TryValidateInput(
          snapshot,
          request,
          tunnelWidth,
          random,
          state,
          out long maximumCommandCount,
          out failureReason))
    {
      return false;
    }

    WorldGenerationStateComponent workingState = state;
    if (workingState.Stage < WorldGenerationStage.Structure &&
        !workingState.TryAdvance(WorldGenerationStage.Structure))
    {
      failureReason = "Pyramid tunnel opening could not enter the structure stage.";
      return false;
    }

    int direction = random.Next(2) == 0 ? -1 : 1;
    int tunnelHeight = random.Next(MinimumTunnelHeight, MaximumTunnelHeightExclusive);
    int initialDelay = random.Next(MinimumDelay, MaximumDelayExclusive);
    int startX = checked(request.OriginX - tunnelWidth * direction);
    int startY = checked(request.OriginY + tunnelWidth);
    List<TileChangeCommand> pendingCommands = new();
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    int currentX = startX;
    int currentY = startY;
    int commandsVisited = 0;
    int clearedPyramidTileCount = 0;
    int wallWriteCount = 0;
    int sandConversionCount = 0;
    bool continueOpening = true;
    long maximumVisitedColumns = snapshot.Metadata.Width + 1L;
    while (continueOpening && commandsVisited < maximumVisitedColumns)
    {
      if (!IsSafeColumn(snapshot.Metadata, currentX, currentY, tunnelHeight, direction))
      {
        break;
      }

      continueOpening = false;
      bool hasSandAbove = false;
      for (int row = currentY; row <= currentY + tunnelHeight; row++)
      {
        WorldTile above = GetProjectedTile(snapshot, projectedTiles, currentX, row - 1);
        if (above.IsActive && above.Type == SandTileType)
        {
          hasSandAbove = true;
        }

        WorldTile current = GetProjectedTile(snapshot, projectedTiles, currentX, row);
        if (current.IsActive && current.Type == PyramidTileType)
        {
          continueOpening = true;
          AppendWallCommand(
            snapshot,
            currentX,
            row + 1,
            ref workingState,
            pendingCommands,
            projectedTiles,
            ref wallWriteCount);
          AppendWallCommand(
            snapshot,
            currentX + direction,
            row,
            ref workingState,
            pendingCommands,
            projectedTiles,
            ref wallWriteCount);
          AppendTileTypeCommand(
            snapshot,
            currentX,
            row,
            PyramidTileType,
            isActive: false,
            ref workingState,
            pendingCommands,
            projectedTiles);
          clearedPyramidTileCount++;
        }

        if (hasSandAbove)
        {
          AppendTileTypeCommand(
            snapshot,
            currentX,
            row,
            SandTileType,
            isActive: true,
            ref workingState,
            pendingCommands,
            projectedTiles);
          AppendTileShapeCommand(
            snapshot,
            currentX,
            row,
            ref workingState,
            pendingCommands,
            projectedTiles);
          sandConversionCount++;
        }
      }

      commandsVisited++;
      currentX -= direction;
    }

    pendingCommands.TrimExcess();
    if (pendingCommands.Count > maximumCommandCount)
    {
      failureReason = "Pyramid tunnel-opening command capacity was exhausted.";
      return false;
    }

    commands.AddRange(pendingCommands);
    state = workingState;
    result = new LegacyPyramidTunnelOpeningResult(
      direction,
      startX,
      startY,
      tunnelHeight,
      initialDelay,
      commandsVisited,
      clearedPyramidTileCount,
      wallWriteCount,
      sandConversionCount,
      request.NoTunnel);
    return true;
  }

  private static bool TryValidateInput(
    WorldGridSnapshot snapshot,
    LegacyPyramidStructureRequest request,
    int tunnelWidth,
    LegacyPassRandomState random,
    WorldGenerationStateComponent state,
    out long maximumCommandCount,
    out string? failureReason)
  {
    maximumCommandCount = 0;
    failureReason = null;
    if (state.IsComplete || state.Stage > WorldGenerationStage.Structure)
    {
      failureReason = "Pyramid tunnel opening received a post-structure generation state.";
      return false;
    }

    if (tunnelWidth < MinimumTunnelWidth || tunnelWidth >= MaximumTunnelWidthExclusive)
    {
      failureReason = "Pyramid tunnel width was outside the source range.";
      return false;
    }

    if (request.OriginX < 0 || request.OriginY < 0 ||
        request.OriginX >= snapshot.Metadata.Width ||
        request.OriginY >= snapshot.Metadata.Height)
    {
      failureReason = "Pyramid tunnel opening origin was outside the world.";
      return false;
    }

    int horizontalPadding = MaximumTunnelWidthExclusive + 1;
    if (request.OriginX < horizontalPadding ||
        request.OriginX >= snapshot.Metadata.Width - horizontalPadding)
    {
      failureReason = "Pyramid tunnel opening crossed the horizontal world envelope.";
      return false;
    }

    long maximumStartY = (long)request.OriginY + MaximumTunnelWidthExclusive - 1;
    long maximumEndY = maximumStartY + MaximumTunnelHeightExclusive;
    if (request.OriginY >= snapshot.Metadata.Height ||
        maximumEndY >= snapshot.Metadata.Height ||
        maximumStartY - 1 < 0)
    {
      failureReason = "Pyramid tunnel opening crossed the vertical world envelope.";
      return false;
    }

    if (random.SampleCount > long.MaxValue - RandomDrawCount)
    {
      failureReason = "Pyramid tunnel-opening random sample capacity was exhausted.";
      return false;
    }

    try
    {
      maximumCommandCount = checked(
        (snapshot.Metadata.Width + 1L) *
        (MaximumTunnelHeightExclusive + 1L) *
        MaximumCommandsPerCell);
    }
    catch (OverflowException)
    {
      failureReason = "Pyramid tunnel-opening command capacity overflowed.";
      return false;
    }

    if (state.NextSequence > long.MaxValue - maximumCommandCount)
    {
      failureReason = "Pyramid tunnel-opening sequence capacity was exhausted.";
      return false;
    }

    return true;
  }

  private static bool IsSafeColumn(
    WorldMetadata metadata,
    int x,
    int y,
    int tunnelHeight,
    int direction)
  {
    return x >= 1 && x < metadata.Width - 1 &&
      x + direction >= 0 && x + direction < metadata.Width &&
      y > 0 && y + tunnelHeight < metadata.Height;
  }

  private static WorldTile GetProjectedTile(
    WorldGridSnapshot snapshot,
    IDictionary<(int X, int Y), WorldTile> projectedTiles,
    int x,
    int y)
  {
    return projectedTiles.TryGetValue((x, y), out WorldTile projected)
      ? projected
      : snapshot.GetTile(x, y);
  }

  private static void AppendWallCommand(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles,
    ref int wallWriteCount)
  {
    TileChangeCommand command = new(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetWall,
      TileType: 0,
      WallType: PyramidWallType,
      Source: Source);
    commands.Add(command);
    projectedTiles[(x, y)] = TileMutationProjection.Apply(
      GetProjectedTile(snapshot, projectedTiles, x, y),
      command);
    wallWriteCount++;
  }

  private static void AppendTileTypeCommand(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort tileType,
    bool isActive,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    TileChangeCommand command = new(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.UpdateTileType,
      tileType,
      Source: Source,
      IsActive: isActive);
    commands.Add(command);
    projectedTiles[(x, y)] = TileMutationProjection.Apply(
      GetProjectedTile(snapshot, projectedTiles, x, y),
      command);
  }

  private static void AppendTileShapeCommand(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    TileChangeCommand command = new(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.UpdateTileShape,
      TileType: 0,
      IsHalfBrick: false,
      Slope: 0,
      Source: Source);
    commands.Add(command);
    projectedTiles[(x, y)] = TileMutationProjection.Apply(
      GetProjectedTile(snapshot, projectedTiles, x, y),
      command);
  }
}
