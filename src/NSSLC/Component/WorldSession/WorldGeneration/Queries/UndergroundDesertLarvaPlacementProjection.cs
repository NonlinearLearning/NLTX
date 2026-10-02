using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Projects committed larva coordinates into ordered tile-placement commands.
/// </summary>
public static class UndergroundDesertLarvaPlacementProjection
{
  private const int FoundationTileType = 225;
  private const int LarvaObjectTileType = 231;
  private const int FootprintWidth = 3;
  private const int FootprintHeight = 4;

  public static UndergroundDesertLarvaPlacementCommand[] CreateCommands(
    in UndergroundDesertLarvaPlacementSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot.Positions);
    if (snapshot.GenerationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    if (snapshot.Count < 0 ||
      snapshot.Count > UndergroundDesertLarvaPlacementComponent.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    if (snapshot.Positions.Count != snapshot.Count)
    {
      throw new ArgumentException(
        "Larva positions must cover the complete used range.",
        nameof(snapshot));
    }

    UndergroundDesertLarvaPlacementCommand[] commands =
      new UndergroundDesertLarvaPlacementCommand[snapshot.Count];
    for (int index = 0; index < snapshot.Count; index++)
    {
      commands[index] = CreateCommand(snapshot.GenerationId, snapshot.Positions[index]);
    }

    return commands;
  }

  private static UndergroundDesertLarvaPlacementCommand CreateCommand(
    long generationId,
    TilePosition anchor)
  {
    List<UndergroundDesertLarvaTileMutation> mutations =
      new(FootprintWidth * FootprintHeight + 1);
    for (int horizontalOffset = -1; horizontalOffset <= 1; horizontalOffset++)
    {
      for (int verticalOffset = -2; verticalOffset <= 1; verticalOffset++)
      {
        TilePosition target = new(
          checked(anchor.X + horizontalOffset),
          checked(anchor.Y + verticalOffset));
        mutations.Add(
          verticalOffset == 1
            ? UndergroundDesertLarvaTileMutation.ConfigureFoundationTile(target)
            : UndergroundDesertLarvaTileMutation.DeactivateTile(target));
      }
    }

    mutations.Add(UndergroundDesertLarvaTileMutation.PlaceLarvaObject(anchor));
    return UndergroundDesertLarvaPlacementCommand.FromProjection(
      generationId,
      anchor,
      mutations);
  }
}
