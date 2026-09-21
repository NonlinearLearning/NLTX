using System;

namespace Terraria.WorldGeneration.Components;

public sealed class ShimmerBiomeAnchorComponent
{
  public ShimmerBiomeAnchorComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool HasAnchor { get; private set; }

  public ShimmerBiomeAnchorPoint Position { get; private set; }

  public ShimmerBiomeAnchorSnapshot CreateSnapshot()
  {
    return new ShimmerBiomeAnchorSnapshot(
      GenerationId,
      HasAnchor,
      Position);
  }

  internal void Publish(ShimmerBiomeAnchorPoint position)
  {
    Position = position;
    HasAnchor = true;
  }

  internal void Clear()
  {
    Position = default;
    HasAnchor = false;
  }
}
