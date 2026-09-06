using System.Collections.Immutable;

namespace Terraria.Content;

public sealed record TileFramingDefinition(
  bool FrameImportant,
  byte LargeFrameCount = 0,
  bool MergeDirt = false,
  bool BlendAll = false,
  ImmutableArray<int> MergeTypeIds = default,
  bool Brick = false,
  bool Cracked = false);
