using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

/// <summary>
/// Carries one immutable underground-desert larva tile-placement request.
/// </summary>
public readonly record struct UndergroundDesertLarvaPlacementCommand
{
  private UndergroundDesertLarvaPlacementCommand(
    long generationId,
    TilePosition anchor,
    IReadOnlyList<UndergroundDesertLarvaTileMutation> mutations)
  {
    GenerationId = generationId;
    Anchor = anchor;
    Mutations = mutations;
  }

  public long GenerationId { get; }

  public TilePosition Anchor { get; }

  public IReadOnlyList<UndergroundDesertLarvaTileMutation> Mutations { get; }

  internal static UndergroundDesertLarvaPlacementCommand FromProjection(
    long generationId,
    TilePosition anchor,
    IReadOnlyList<UndergroundDesertLarvaTileMutation> mutations)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(mutations);
    if (mutations.Count == 0)
    {
      throw new ArgumentException(
        "A larva placement command requires at least one tile mutation.",
        nameof(mutations));
    }

    List<UndergroundDesertLarvaTileMutation> copy = new(mutations.Count);
    for (int index = 0; index < mutations.Count; index++)
    {
      UndergroundDesertLarvaTileMutation mutation = mutations[index];
      mutation.Validate();
      copy.Add(mutation);
    }

    return new UndergroundDesertLarvaPlacementCommand(
      generationId,
      anchor,
      copy.AsReadOnly());
  }
}
