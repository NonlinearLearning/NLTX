using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyIceBiomeSurfacePass
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyIceBiomeSurfaceDefinition definition,
    LegacyPassRandomState random,
    bool isSkyblockWorld,
    bool isRemixWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    definition.Validate();
    if (isSkyblockWorld)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    int top = Math.Clamp((int)profile.WorldSurface, 0, snapshot.Metadata.Height - 1);
    int bottom = CalculateBottom(snapshot.Metadata.Height, profile.LavaLine, isRemixWorld);
    int conversionBottom = CalculateConversionBottom(bottom, random, top, snapshot.Metadata.Height);
    int snowMinX = profile.InitialSnowOriginLeft ?? profile.LeftBeachEnd;
    int snowMaxX = profile.InitialSnowOriginRight ?? profile.RightBeachStart;
    int previousSnowMinX = snowMinX;
    int previousSnowMaxX = snowMaxX;
    int lowerBandHeight = 10;
    for (int y = 0; y <= bottom - 140; y++)
    {
      snowMinX += random.Next(-4, 4);
      snowMaxX += random.Next(-3, 5);
      if (y > 0)
      {
        snowMinX = (snowMinX + previousSnowMinX) / 2;
        snowMaxX = (snowMaxX + previousSnowMaxX) / 2;
      }

      previousSnowMinX = snowMinX;
      previousSnowMaxX = snowMaxX;
      int minimumX = Math.Clamp(snowMinX, 0, snapshot.Metadata.Width);
      int maximumX = Math.Clamp(snowMaxX, minimumX, snapshot.Metadata.Width);
      if (y >= conversionBottom)
      {
        lowerBandHeight += random.Next(-3, 4);
        if (random.Next(3) == 0)
        {
          lowerBandHeight += random.Next(-4, 5);
          if (random.Next(3) == 0)
          {
            lowerBandHeight += random.Next(-6, 7);
          }
        }

        if (lowerBandHeight < 0)
        {
          lowerBandHeight = random.Next(3);
        }
        else if (lowerBandHeight > 50)
        {
          lowerBandHeight = 50 - random.Next(3);
        }
      }

      for (int x = minimumX; x < maximumX; x++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.WallType == 2)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.SetWall, 0,
            WallType: (ushort)definition.SnowWallType,
            Source: "worldgen.biome.IceBiome"));
        }

        ushort replacement = tile.Type switch
        {
          0 or 2 or 23 or 40 or 53 => (ushort)definition.SnowTileType,
          1 => (ushort)definition.IceTileType,
          _ => 0
        };
        if (y >= conversionBottom)
        {
          for (int lowerY = y; lowerY < y + lowerBandHeight; lowerY++)
          {
            if (lowerY >= snapshot.Metadata.Height)
            {
              break;
            }

            WorldTile lowerTile = snapshot.GetTile(x, lowerY);
            if (lowerTile.WallType == 2)
            {
              commands.Add(new TileChangeCommand(
                state.ReserveSequence(), x, lowerY, TileChangeKind.SetWall, 0,
                WallType: (ushort)definition.SnowWallType,
                Source: "worldgen.biome.IceBiome"));
            }

            ushort lowerReplacement = lowerTile.Type switch
            {
              0 or 2 or 23 or 40 or 53 => (ushort)definition.SnowTileType,
              1 => (ushort)definition.IceTileType,
              _ => 0
            };
            if (lowerTile.IsActive && lowerReplacement != 0)
            {
              commands.Add(new TileChangeCommand(
                state.ReserveSequence(), x, lowerY, TileChangeKind.UpdateTileType,
                lowerReplacement, IsActive: lowerTile.IsActive,
                Source: "worldgen.biome.IceBiome"));
            }
          }
        }
        else if (tile.IsActive && replacement != 0)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType, replacement,
            IsActive: tile.IsActive,
            Source: "worldgen.biome.IceBiome"));
        }
      }
    }
  }

  public static int CalculateBottom(
    int height,
    int lavaLine,
    bool isRemixWorld)
  {
    if (height <= 0 || lavaLine < 0 || lavaLine >= height)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    int upperLayer = isRemixWorld ? height - 250 : lavaLine;
    return Math.Clamp(upperLayer, 1, height);
  }

  public static int CalculateConversionBottom(
    int bottom,
    LegacyPassRandomState random,
    int top,
    int height)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (bottom <= 0 || top < 0 || top >= height)
    {
      throw new ArgumentOutOfRangeException(nameof(bottom));
    }

    return Math.Clamp(bottom - random.Next(160, 200), top + 1, height);
  }
}
