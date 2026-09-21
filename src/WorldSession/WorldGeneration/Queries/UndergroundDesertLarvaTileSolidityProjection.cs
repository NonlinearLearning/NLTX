using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Projects the sourced tile-solidity updates for one larva placement pass.
/// </summary>
public static class UndergroundDesertLarvaTileSolidityProjection
{
  public static IReadOnlyList<UndergroundDesertLarvaTileSolidityCommand> CreateCommands(
    long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    UndergroundDesertLarvaTileSolidityCommand[] commands =
    [
      UndergroundDesertLarvaTileSolidityCommand.BeforePlacement(generationId),
      UndergroundDesertLarvaTileSolidityCommand.AfterPlacement(
        generationId,
        tileTypeId: 232),
      UndergroundDesertLarvaTileSolidityCommand.AfterPlacement(
        generationId,
        tileTypeId: 162),
    ];

    foreach (UndergroundDesertLarvaTileSolidityCommand command in commands)
    {
      command.Validate();
    }

    return Array.AsReadOnly(commands);
  }
}
