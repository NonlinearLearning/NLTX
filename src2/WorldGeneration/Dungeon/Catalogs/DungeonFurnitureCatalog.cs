using System.Collections.ObjectModel;
using Terraria.WorldGeneration.Dungeon.Rooms;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Catalogs;

public sealed class DungeonFurnitureCatalog
{
  private readonly IReadOnlyDictionary<DungeonStyleId, DungeonFurnitureDefinition> _entries;

  public DungeonFurnitureCatalog(IEnumerable<DungeonFurnitureDefinition> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    var dictionary = new Dictionary<DungeonStyleId, DungeonFurnitureDefinition>();
    foreach (DungeonFurnitureDefinition definition in entries)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!dictionary.TryAdd(definition.Style, definition))
      {
        throw new ArgumentException(
          $"Dungeon furniture style '{definition.Style}' is registered more than once.",
          nameof(entries));
      }
    }

    if (dictionary.Count == 0)
    {
      throw new ArgumentException(
        "At least one dungeon furniture definition is required.",
        nameof(entries));
    }

    _entries = new ReadOnlyDictionary<DungeonStyleId, DungeonFurnitureDefinition>(dictionary);
  }

  public IReadOnlyList<DungeonFurnitureDefinition> Entries => _entries.Values.ToArray();

  public DungeonFurnitureDefinition Get(DungeonStyleId style)
  {
    if (!TryGet(style, out DungeonFurnitureDefinition? definition))
    {
      throw new KeyNotFoundException($"Dungeon furniture style '{style}' is not registered.");
    }

    return definition;
  }

  public bool TryGet(
    DungeonStyleId style,
    out DungeonFurnitureDefinition definition)
  {
    return _entries.TryGetValue(style, out definition!);
  }

  public static DungeonFurnitureCatalog CreateDefault()
  {
    return new DungeonFurnitureCatalog(
    [
      CreateShimmer(),
      CreateSpider(),
      CreateLivingWood(),
      CreateCavern(),
      CreateSnow(),
      CreateDesert(),
      CreateCorruption(),
      CreateCrimson(),
      CreateCrystal(),
      CreateHallow(),
      CreateGlowingMushroom(),
      CreateBeehive(),
      CreateLivingMahogany(),
      CreateJungle(),
      CreateTemple()
    ]);
  }

  private static DungeonFurnitureDefinition CreateShimmer()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Shimmer,
      [94],
      -1,
      -1,
      -1,
      -1,
      [5556],
      [5558],
      [5562],
      [5555],
      [5560],
      [5565],
      [5566],
      [5553],
      [],
      [5550],
      [5554],
      [5549],
      [5561],
      [5551],
      [5564],
      [5548],
      [5559],
      [5552],
      [5557],
      [337, 339, 338, 340, 5497, 5498],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateSpider()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Spider,
      [94],
      -1,
      -1,
      -1,
      -1,
      [952],
      [4415],
      [4416],
      [106, 107, 108, 710, 711, 712],
      [2037],
      [32],
      [36],
      [105, 713],
      [],
      [354],
      [34],
      [224],
      [333],
      [334],
      [2397],
      [336],
      [342],
      [349, 714],
      [359],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateLivingWood()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.LivingWood,
      [2629],
      -1,
      -1,
      -1,
      -1,
      [831],
      [819],
      [2629],
      [2141],
      [2145],
      [829],
      [2633],
      [2153],
      [],
      [2135],
      [806],
      [2139],
      [2245],
      [3914],
      [2636],
      [2126],
      [2131],
      [2149],
      [2596],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateCavern()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Cavern,
      [94, 4416],
      -1,
      -1,
      -1,
      -1,
      [306, 5886],
      [25, 4415],
      [94, 4416],
      [106, 107, 108, 710, 711, 712, 5885],
      [2037, 5890],
      [32, 5894],
      [36, 5896],
      [105, 713, 5883],
      [],
      [354, 5881],
      [34, 5884],
      [224, 5880],
      [333, 5891],
      [334, 5888],
      [2397, 5893],
      [336, 5879],
      [342, 5889],
      [349, 714, 5882],
      [359, 5887],
      [337, 339, 338, 340, 5497, 5498],
      DungeonRoomType.BiomeStructured,
      [DungeonStyleId.Shimmer, DungeonStyleId.Spider, DungeonStyleId.LivingWood]);
  }

  private static DungeonFurnitureDefinition CreateSnow()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Snow,
      [3908],
      21,
      27,
      1532,
      1572,
      [681, 5805],
      [2044, 5807],
      [3908, 5812],
      [2059, 5804],
      [2040, 5810],
      [2248, 5815],
      [2252, 5817],
      [2049, 5802],
      [],
      [2031, 5800],
      [2288, 5803],
      [2068, 5799],
      [2247, 5811],
      [3913, 5808],
      [2635, 5814],
      [2076, 5798],
      [2086, 5809],
      [2100, 5801],
      [2594, 5806],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateDesert()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Desert,
      [4311],
      467,
      13,
      4712,
      4607,
      [4267],
      [4307],
      [4311],
      [4305],
      [4309],
      [4314],
      [4315],
      [4303],
      [],
      [4300],
      [4304],
      [4299],
      [4310],
      [4301],
      [4313],
      [4298],
      [4308],
      [4302],
      [4306],
      [790, 791, 789],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateCorruption()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Corruption,
      [631],
      21,
      24,
      1529,
      1571,
      [625, 3965, 5763],
      [650, 3967, 5765],
      [631, 3957, 5770],
      [2056, 3964, 5762],
      [2033, 3970, 5768],
      [638, 3974, 5773],
      [635, 3975, 5775],
      [2046, 3962, 5760],
      [],
      [2021, 3960, 5758],
      [628, 3963, 5761],
      [644, 3959, 5757],
      [641, 3971, 5769],
      [647, 3968, 5766],
      [2398, 3973, 5772],
      [2073, 3958, 5756],
      [2083, 3969, 5767],
      [2093, 3961, 5759],
      [2593, 3966, 5764],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateCrimson()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Crimson,
      [913],
      21,
      25,
      1530,
      1569,
      [914, 2617, 5784],
      [912, 817, 5786],
      [913, 3907, 5791],
      [2142, 2057, 5783],
      [2146, 2034, 5789],
      [917, 828, 5794],
      [916, 813, 5796],
      [2154, 2047, 5781],
      [],
      [2136, 2022, 5779],
      [915, 809, 5782],
      [920, 2067, 5778],
      [919, 2246, 5790],
      [918, 2640, 5787],
      [2401, 2634, 5793],
      [2127, 2074, 5777],
      [2132, 2084, 5788],
      [2150, 2094, 5780],
      [2604, 2598, 5785],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateCrystal()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Crystal,
      [633],
      -1,
      -1,
      -1,
      -1,
      [3884],
      [3888],
      [3903],
      [3894],
      [3891],
      [3920],
      [3909],
      [3890],
      [],
      [3917],
      [3889],
      [3897],
      [3915],
      [3911],
      [3918],
      [3895],
      [3892],
      [3893],
      [3898],
      [],
      DungeonRoomType.BiomeStructured);
  }

  private static DungeonFurnitureDefinition CreateHallow()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Hallow,
      [633],
      21,
      26,
      1531,
      1260,
      [627, 3884],
      [652, 3888],
      [633, 3903],
      [2061, 3894],
      [2039, 3891],
      [640, 3920],
      [637, 3909],
      [2051, 3890],
      [],
      [2027, 3917],
      [630, 3889],
      [646, 3897],
      [643, 3915],
      [649, 3911],
      [2400, 3918],
      [2078, 3895],
      [2088, 3892],
      [2099, 3893],
      [2602, 3898],
      [],
      DungeonRoomType.BiomeRugged,
      [DungeonStyleId.Crystal]);
  }

  private static DungeonFurnitureDefinition CreateGlowingMushroom()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.GlowingMushroom,
      [2549],
      -1,
      -1,
      -1,
      -1,
      [2544],
      [818],
      [2549],
      [2543],
      [2546],
      [2550],
      [814],
      [2542],
      [],
      [2540],
      [810],
      [2538],
      [2548],
      [2545],
      [2413],
      [2537],
      [2547],
      [2541],
      [2599],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateBeehive()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Beehive,
      [2630],
      -1,
      -1,
      -1,
      -1,
      [2249],
      [1711],
      [2630],
      [2058],
      [2035],
      [1717],
      [2251],
      [2648],
      [],
      [2023],
      [1707],
      [1721],
      [2255],
      [2395],
      [2411],
      [2124],
      [2129],
      [2095],
      [2240],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateLivingMahogany()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.LivingMahogany,
      [2629],
      -1,
      -1,
      -1,
      -1,
      [831],
      [819],
      [2629],
      [2141],
      [2145],
      [829],
      [2633],
      [2153],
      [],
      [2135],
      [806],
      [2139],
      [2245],
      [3914],
      [2636],
      [2126],
      [2131],
      [2149],
      [2596],
      [],
      DungeonRoomType.BiomeRugged);
  }

  private static DungeonFurnitureDefinition CreateJungle()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Jungle,
      [632],
      21,
      23,
      1528,
      1156,
      [626, 680],
      [651],
      [632],
      [2060],
      [2038, 4578],
      [639],
      [636],
      [2050],
      [],
      [2026],
      [629],
      [645],
      [642],
      [648],
      [2399],
      [2077],
      [2087],
      [2098],
      [2597],
      [],
      DungeonRoomType.BiomeRugged,
      [DungeonStyleId.Beehive, DungeonStyleId.LivingMahogany]);
  }

  private static DungeonFurnitureDefinition CreateTemple()
  {
    return new DungeonFurnitureDefinition(
      DungeonStyleId.Temple,
      [3906],
      -1,
      -1,
      -1,
      -1,
      [1142],
      [1137],
      [3906],
      [2062],
      [2041],
      [1144],
      [1145],
      [2052],
      [1152, 1153, 1154],
      [2030],
      [1143],
      [2069],
      [2385],
      [2396],
      [2416],
      [2079],
      [2089],
      [2101],
      [2595],
      [],
      DungeonRoomType.BiomeStructured);
  }
}
