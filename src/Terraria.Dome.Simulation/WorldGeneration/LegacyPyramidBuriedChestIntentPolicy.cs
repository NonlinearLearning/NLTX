using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidBuriedChestIntentPolicy
{
  public const string Source = "worldgen.Pyramid.buried-chest";
  public const int SourceLine = 28555;

  private const int ChestStyle = 1;
  private const int MaximumRandomSamples = 2;
  private const int SelectionCount = 3;
  private const int StandardChestItemType = 848;
  private const int WaterChestItemType = 857;
  private const int GoldChestItemType = 934;
  private const bool NotNearOtherChests = false;
  private const bool TrySlope = false;
  private const ushort DefaultChestTileType = 0;

  public static bool TryCreateIntent(
    WorldGridSnapshot snapshot,
    int tunnelStartX,
    int tunnelEndX,
    int openingY,
    bool tenthAnniversaryWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    out LegacyPyramidBuriedChestIntent intent,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    intent = default;
    failureReason = null;
    if (state.IsComplete || state.Stage > WorldGenerationStage.Structure)
    {
      failureReason = "Pyramid buried-chest intent received a post-structure generation state.";
      return false;
    }

    if (!TryGetChestX(snapshot, tunnelStartX, tunnelEndX, openingY, out int chestX))
    {
      failureReason = "Pyramid buried-chest intent crossed the world envelope.";
      return false;
    }

    if (random.SampleCount > long.MaxValue - MaximumRandomSamples)
    {
      failureReason = "Pyramid buried-chest random sample capacity was exhausted.";
      return false;
    }

    if (state.NextSequence == long.MaxValue)
    {
      failureReason = "Pyramid buried-chest intent sequence capacity was exhausted.";
      return false;
    }

    WorldGenerationStateComponent workingState = state;
    if (workingState.Stage < WorldGenerationStage.Structure &&
        !workingState.TryAdvance(WorldGenerationStage.Structure))
    {
      failureReason = "Pyramid buried-chest intent could not enter the structure stage.";
      return false;
    }

    int selection = random.Next(SelectionCount);
    if (selection == 0)
    {
      selection = random.Next(SelectionCount);
    }

    if (tenthAnniversaryWorld && selection == 0)
    {
      selection = 1;
    }

    int mainItemInChest = selection switch
    {
      0 => StandardChestItemType,
      1 => WaterChestItemType,
      2 => GoldChestItemType,
      _ => throw new InvalidOperationException(
        "Pyramid buried-chest item selection was outside the source range.")
    };
    intent = new LegacyPyramidBuriedChestIntent(
      workingState.ReserveSequence(),
      chestX,
      openingY,
      mainItemInChest,
      NotNearOtherChests,
      ChestStyle,
      TrySlope,
      DefaultChestTileType,
      Source,
      SourceLine);
    state = workingState;
    return true;
  }

  private static bool TryGetChestX(
    WorldGridSnapshot snapshot,
    int tunnelStartX,
    int tunnelEndX,
    int openingY,
    out int chestX)
  {
    chestX = 0;
    if (openingY <= 0 ||
        !snapshot.Metadata.IsInside(tunnelStartX, openingY) ||
        !snapshot.Metadata.IsInside(tunnelEndX, openingY))
    {
      return false;
    }

    int minimumX = Math.Min(tunnelStartX, tunnelEndX);
    int maximumX = Math.Max(tunnelStartX, tunnelEndX);
    try
    {
      chestX = checked((int)(((long)minimumX + maximumX) / 2));
    }
    catch (OverflowException)
    {
      return false;
    }

    return snapshot.Metadata.IsInside(chestX - 1, openingY - 1) &&
      snapshot.Metadata.IsInside(chestX, openingY - 1);
  }
}
