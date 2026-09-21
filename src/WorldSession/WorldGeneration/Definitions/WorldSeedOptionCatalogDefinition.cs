using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Definitions;

public sealed class WorldSeedOptionCatalogDefinition
{
  private static readonly IReadOnlyList<WorldSeedOptionDefinition> Version4Options =
    CreateVersion4Options();

  public WorldSeedOptionCatalogDefinition(
    IReadOnlyList<WorldSeedOptionDefinition>? options = null)
  {
    IReadOnlyList<WorldSeedOptionDefinition> source = options ?? Version4Options;
    ArgumentNullException.ThrowIfNull(source);
    List<WorldSeedOptionDefinition> copy = new(source.Count);
    HashSet<WorldSeedOptionId> ids = new();
    foreach (WorldSeedOptionDefinition? option in source)
    {
      ArgumentNullException.ThrowIfNull(option);
      if (!ids.Add(option.Id))
      {
        throw new ArgumentException(
          "World-seed option identifiers must be unique.",
          nameof(options));
      }

      copy.Add(option);
    }

    Options = copy.AsReadOnly();
  }

  public static WorldSeedOptionCatalogDefinition Version4 { get; } = new();

  public IReadOnlyList<WorldSeedOptionDefinition> Options { get; }

  public bool TryGet(
    WorldSeedOptionId id,
    out WorldSeedOptionDefinition? option)
  {
    foreach (WorldSeedOptionDefinition candidate in Options)
    {
      if (candidate.Id == id)
      {
        option = candidate;
        return true;
      }
    }

    option = null;
    return false;
  }

  private static IReadOnlyList<WorldSeedOptionDefinition> CreateVersion4Options()
  {
    return new List<WorldSeedOptionDefinition>
    {
      new(WorldSeedOptionId.Normal, "Seed_Normal", null),
      new(
        WorldSeedOptionId.NotTheBees,
        "Seed_NotTheBees",
        "notthebees",
        ["notthebees"]),
      new(
        WorldSeedOptionId.Drunk,
        "Seed_Drunk",
        "drunk",
        specialSeedValues: [5162020]),
      new(
        WorldSeedOptionId.Anniversary,
        "Seed_Celebration",
        "celebration",
        ["celebrationmk10"],
        [5162021, 5162011]),
      new(
        WorldSeedOptionId.DontStarve,
        "Seed_TheConstant",
        "theconstant",
        ["constant", "theconstant", "eye4aneye", "eyeforaneye"]),
      new(
        WorldSeedOptionId.ForTheWorthy,
        "Seed_ForTheWorthy",
        "fortheworthy",
        ["fortheworthy"]),
      new(
        WorldSeedOptionId.NoTraps,
        "Seed_NoTraps",
        "notraps",
        ["notraps"]),
      new(
        WorldSeedOptionId.Remix,
        "Seed_Remix",
        "remix",
        ["dontdigup"]),
      new(
        WorldSeedOptionId.Everything,
        "Seed_Everything",
        "zenith",
        ["getfixedboi"],
        dependencies:
        [
          WorldSeedOptionId.Remix,
          WorldSeedOptionId.Drunk,
          WorldSeedOptionId.NotTheBees,
          WorldSeedOptionId.NoTraps,
          WorldSeedOptionId.DontStarve,
          WorldSeedOptionId.Anniversary,
          WorldSeedOptionId.ForTheWorthy,
        ]),
      new(
        WorldSeedOptionId.Skyblock,
        "Seed_Skyblock",
        "skyblock",
        ["skyblock"]),
    }.AsReadOnly();
  }
}
