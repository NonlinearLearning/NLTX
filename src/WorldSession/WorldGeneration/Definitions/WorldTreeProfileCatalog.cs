using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Definitions;

public sealed class WorldTreeProfileCatalog
{
  private static readonly IReadOnlyList<WorldTreeProfileDefinition> Version4Profiles =
    CreateVersion4Profiles();

  private readonly FrozenDictionary<WorldTreeProfileId, WorldTreeProfileDefinition>
    _profilesById;

  private readonly FrozenDictionary<ushort, WorldTreeProfileDefinition>
    _profilesByTreeTileType;

  public WorldTreeProfileCatalog(
    IReadOnlyList<WorldTreeProfileDefinition>? profiles = null)
  {
    IReadOnlyList<WorldTreeProfileDefinition> source = profiles ?? Version4Profiles;
    ArgumentNullException.ThrowIfNull(source);
    List<WorldTreeProfileDefinition> orderedProfiles = new(source.Count);
    Dictionary<WorldTreeProfileId, WorldTreeProfileDefinition> byId = new();
    Dictionary<ushort, WorldTreeProfileDefinition> byTreeTileType = new();
    foreach (WorldTreeProfileDefinition profile in source)
    {
      if (!byId.TryAdd(profile.Id, profile))
      {
        throw new ArgumentException(
          "Tree profile identifiers must be unique.",
          nameof(profiles));
      }

      if (!byTreeTileType.TryAdd(profile.TreeTileType, profile))
      {
        throw new ArgumentException(
          "Tree tile types must identify one profile each.",
          nameof(profiles));
      }

      orderedProfiles.Add(profile);
    }

    Profiles = orderedProfiles.AsReadOnly();
    _profilesById = byId.ToFrozenDictionary();
    _profilesByTreeTileType = byTreeTileType.ToFrozenDictionary();
  }

  public static WorldTreeProfileCatalog Version4 { get; } = new();

  public IReadOnlyList<WorldTreeProfileDefinition> Profiles { get; }

  public bool TryGet(
    WorldTreeProfileId id,
    out WorldTreeProfileDefinition profile)
  {
    return _profilesById.TryGetValue(id, out profile);
  }

  public bool TryGetFromTreeId(
    int treeTileType,
    out WorldTreeProfileDefinition profile)
  {
    if (treeTileType < ushort.MinValue || treeTileType > ushort.MaxValue)
    {
      profile = default;
      return false;
    }

    return _profilesByTreeTileType.TryGetValue((ushort)treeTileType, out profile);
  }

  private static IReadOnlyList<WorldTreeProfileDefinition> CreateVersion4Profiles()
  {
    const int minimumHeight = 7;
    const int maximumHeight = 12;
    const int topPaddingNeeded = 4;
    return new List<WorldTreeProfileDefinition>
    {
      new(
        WorldTreeProfileId.GemTreeRuby,
        587,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.GemTreeDiamond,
        588,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.GemTreeTopaz,
        583,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.GemTreeAmethyst,
        584,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.GemTreeSapphire,
        585,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.GemTreeEmerald,
        586,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.GemTreeAmber,
        589,
        590,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.GemTreeGround,
        WorldTreeWallPredicateId.GemTreeWall),
      new(
        WorldTreeProfileId.VanityTreeSakura,
        596,
        595,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.VanityTreeGround,
        WorldTreeWallPredicateId.DefaultTreeWall),
      new(
        WorldTreeProfileId.VanityTreeWillow,
        616,
        615,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.VanityTreeGround,
        WorldTreeWallPredicateId.DefaultTreeWall),
      new(
        WorldTreeProfileId.TreeAsh,
        634,
        20,
        minimumHeight,
        maximumHeight,
        topPaddingNeeded,
        WorldTreeGroundPredicateId.AshTreeGround,
        WorldTreeWallPredicateId.DefaultTreeWall),
    }.AsReadOnly();
  }
}
