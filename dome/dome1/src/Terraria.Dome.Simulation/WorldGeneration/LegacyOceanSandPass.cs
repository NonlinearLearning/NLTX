using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOceanSandPass
{
  private const int MinimumWidth = 35;
  private const int MaximumWidthExclusive = 90;
  private const int MinimumDepth = 50;
  private const int MaximumDepthExclusive = 100;
  private const int MinimumDepthClamp = 50;
  private const int MaximumDepthClamp = 200;
  private const ushort SandTileType = 53;
  private const double CentralBandMinimum = 0.4;
  private const double CentralBandMaximum = 0.6;

  public const int SourceLine = 11913;

  public static LegacyOceanSandPassResult CreatePlan(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isSkyblockWorld,
    bool isNoSurfaceWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    long initialSampleCount = random.SampleCount;
    if (isSkyblockWorld || isNoSurfaceWorld)
    {
      return CreateResult(
        skipped: true,
        Array.Empty<LegacyOceanSandBandPlan>(),
        Array.Empty<LegacyOceanSandColumnPlan>(),
        Array.Empty<(int X, int Y)>(),
        Array.Empty<LegacyOceanSandTileWrite>(),
        random.SampleCount - initialSampleCount);
    }

    profile.Validate(snapshot.Metadata);
    List<LegacyOceanSandBandPlan> bands = new();
    List<LegacyOceanSandColumnPlan> columns = new();
    List<(int X, int Y)> pyramidCandidates = new();
    List<LegacyOceanSandTileWrite> tileWrites = new();
    double scanLimit = (profile.WorldSurface + profile.RockLayer) / 2.0;
    for (int iteration = 0; iteration < 3; iteration++)
    {
      long setupStart = random.SampleCount;
      int candidateX = random.Next(snapshot.Metadata.Width);
      int candidateRetryCount = 0;
      while (IsInsideCentralBand(candidateX, snapshot.Metadata.Width))
      {
        candidateX = random.Next(snapshot.Metadata.Width);
        candidateRetryCount++;
      }

      int leftWidth = random.Next(MinimumWidth, MaximumWidthExclusive);
      if (iteration == 1)
      {
        double worldScale = snapshot.Metadata.Width / 4200.0;
        leftWidth += (int)(random.Next(20, 40) * worldScale);
      }

      if (random.Next(3) == 0)
      {
        leftWidth *= 2;
      }

      if (iteration == 1)
      {
        leftWidth *= 2;
      }

      int rightWidth = random.Next(MinimumWidth, MaximumWidthExclusive);
      if (random.Next(3) == 0)
      {
        rightWidth *= 2;
      }

      if (iteration == 1)
      {
        rightWidth *= 2;
      }

      int left = Math.Max(0, candidateX - leftWidth);
      int rightExclusive = Math.Min(snapshot.Metadata.Width, candidateX + rightWidth);
      bool skipped = iteration == 1;
      if (iteration == 0)
      {
        left = 0;
        rightExclusive = profile.LeftBeachEnd;
      }
      else if (iteration == 2)
      {
        left = profile.RightBeachStart;
        rightExclusive = snapshot.Metadata.Width;
      }

      bands.Add(new LegacyOceanSandBandPlan(
        iteration,
        candidateX,
        candidateRetryCount,
        left,
        rightExclusive,
        leftWidth,
        rightWidth,
        skipped,
        checked((int)(random.SampleCount - setupStart))));
      if (skipped)
      {
        continue;
      }

      int depth = random.Next(MinimumDepth, MaximumDepthExclusive);
      int midpoint = (left + rightExclusive) / 2;
      for (int x = left; x < rightExclusive; x++)
      {
        long columnStart = random.SampleCount;
        if (random.Next(2) == 0)
        {
          depth += random.Next(-1, 2);
          depth = Math.Clamp(depth, MinimumDepthClamp, MaximumDepthClamp);
        }

        int firstActiveY = FindFirstActiveY(snapshot, x, scanLimit);
        if (firstActiveY < 0)
        {
          columns.Add(new LegacyOceanSandColumnPlan(
            iteration,
            x,
            depth,
            firstActiveY,
            0,
            false,
            0,
            checked((int)(random.SampleCount - columnStart))));
          continue;
        }

        bool pyramidCandidate = x == midpoint && random.Next(6) == 0;
        if (pyramidCandidate)
        {
          pyramidCandidates.Add((x, firstActiveY));
        }

        int carveDepth = depth;
        carveDepth = Math.Min(carveDepth, x - left);
        carveDepth = Math.Min(carveDepth, rightExclusive - x);
        carveDepth += random.Next(5);
        int eligibleTileCount = AppendTileWrites(
          snapshot,
          x,
          left,
          rightExclusive,
          firstActiveY,
          carveDepth,
          random,
          tileWrites);
        columns.Add(new LegacyOceanSandColumnPlan(
          iteration,
          x,
          depth,
          firstActiveY,
          carveDepth,
          pyramidCandidate,
          eligibleTileCount,
          checked((int)(random.SampleCount - columnStart))));
      }
    }

    return CreateResult(
      skipped: false,
      bands,
      columns,
      pyramidCandidates,
      tileWrites,
      random.SampleCount - initialSampleCount);
  }

  public static LegacyOceanSandPassResult AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isSkyblockWorld,
    bool isNoSurfaceWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    LegacyOceanSandPassResult result = CreatePlan(
      snapshot,
      profile,
      random,
      isSkyblockWorld,
      isNoSurfaceWorld);
    foreach (LegacyOceanSandTileWrite write in result.TileWrites)
    {
      WorldTile tile = snapshot.GetTile(write.X, write.Y);
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        write.X,
        write.Y,
        TileChangeKind.UpdateTileType,
        SandTileType,
        IsActive: tile.IsActive,
        Source: "worldgen.ocean-sand.type-53"));
    }

    return result;
  }

  private static LegacyOceanSandPassResult CreateResult(
    bool skipped,
    IReadOnlyList<LegacyOceanSandBandPlan> bands,
    IReadOnlyList<LegacyOceanSandColumnPlan> columns,
    IReadOnlyList<(int X, int Y)> pyramidCandidates,
    IReadOnlyList<LegacyOceanSandTileWrite> tileWrites,
    long randomSamplesConsumed)
  {
    return new LegacyOceanSandPassResult(
      skipped,
      Freeze(bands),
      Freeze(columns),
      Freeze(pyramidCandidates),
      Freeze(tileWrites),
      randomSamplesConsumed);
  }

  private static IReadOnlyList<T> Freeze<T>(IReadOnlyList<T> values)
  {
    if (values.Count == 0)
    {
      return Array.Empty<T>();
    }

    T[] copy = new T[values.Count];
    for (int index = 0; index < values.Count; index++)
    {
      copy[index] = values[index];
    }

    return Array.AsReadOnly(copy);
  }

  private static bool IsInsideCentralBand(int candidateX, int width)
  {
    return (double)candidateX > width * CentralBandMinimum &&
      (double)candidateX < width * CentralBandMaximum;
  }

  private static int FindFirstActiveY(
    WorldGridSnapshot snapshot,
    int x,
    double scanLimit)
  {
    for (int y = 0; (double)y < scanLimit; y++)
    {
      if (snapshot.GetTile(x, y).IsActive)
      {
        return y;
      }
    }

    return -1;
  }

  private static int AppendTileWrites(
    WorldGridSnapshot snapshot,
    int x,
    int left,
    int rightExclusive,
    int firstActiveY,
    int carveDepth,
    LegacyPassRandomState random,
    ICollection<LegacyOceanSandTileWrite> tileWrites)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileWrites);
    int eligibleTileCount = 0;
    int endY = checked(firstActiveY + carveDepth);
    for (int y = firstActiveY; y < endY; y++)
    {
      bool insideLeftBoundary = x > left + random.Next(5);
      bool insideRightBoundary = insideLeftBoundary &&
        x < rightExclusive - random.Next(5);
      if (insideLeftBoundary && insideRightBoundary && y >= 0 &&
          y < snapshot.Metadata.Height)
      {
        tileWrites.Add(new LegacyOceanSandTileWrite(x, y));
        eligibleTileCount++;
      }
    }

    return eligibleTileCount;
  }
}
