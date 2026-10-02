namespace Terraria.WorldSession.Seeds;

public sealed class WorldRandomPort : IWorldRandomPort
{
  private readonly Random _random;

  public WorldRandomPort(int seed)
  {
    _random = new Random(seed);
  }

  public int Next(int exclusiveUpperBound)
  {
    return _random.Next(exclusiveUpperBound);
  }

  public double NextDouble()
  {
    return _random.NextDouble();
  }
}
