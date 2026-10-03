using System.Collections.Frozen;

namespace Terraria.Network;

public sealed class SectionInterestProjection {
  private readonly FrozenSet<SectionCoordinate> _sections;

  public Guid WorldKey { get; }
  public long WorldGeneration { get; }
  public long Revision { get; }
  public IReadOnlySet<SectionCoordinate> Sections => _sections;

  public SectionInterestProjection(Guid worldKey, long worldGeneration, long revision,
      IEnumerable<SectionCoordinate> sections) {
    ArgumentOutOfRangeException.ThrowIfNegative(worldGeneration);
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    WorldKey = worldKey;
    WorldGeneration = worldGeneration;
    Revision = revision;
    _sections = sections.ToFrozenSet();
  }
}
