using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMountainCavesPass
{
  public const int SourceLine = 10839;
  public const int SourceAnchorLine = SourceLine;

  private const int CandidateMaximumPercent = 75;
  private const int CandidateMinimumPercent = 25;
  private const int CenterExclusionRadius = 90;
  private const int CaveHistorySpacing = 100;
  private const int SandScanHorizontalRadius = 50;
  private const int SandScanMaximumYOffset = 25;
  private const int SandScanMinimumYOffset = -25;
  private const int ReferenceInvocationCount = 1;
  private const int ReferenceWorldWidth = 1000;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    ICollection<LegacyCaveCoordinate>? caveHistory = null,
    bool isRemixWorld = false,
    bool isSkyblockWorld = false,
    bool isNoSurfaceWorld = false,
    bool isSurfaceDesertWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld || isNoSurfaceWorld || isSurfaceDesertWorld)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    int candidateMinimumX = snapshot.Metadata.Width * CandidateMinimumPercent / 100;
    int candidateMaximumXExclusive = snapshot.Metadata.Width * CandidateMaximumPercent / 100;
    if (candidateMaximumXExclusive <= candidateMinimumX)
    {
      return;
    }

    int surfaceMaximumYExclusive = Math.Clamp(
      (int)profile.WorldSurface,
      0,
      snapshot.Metadata.Height);
    List<int> caveXs = new();
    int count = CalculateInvocationCount(snapshot.Metadata.Width, isRemixWorld);
    for (int index = 0; index < count; index++)
    {
      int candidateX = random.Next(candidateMinimumX, candidateMaximumXExclusive);
      if (!TrySelectCandidateX(
            snapshot.Metadata.Width,
            candidateMinimumX,
            candidateMaximumXExclusive,
            random,
            caveXs,
            ref candidateX,
            isRemixWorld))
      {
        continue;
      }

      int surfaceY = FindSurfaceY(snapshot, candidateX, surfaceMaximumYExclusive);
      if (surfaceY < 0 || HasSandVeto(snapshot, candidateX, surfaceY))
      {
        continue;
      }

      LegacyMountinater.AppendCommands(
        snapshot,
        candidateX,
        surfaceY,
        random,
        ref state,
        commands);
      caveXs.Add(candidateX);
      caveHistory?.Add(new LegacyCaveCoordinate(candidateX, surfaceY));
    }
  }

  public static int CalculateInvocationCount(int width, bool isRemixWorld = false)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int count = (int)(width * (double)ReferenceInvocationCount / ReferenceWorldWidth);
    return isRemixWorld ? (int)(count * 1.5) : count;
  }

  public static bool IsCandidateXAccepted(
    int candidateX,
    int width,
    IReadOnlyList<int> caveXs,
    bool isRemixWorld = false)
  {
    ArgumentNullException.ThrowIfNull(caveXs);
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (!isRemixWorld &&
        candidateX > width / 2 - CenterExclusionRadius &&
        candidateX < width / 2 + CenterExclusionRadius)
    {
      return false;
    }

    for (int index = 0; index < caveXs.Count; index++)
    {
      if (Math.Abs(candidateX - caveXs[index]) < CaveHistorySpacing)
      {
        return false;
      }
    }

    return true;
  }

  private static int FindSurfaceY(WorldGridSnapshot snapshot, int x, int maximumYExclusive)
  {
    for (int y = 0; y < maximumYExclusive; y++)
    {
      if (snapshot.GetTile(x, y).IsActive)
      {
        return y;
      }
    }

    return -1;
  }

  private static bool HasSandVeto(WorldGridSnapshot snapshot, int centerX, int centerY)
  {
    for (int x = centerX - SandScanHorizontalRadius;
         x < centerX + SandScanHorizontalRadius;
         x++)
    {
      for (int y = centerY + SandScanMinimumYOffset;
           y < centerY + SandScanMaximumYOffset;
           y++)
      {
        if (!snapshot.Metadata.IsInside(x, y))
        {
          continue;
        }

        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.IsActive && (tile.Type == 53 || tile.Type == 151 || tile.Type == 274))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static bool TrySelectCandidateX(
    int width,
    int minimumX,
    int maximumXExclusive,
    LegacyPassRandomState random,
    IReadOnlyList<int> caveXs,
    ref int candidateX,
    bool isRemixWorld)
  {
    int historyRetryCount = 0;
    while (true)
    {
      if (IsCandidateXAccepted(candidateX, width, caveXs, isRemixWorld))
      {
        return true;
      }

      if (!isRemixWorld && !IsOutsideCenterExclusion(candidateX, width))
      {
        candidateX = random.Next(minimumX, maximumXExclusive);
        continue;
      }

      historyRetryCount++;
      if (historyRetryCount >= width / 5)
      {
        return false;
      }

      candidateX = random.Next(minimumX, maximumXExclusive);
    }
  }

  private static bool IsOutsideCenterExclusion(int candidateX, int width)
  {
    return candidateX <= width / 2 - CenterExclusionRadius ||
      candidateX >= width / 2 + CenterExclusionRadius;
  }
}
