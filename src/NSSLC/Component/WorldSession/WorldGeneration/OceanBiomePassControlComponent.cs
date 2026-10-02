using System;

namespace Terraria.WorldGeneration.Components;

public sealed class OceanBiomePassControlComponent
{
  public OceanBiomePassControlComponent(
    long generationId,
    bool skipDesertTileCheck = false)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    SkipDesertTileCheck = skipDesertTileCheck;
  }

  public long GenerationId { get; }

  public bool SkipDesertTileCheck { get; private set; }

  internal void SetSkipDesertTileCheck(bool value)
  {
    SkipDesertTileCheck = value;
  }

  internal void Reset()
  {
    SkipDesertTileCheck = false;
  }
}
