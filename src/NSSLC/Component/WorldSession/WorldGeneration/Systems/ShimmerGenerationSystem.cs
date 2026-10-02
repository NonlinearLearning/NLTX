using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class ShimmerGenerationSystem
{
  public static ShimmerBiomeCommitResult CommitAfterSuccessfulBiome(
    ShimmerBiomeAnchorComponent component,
    ShimmerBiomeAnchorPoint position,
    bool biomeCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!biomeCommitted)
    {
      return new(
        ShimmerBiomeCommitStatus.RejectedBiomeCommit,
        BiomeCommitSucceeded: false,
        Published: false);
    }

    component.Publish(position);
    return new(
      ShimmerBiomeCommitStatus.Published,
      BiomeCommitSucceeded: true,
      Published: true);
  }

  public static void Reset(ShimmerBiomeAnchorComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
