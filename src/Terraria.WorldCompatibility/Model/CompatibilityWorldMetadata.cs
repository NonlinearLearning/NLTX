using System;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldCompatibility.Model;

public sealed record CompatibilityWorldMetadata(
  string Name,
  int WorldId,
  int Width,
  int Height,
  int SpawnX,
  int SpawnY,
  int LeftWorld = 0,
  int RightWorld = 0,
  int TopWorld = 0,
  int BottomWorld = 0)
{
  public string Name { get; init; } = Name ?? throw new ArgumentNullException(nameof(Name));

  internal static CompatibilityWorldMetadata From(LegacyWorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return new CompatibilityWorldMetadata(
      metadata.Name,
      metadata.WorldId,
      metadata.Width,
      metadata.Height,
      metadata.SpawnX,
      metadata.SpawnY,
      metadata.LeftWorld,
      metadata.RightWorld,
      metadata.TopWorld,
      metadata.BottomWorld);
  }
}
