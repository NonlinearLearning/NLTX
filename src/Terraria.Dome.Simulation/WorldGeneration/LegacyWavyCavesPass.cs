using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWavyCavesPass
{
  public static int CalculateInvocationCount(int width, bool isRemixWorld = false)
  {
    return LegacyWavyCavesPassDefinition.CreateDefault().CalculateInvocationCount(
      width,
      isRemixWorld);
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    LegacyWavyCavesPassDefinition definition,
    bool isRemixWorld,
    bool isSkyblockWorld,
    bool isDontStarveWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(commands);
    definition.Validate();
    if (isSkyblockWorld || !isDontStarveWorld)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    int invocationCount = definition.CalculateInvocationCount(snapshot.Metadata.Width, isRemixWorld);
    if (invocationCount <= 0)
    {
      return;
    }

    int minimumY = Math.Clamp((int)profile.WorldSurface + definition.MinimumYInset, 0,
      snapshot.Metadata.Height - 1);
    int underworldLayer = snapshot.Metadata.Height - 200;
    int maximumYExclusive = Math.Clamp(underworldLayer - definition.UnderworldYInset,
      minimumY + 1, snapshot.Metadata.Height);
    int previousY = 0;
    int startXRange = Math.Max(0, snapshot.Metadata.Width - definition.MinimumStartXInset * 2);
    for (int index = 0; index < invocationCount; index++)
    {
      double progress = invocationCount == 1 ? 0.0 : index / (double)(invocationCount - 1);
      int startX = definition.MinimumStartXInset + (int)(startXRange * progress);
      int startY = random.Next(minimumY, maximumYExclusive);
      int retries = 0;
      while (Math.Abs(startY - previousY) < definition.MinimumSpacing && retries++ < 100)
      {
        startY = random.Next(minimumY, maximumYExclusive);
      }

      previousY = startY;
      LegacyWavyCaverer.AppendCommands(
        snapshot,
        new LegacyWavyCavererInvocation(
          startX,
          startY,
          12 + random.Next(3, 6),
          0.25 + random.NextDouble(),
          random.Next(300, 500),
          -1),
        random,
        ref state,
        commands);
    }
  }
}
