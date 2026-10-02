namespace Terraria.WorldSession.Runtime;

public readonly record struct HostCapacityDefinition
{
  public int MaxNpcs { get; }

  public HostCapacityDefinition(int maxNpcs)
  {
    MaxNpcs = maxNpcs <= 0
      ? throw new ArgumentOutOfRangeException(nameof(maxNpcs))
      : maxNpcs;
  }
}
