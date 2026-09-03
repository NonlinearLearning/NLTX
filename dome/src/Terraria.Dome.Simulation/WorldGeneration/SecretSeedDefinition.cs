using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SecretSeedDefinition
{
  public SecretSeedDefinition(
    string variant,
    string sourceAnchor,
    SecretSeedBehaviorStatus status)
    : this()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(variant);
    ArgumentException.ThrowIfNullOrWhiteSpace(sourceAnchor);
    Variant = variant;
    SourceAnchor = sourceAnchor;
    Status = status;
  }

  public string Variant { get; }

  public string SourceAnchor { get; }

  public SecretSeedBehaviorStatus Status { get; }
}
