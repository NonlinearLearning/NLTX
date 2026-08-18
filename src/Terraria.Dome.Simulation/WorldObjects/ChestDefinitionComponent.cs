using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestDefinitionComponent
{
  public const int DefaultMaximumNameLength = 20;

  public ChestDefinitionComponent(
    ChestKind kind,
    int maximumNameLength = DefaultMaximumNameLength)
  {
    if (maximumNameLength < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumNameLength));
    }

    Kind = kind;
    MaximumNameLength = maximumNameLength;
  }

  public ChestKind Kind { get; }

  public int MaximumNameLength { get; }

  public static ChestDefinitionComponent World => new(ChestKind.World);
}
