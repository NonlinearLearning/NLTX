using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidBuriedChestCommandProjection
{
  public const string Source = LegacyPyramidBuriedChestIntentPolicy.Source;
  public const int SourceLine = LegacyPyramidBuriedChestIntentPolicy.SourceLine;

  private const ushort DefaultChestTileType = 0;
  private const ushort ChestTileType = 21;
  private const int ChestStyle = 1;
  private const int ChestWidth = 2;
  private const int ChestHeight = 2;
  private const int TileFrameWidth = 18;
  private const int ChestStyleWidth = 36;
  private const int TileCommandCount = ChestWidth * ChestHeight;
  private const int StandardChestItemType = 848;
  private const int WaterChestItemType = 857;
  private const int GoldChestItemType = 934;

  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPyramidBuriedChestIntent intent,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    out LegacyPyramidBuriedChestCommandProjectionResult result,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    result = default;
    failureReason = null;
    if (state.IsComplete || state.Stage > WorldGenerationStage.Structure)
    {
      failureReason = "Pyramid buried-chest placement received a post-structure state.";
      return false;
    }

    if (!IsSupportedIntent(intent))
    {
      failureReason = "Pyramid buried-chest placement received unsupported source arguments.";
      return false;
    }

    if (!TryGetOrigin(snapshot.Metadata, intent, out int originX, out int originY))
    {
      failureReason = "Pyramid buried-chest placement crossed the world envelope.";
      return false;
    }

    if (!HasEmptyFootprint(snapshot, originX, originY))
    {
      failureReason = "Pyramid buried-chest placement overlapped an active tile.";
      return false;
    }

    if (state.NextSequence > long.MaxValue - 1 - TileCommandCount)
    {
      failureReason = "Pyramid buried-chest command sequence capacity was exhausted.";
      return false;
    }

    WorldGenerationStateComponent workingState = state;
    if (workingState.Stage < WorldGenerationStage.Structure &&
        !workingState.TryAdvance(WorldGenerationStage.Structure))
    {
      failureReason = "Pyramid buried-chest placement could not enter the structure stage.";
      return false;
    }

    List<TileChangeCommand> pendingCommands = new(TileCommandCount);
    for (int localX = 0; localX < ChestWidth; localX++)
    {
      for (int localY = 0; localY < ChestHeight; localY++)
      {
        int tileX = originX + localX;
        int tileY = originY + localY;
        WorldSectionCoordinates section = new(
          tileX / WorldGrid.SectionWidth,
          tileY / WorldGrid.SectionHeight);
        pendingCommands.Add(new TileChangeCommand(
          workingState.ReserveSequence(),
          tileX,
          tileY,
          TileChangeKind.Place,
          ChestTileType,
          FrameX: checked((short)(ChestStyle * ChestStyleWidth + localX * TileFrameWidth)),
          FrameY: checked((short)(localY * TileFrameWidth)),
          IsHalfBrick: false,
          Slope: 0,
          Source: Source,
          SourceLine: SourceLine,
          ExpectedSectionVersion: snapshot.GetSectionVersion(section),
          IsActive: true));
      }
    }

    commands.AddRange(pendingCommands);
    state = workingState;
    result = new LegacyPyramidBuriedChestCommandProjectionResult(
      originX,
      originY,
      ChestTileType,
      ChestStyle,
      intent.MainItemInChest,
      pendingCommands.Count);
    return true;
  }

  private static bool IsSupportedIntent(LegacyPyramidBuriedChestIntent intent)
  {
    return intent.Sequence >= 0 &&
      StringComparer.Ordinal.Equals(intent.Source, Source) &&
      intent.SourceLine == SourceLine &&
      !intent.NotNearOtherChests &&
      intent.ChestStyle == ChestStyle &&
      !intent.TrySlope &&
      intent.ChestTileType == DefaultChestTileType &&
      intent.MainItemInChest is StandardChestItemType or WaterChestItemType or GoldChestItemType;
  }

  private static bool TryGetOrigin(
    WorldMetadata metadata,
    LegacyPyramidBuriedChestIntent intent,
    out int originX,
    out int originY)
  {
    originX = 0;
    originY = 0;
    try
    {
      originX = checked(intent.TileX - 1);
      originY = checked(intent.TileY - 1);
    }
    catch (OverflowException)
    {
      return false;
    }

    return originX >= 0 && originY >= 0 &&
      originX < metadata.Width - ChestWidth + 1 &&
      originY < metadata.Height - ChestHeight + 1;
  }

  private static bool HasEmptyFootprint(
    WorldGridSnapshot snapshot,
    int originX,
    int originY)
  {
    for (int localX = 0; localX < ChestWidth; localX++)
    {
      for (int localY = 0; localY < ChestHeight; localY++)
      {
        if (snapshot.GetTile(originX + localX, originY + localY).IsActive)
        {
          return false;
        }
      }
    }

    return true;
  }
}
