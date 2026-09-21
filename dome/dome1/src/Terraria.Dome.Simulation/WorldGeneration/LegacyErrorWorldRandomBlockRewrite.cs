using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldRandomBlockRewrite
{
  private const string Source = "worldgen.secretseed.ErrorWorld.randomBlockRewrite";

  public static bool TryAppendCommand(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    IReadOnlyList<LegacyErrorWorldTileDefinition> replacementDefinitions,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(replacementDefinitions);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    ValidateBounds(snapshot, minimumX, maximumXExclusive, minimumY, maximumYExclusive);
    if (!HasEligibleCandidate(
          snapshot, definitions, minimumX, maximumXExclusive, minimumY, maximumYExclusive))
    {
      return false;
    }

    while (true)
    {
      int x = random.Next(minimumX, maximumXExclusive);
      int y = random.Next(minimumY, maximumYExclusive);
      WorldTile tile = snapshot.GetTile(x, y);
      if (!LegacyErrorWorldReplacementCandidateQuery.IsEligible(tile, definitions))
      {
        continue;
      }

      if (!LegacyErrorWorldRandomBlock.TrySelect(replacementDefinitions, random, out ushort tileType))
      {
        return false;
      }

      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        TileChangeKind.UpdateTileType,
        tileType,
        IsActive: isSkyblockWorld ? true : tile.IsActive,
        Source: Source));
      return true;
    }
  }

  private static bool HasEligibleCandidate(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive)
  {
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        if (LegacyErrorWorldReplacementCandidateQuery.IsEligible(snapshot.GetTile(x, y), definitions))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static void ValidateBounds(
    WorldGridSnapshot snapshot,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive)
  {
    if (minimumX < 0 || minimumY < 0 || maximumXExclusive > snapshot.Metadata.Width ||
        maximumYExclusive > snapshot.Metadata.Height || minimumX >= maximumXExclusive ||
        minimumY >= maximumYExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumX));
    }
  }
}
