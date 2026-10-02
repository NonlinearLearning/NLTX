using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Dungeon.Catalogs;

public sealed class DungeonRoomVariantCatalog
{
  public DungeonRoomVariantCatalog(
    int baseInnerSize,
    int templeInnerSize,
    int wallDepth,
    IEnumerable<DungeonRoomVariantId> variants,
    int maxVariants)
  {
    if (baseInnerSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baseInnerSize));
    }

    if (templeInnerSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(templeInnerSize));
    }

    if (wallDepth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wallDepth));
    }

    if (maxVariants <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxVariants));
    }

    ArgumentNullException.ThrowIfNull(variants);
    DungeonRoomVariantId[] copy = variants.ToArray();
    if (copy.Length != maxVariants || copy.Distinct().Count() != copy.Length)
    {
      throw new ArgumentException(
        "The room variant list must match the declared maximum and be unique.",
        nameof(variants));
    }

    BaseInnerSize = baseInnerSize;
    TempleInnerSize = templeInnerSize;
    WallDepth = wallDepth;
    Variants = new ReadOnlyCollection<DungeonRoomVariantId>(copy);
    MaxVariants = maxVariants;
  }

  public int BaseInnerSize { get; }

  public int TempleInnerSize { get; }

  public int WallDepth { get; }

  public IReadOnlyList<DungeonRoomVariantId> Variants { get; }

  public int MaxVariants { get; }

  public static DungeonRoomVariantCatalog CreateDefault()
  {
    return new DungeonRoomVariantCatalog(
      baseInnerSize: 32,
      templeInnerSize: 50,
      wallDepth: 8,
      variants:
      [
        DungeonRoomVariantId.DoubleDiamond,
        DungeonRoomVariantId.Rounded,
        DungeonRoomVariantId.Candy,
        DungeonRoomVariantId.Wiggled
      ],
      maxVariants: 4);
  }
}
