using System.Collections.ObjectModel;
using Terraria.WorldGeneration.Dungeon.Rooms;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Catalogs;

public sealed class DungeonFurnitureDefinition
{
  public DungeonFurnitureDefinition(
    DungeonStyleId style,
    IEnumerable<int>? windowPlatformItemTypes,
    int lockedBiomeChestType,
    int lockedBiomeChestStyle,
    int biomeChestItemType,
    int biomeChestLootItemType,
    IEnumerable<int>? chestItemTypes,
    IEnumerable<int>? doorItemTypes,
    IEnumerable<int>? platformItemTypes,
    IEnumerable<int>? chandelierItemTypes,
    IEnumerable<int>? lanternItemTypes,
    IEnumerable<int>? tableItemTypes,
    IEnumerable<int>? workbenchItemTypes,
    IEnumerable<int>? candleItemTypes,
    IEnumerable<int>? vaseOrStatueItemTypes,
    IEnumerable<int>? bookcaseItemTypes,
    IEnumerable<int>? chairItemTypes,
    IEnumerable<int>? bedItemTypes,
    IEnumerable<int>? pianoItemTypes,
    IEnumerable<int>? dresserItemTypes,
    IEnumerable<int>? sofaItemTypes,
    IEnumerable<int>? bathtubItemTypes,
    IEnumerable<int>? lampItemTypes,
    IEnumerable<int>? candelabraItemTypes,
    IEnumerable<int>? clockItemTypes,
    IEnumerable<int>? bannerItemTypes,
    DungeonRoomType biomeRoomType,
    IEnumerable<DungeonStyleId>? subStyles = null)
  {
    Style = style;
    WindowPlatformItemTypes = CopyItemTypes(windowPlatformItemTypes);
    LockedBiomeChestType = lockedBiomeChestType;
    LockedBiomeChestStyle = lockedBiomeChestStyle;
    BiomeChestItemType = biomeChestItemType;
    BiomeChestLootItemType = biomeChestLootItemType;
    ChestItemTypes = CopyItemTypes(chestItemTypes);
    DoorItemTypes = CopyItemTypes(doorItemTypes);
    PlatformItemTypes = CopyItemTypes(platformItemTypes);
    ChandelierItemTypes = CopyItemTypes(chandelierItemTypes);
    LanternItemTypes = CopyItemTypes(lanternItemTypes);
    TableItemTypes = CopyItemTypes(tableItemTypes);
    WorkbenchItemTypes = CopyItemTypes(workbenchItemTypes);
    CandleItemTypes = CopyItemTypes(candleItemTypes);
    VaseOrStatueItemTypes = CopyItemTypes(vaseOrStatueItemTypes);
    BookcaseItemTypes = CopyItemTypes(bookcaseItemTypes);
    ChairItemTypes = CopyItemTypes(chairItemTypes);
    BedItemTypes = CopyItemTypes(bedItemTypes);
    PianoItemTypes = CopyItemTypes(pianoItemTypes);
    DresserItemTypes = CopyItemTypes(dresserItemTypes);
    SofaItemTypes = CopyItemTypes(sofaItemTypes);
    BathtubItemTypes = CopyItemTypes(bathtubItemTypes);
    LampItemTypes = CopyItemTypes(lampItemTypes);
    CandelabraItemTypes = CopyItemTypes(candelabraItemTypes);
    ClockItemTypes = CopyItemTypes(clockItemTypes);
    BannerItemTypes = CopyItemTypes(bannerItemTypes);
    BiomeRoomType = biomeRoomType;
    SubStyles = CopyStyles(subStyles);
  }

  public DungeonStyleId Style { get; }

  public IReadOnlyList<int> WindowPlatformItemTypes { get; }

  public int LockedBiomeChestType { get; }

  public int LockedBiomeChestStyle { get; }

  public int BiomeChestItemType { get; }

  public int BiomeChestLootItemType { get; }

  public IReadOnlyList<int> ChestItemTypes { get; }

  public IReadOnlyList<int> DoorItemTypes { get; }

  public IReadOnlyList<int> PlatformItemTypes { get; }

  public IReadOnlyList<int> ChandelierItemTypes { get; }

  public IReadOnlyList<int> LanternItemTypes { get; }

  public IReadOnlyList<int> TableItemTypes { get; }

  public IReadOnlyList<int> WorkbenchItemTypes { get; }

  public IReadOnlyList<int> CandleItemTypes { get; }

  public IReadOnlyList<int> VaseOrStatueItemTypes { get; }

  public IReadOnlyList<int> BookcaseItemTypes { get; }

  public IReadOnlyList<int> ChairItemTypes { get; }

  public IReadOnlyList<int> BedItemTypes { get; }

  public IReadOnlyList<int> PianoItemTypes { get; }

  public IReadOnlyList<int> DresserItemTypes { get; }

  public IReadOnlyList<int> SofaItemTypes { get; }

  public IReadOnlyList<int> BathtubItemTypes { get; }

  public IReadOnlyList<int> LampItemTypes { get; }

  public IReadOnlyList<int> CandelabraItemTypes { get; }

  public IReadOnlyList<int> ClockItemTypes { get; }

  public IReadOnlyList<int> BannerItemTypes { get; }

  public DungeonRoomType BiomeRoomType { get; }

  public IReadOnlyList<DungeonStyleId> SubStyles { get; }

  private static IReadOnlyList<int> CopyItemTypes(IEnumerable<int>? values)
  {
    int[] copy = values?.ToArray() ?? Array.Empty<int>();
    if (copy.Any(value => value < 0))
    {
      throw new ArgumentOutOfRangeException(nameof(values));
    }

    return new ReadOnlyCollection<int>(copy);
  }

  private static IReadOnlyList<DungeonStyleId> CopyStyles(
    IEnumerable<DungeonStyleId>? values)
  {
    DungeonStyleId[] copy = values?.ToArray() ?? Array.Empty<DungeonStyleId>();
    return new ReadOnlyCollection<DungeonStyleId>(copy);
  }
}
