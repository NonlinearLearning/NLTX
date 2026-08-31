using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySurfaceCavesPass
{
  public const int SourceLine = 12676;

  public static int SourceAnchorLine => SourceLine;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld,
    bool isNoSurfaceWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    if (isSkyblockWorld || isNoSurfaceWorld)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    LegacySurfaceCavesPassDefinition definition =
      LegacySurfaceCavesPassDefinitionFactory.CreateDefault();
    definition.Validate();
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    LegacySurfaceCavesVerticalPass.AppendCommands(
      snapshot,
      profile,
      random,
      ref state,
      tileCommands,
      isRemixWorld,
      projectedTiles);
    LegacySurfaceCavesCavererPass.AppendCommands(
      snapshot,
      profile,
      random,
      (int)profile.WorldSurface,
      (int)profile.RockLayer,
      ref state,
      tileCommands,
      liquidCommands,
      projectedTiles);
  }
}
