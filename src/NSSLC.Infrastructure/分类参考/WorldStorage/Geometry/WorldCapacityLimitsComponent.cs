namespace Terraria.NonAuthoritative.WorldStorage.Geometry;

public sealed class WorldCapacityLimitsComponent
{
  public WorldCapacityLimitsComponent(int maxNetPlayers, int maxNpcCount)
  {
    if (maxNetPlayers <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxNetPlayers));
    }

    if (maxNpcCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxNpcCount));
    }

    MaxNetPlayers = maxNetPlayers;
    MaxNpcCount = maxNpcCount;
  }

  public int MaxNetPlayers { get; }

  public int MaxNpcCount { get; }
}
