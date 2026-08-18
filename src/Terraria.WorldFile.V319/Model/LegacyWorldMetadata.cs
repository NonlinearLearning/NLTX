using System;

namespace Terraria.WorldFile.V319.Model;

public sealed record LegacyWorldMetadata(
  string Name,
  int WorldId,
  int Width,
  int Height,
  int SpawnX,
  int SpawnY,
  int LeftWorld = 0,
  int RightWorld = 0,
  int TopWorld = 0,
  int BottomWorld = 0,
  double WorldSurface = 0,
  double RockLayer = 0)
{
  public string Name { get; init; } = Name ?? throw new ArgumentNullException(nameof(Name));
}
