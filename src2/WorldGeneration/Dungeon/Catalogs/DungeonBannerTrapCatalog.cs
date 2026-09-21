using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Dungeon.Catalogs;

public sealed class DungeonBannerTrapCatalog
{
  public DungeonBannerTrapCatalog(
    IEnumerable<int> bannerStyles,
    IEnumerable<int> trapTypes)
  {
    BannerStyles = CopyDistinctValues(bannerStyles, nameof(bannerStyles));
    TrapTypes = CopyDistinctValues(trapTypes, nameof(trapTypes));
  }

  public IReadOnlyList<int> BannerStyles { get; }

  public IReadOnlyList<int> TrapTypes { get; }

  public static DungeonBannerTrapCatalog CreateDefault()
  {
    return new DungeonBannerTrapCatalog(
      [10, 11, 12, 13, 14, 15],
      [0]);
  }

  private static IReadOnlyList<int> CopyDistinctValues(
    IEnumerable<int> values,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values);
    int[] copy = values.ToArray();
    if (copy.Any(value => value < 0))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    if (copy.Distinct().Count() != copy.Length)
    {
      throw new ArgumentException(
        "Catalog values must be unique.",
        parameterName);
    }

    return new ReadOnlyCollection<int>(copy);
  }
}
