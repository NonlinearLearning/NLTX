using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class BiomeEcologyStateComponent
{
  private IReadOnlySet<string> _biomeTags = FrozenSet<string>.Empty;

  public BiomeEcologyStateComponent(
    long generationId,
    IReadOnlySet<string>? biomeTags = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    _biomeTags = CopyTags(biomeTags);
  }

  public long GenerationId { get; }

  public ulong BiomeRevision { get; init; }

  public IReadOnlySet<string> BiomeTags
  {
    get => _biomeTags;
    init => _biomeTags = CopyTags(value);
  }

  public bool? WorldIsInfected { get; init; }

  public WorldEvilType? WorldEvil { get; init; }

  public ulong ConversionRevision { get; init; }

  public int? TotalEvil { get; init; }

  public int? TotalBlood { get; init; }

  public int? TotalGood { get; init; }

  public int? TotalSolid { get; init; }

  public ulong? TilePresenceRevision { get; init; }

  private static IReadOnlySet<string> CopyTags(IReadOnlySet<string>? source)
  {
    if (source is null)
    {
      return FrozenSet<string>.Empty;
    }

    HashSet<string> copy = new(StringComparer.Ordinal);
    foreach (string tag in source)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(tag);
      copy.Add(tag);
    }

    return copy.ToFrozenSet(StringComparer.Ordinal);
  }
}
