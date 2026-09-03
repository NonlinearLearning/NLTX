using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySurfaceCavesCavererPass
{
  public const int SurfaceCavesBeachAvoidance = 340;
  private const int ReferenceWorldWidth = 4200;
  private const int ReferenceInvocationCount = 5;
  private const int UnderworldReservationHeight = 400;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands,
    IDictionary<(int X, int Y), WorldTile>? projectedTiles = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    profile.Validate(snapshot.Metadata);
    projectedTiles ??= new Dictionary<(int X, int Y), WorldTile>();
    int minimumX = SurfaceCavesBeachAvoidance;
    int maximumXExclusive = snapshot.Metadata.Width - SurfaceCavesBeachAvoidance;
    int maximumYExclusive = snapshot.Metadata.Height - UnderworldReservationHeight;
    int minimumY = (int)profile.RockLayer;
    if (maximumXExclusive <= minimumX || maximumYExclusive <= 0)
    {
      return;
    }

    if (minimumY >= maximumYExclusive)
    {
      minimumY = maximumYExclusive - 1;
    }

    if (minimumY < 0 || minimumY >= maximumYExclusive)
    {
      return;
    }

    int count = CalculateInvocationCount(snapshot.Metadata.Width);
    for (int index = 0; index < count; index++)
    {
      int startX = random.Next(minimumX, maximumXExclusive);
      int startY = random.Next(minimumY, maximumYExclusive);
      int tileCommandStart = tileCommands.Count;
      int liquidCommandStart = liquidCommands.Count;
      LegacyCaverer.AppendCommands(
          snapshot,
          startX,
          startY,
          random,
          worldSurfaceY,
          rockLayerY,
          ref state,
          tileCommands,
          liquidCommands,
          projectedTiles);
      RewriteCavererSources(tileCommands, tileCommandStart);
      RewriteCavererLiquidSources(liquidCommands, liquidCommandStart);
    }
  }

  public static int CalculateInvocationCount(int width)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return (int)(width * (double)ReferenceInvocationCount / ReferenceWorldWidth);
  }

  private static void RewriteCavererSources(
    List<TileChangeCommand> tileCommands,
    int commandStart)
  {
    const string sourcePrefix = "worldgen.cave.SurfaceCaves.Caverer.";
    for (int index = commandStart; index < tileCommands.Count; index++)
    {
      TileChangeCommand command = tileCommands[index];
      string source = RemoveCavererSourcePrefix(command.Source);
      tileCommands[index] = command with { Source = sourcePrefix + source };
    }
  }

  private static void RewriteCavererLiquidSources(
    List<LiquidChangeCommand> liquidCommands,
    int commandStart)
  {
    const string sourcePrefix = "worldgen.cave.SurfaceCaves.Caverer.";
    for (int index = commandStart; index < liquidCommands.Count; index++)
    {
      LiquidChangeCommand command = liquidCommands[index];
      string source = RemoveCavererSourcePrefix(command.Source);
      liquidCommands[index] = command with { Source = sourcePrefix + source };
    }
  }

  private static string RemoveCavererSourcePrefix(string source)
  {
    const string cavererPrefix = "worldgen.cave.Caverer.";
    const string cavePrefix = "worldgen.cave.";
    if (source.StartsWith(cavererPrefix, StringComparison.Ordinal))
    {
      return source[cavererPrefix.Length..];
    }

    return source.StartsWith(cavePrefix, StringComparison.Ordinal)
      ? source[cavePrefix.Length..]
      : source;
  }
}
