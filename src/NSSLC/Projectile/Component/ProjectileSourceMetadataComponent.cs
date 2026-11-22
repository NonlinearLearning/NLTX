using System;

namespace Terraria.Projectile;

/// <summary>Captures immutable source attribution for one projectile instance.</summary>
public readonly record struct ProjectileSourceMetadataComponent
{
  public ProjectileSourceMetadataComponent(
      string sourceKind,
      string sourceId,
      long sourceRevision = 0)
  {
    if (string.IsNullOrWhiteSpace(sourceKind))
    {
      throw new ArgumentException("A projectile source kind is required.", nameof(sourceKind));
    }

    if (string.IsNullOrWhiteSpace(sourceId))
    {
      throw new ArgumentException("A projectile source id is required.", nameof(sourceId));
    }

    if (sourceRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceRevision));
    }

    SourceKind = sourceKind;
    SourceId = sourceId;
    SourceRevision = sourceRevision;
  }

  public string SourceKind { get; }

  public string SourceId { get; }

  public long SourceRevision { get; }
}
