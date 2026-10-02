namespace Terraria.WorldGeneration.Terrain;

public sealed class DunesPlacementDefinition
{
  public DunesPlacementDefinition(int singleDunesWidthMinimum = 0, int singleDunesWidthMaximum = 0)
  {
    if (singleDunesWidthMinimum < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(singleDunesWidthMinimum));
    }

    if (singleDunesWidthMaximum < singleDunesWidthMinimum)
    {
      throw new ArgumentOutOfRangeException(nameof(singleDunesWidthMaximum));
    }

    SingleDunesWidthMinimum = singleDunesWidthMinimum;
    SingleDunesWidthMaximum = singleDunesWidthMaximum;
  }

  public int SingleDunesWidthMinimum { get; }

  public int SingleDunesWidthMaximum { get; }

  public int MaximumWidth => checked(SingleDunesWidthMaximum * 2);
}
