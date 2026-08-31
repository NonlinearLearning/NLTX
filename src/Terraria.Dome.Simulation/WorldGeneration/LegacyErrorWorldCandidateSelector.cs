using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldCandidateSelector
{
  public static bool TrySelect(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    int width,
    int height,
    LegacyPassRandomState random,
    out LegacyErrorWorldCandidate candidate)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(random);
    ValidateBounds(snapshot, minimumX, maximumXExclusive, minimumY, maximumYExclusive, width, height);
    candidate = default;
    if (!HasEligibleCandidate(
          snapshot,
          definitions,
          minimumX,
          maximumXExclusive,
          minimumY,
          maximumYExclusive,
          width,
          height))
    {
      return false;
    }

    while (true)
    {
      int x = random.Next(minimumX, maximumXExclusive);
      int y = random.Next(minimumY, maximumYExclusive);
      if (IsEligibleRectangle(snapshot, definitions, x, y, width, height))
      {
        candidate = new LegacyErrorWorldCandidate(x, y);
        return true;
      }
    }
  }

  private static bool HasEligibleCandidate(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    int width,
    int height)
  {
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        if (IsEligibleRectangle(snapshot, definitions, x, y, width, height))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static bool IsEligibleRectangle(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    int x,
    int y,
    int width,
    int height)
  {
    if (x < 0 || y < 1 || x + width > snapshot.Metadata.Width ||
        y + height >= snapshot.Metadata.Height)
    {
      return false;
    }

    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      for (int offsetY = 0; offsetY < height; offsetY++)
      {
        if (!LegacyErrorWorldCandidateQuery.IsSwapEligible(
              snapshot, definitions, x + offsetX, y + offsetY))
        {
          return false;
        }
      }
    }

    return true;
  }

  private static void ValidateBounds(
    WorldGridSnapshot snapshot,
    int minimumX,
    int maximumXExclusive,
    int minimumY,
    int maximumYExclusive,
    int width,
    int height)
  {
    if (width <= 0 || height <= 0 || minimumX < 0 || minimumY < 1 ||
        maximumXExclusive > snapshot.Metadata.Width || maximumYExclusive > snapshot.Metadata.Height - 1)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }
  }
}
