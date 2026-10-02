using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Biomes;

public sealed class BiomeStructurePlacementDefinition
{
  public BiomeStructurePlacementDefinition(
    IEnumerable<int>? blacklistedTiles = null,
    IEnumerable<int>? beelistedTiles = null,
    double iceChestChance = 0d,
    double jungleChestChance = 0d,
    double goldChestChance = 0d,
    double graniteChestChance = 0d,
    double marbleChestChance = 0d,
    double mushroomChestChance = 0d,
    double desertChestChance = 0d)
  {
    BlacklistedTiles = CopyTileIds(blacklistedTiles, nameof(blacklistedTiles));
    BeelistedTiles = CopyTileIds(beelistedTiles, nameof(beelistedTiles));
    EnsureSubset(BeelistedTiles, BlacklistedTiles);
    IceChestChance = ValidateChance(iceChestChance, nameof(iceChestChance));
    JungleChestChance = ValidateChance(jungleChestChance, nameof(jungleChestChance));
    GoldChestChance = ValidateChance(goldChestChance, nameof(goldChestChance));
    GraniteChestChance = ValidateChance(graniteChestChance, nameof(graniteChestChance));
    MarbleChestChance = ValidateChance(marbleChestChance, nameof(marbleChestChance));
    MushroomChestChance = ValidateChance(mushroomChestChance, nameof(mushroomChestChance));
    DesertChestChance = ValidateChance(desertChestChance, nameof(desertChestChance));
  }

  public IReadOnlyList<int> BlacklistedTiles { get; }

  public IReadOnlyList<int> BeelistedTiles { get; }

  public double IceChestChance { get; }

  public double JungleChestChance { get; }

  public double GoldChestChance { get; }

  public double GraniteChestChance { get; }

  public double MarbleChestChance { get; }

  public double MushroomChestChance { get; }

  public double DesertChestChance { get; }

  public static BiomeStructurePlacementDefinition CreateDefault()
  {
    return new BiomeStructurePlacementDefinition(
      blacklistedTiles: [225, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467],
      beelistedTiles: [41, 43, 44, 226, 203, 112, 25, 151, 21, 467]);
  }

  private static IReadOnlyList<int> CopyTileIds(
    IEnumerable<int>? values,
    string parameterName)
  {
    int[] copy = values?.Distinct().ToArray() ?? Array.Empty<int>();
    if (copy.Any(static value => value < 0))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return new ReadOnlyCollection<int>(copy);
  }

  private static double ValidateChance(double value, string parameterName)
  {
    if (!double.IsFinite(value) || value < 0d || value > 1d)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }

  private static void EnsureSubset(
    IReadOnlyList<int> subset,
    IReadOnlyList<int> superset)
  {
    HashSet<int> values = superset.ToHashSet();
    if (subset.Any(value => !values.Contains(value)))
    {
      throw new ArgumentException("Beelisted tiles must be included in the blacklist.");
    }
  }
}
