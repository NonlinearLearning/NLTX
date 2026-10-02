namespace Terraria.WorldSession.Session;

public readonly record struct WorldLoadBudgetPolicy
{
  public int MaxLoadWorld { get; }

  public WorldLoadBudgetPolicy(int maxLoadWorld)
  {
    MaxLoadWorld = maxLoadWorld < 0
      ? throw new ArgumentOutOfRangeException(nameof(maxLoadWorld))
      : maxLoadWorld;
  }
}
