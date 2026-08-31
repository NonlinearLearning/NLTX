namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonBookshelfSize
{
  public DungeonBookshelfSize(int minimum, int maximum)
  {
    Minimum = minimum;
    Maximum = maximum;
  }

  public int Minimum { get; }

  public int Maximum { get; }
}

public static class DungeonBookshelfSizeQuery
{
  public static DungeonBookshelfSize GetDefault(int defaultMinimum, int defaultMaximum)
  {
    return new DungeonBookshelfSize(defaultMinimum, defaultMaximum);
  }
}
