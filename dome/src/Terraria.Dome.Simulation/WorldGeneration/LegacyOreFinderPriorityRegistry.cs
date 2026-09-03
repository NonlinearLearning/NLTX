using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOreFinderPriorityRegistry
{
  private static readonly IReadOnlyDictionary<ushort, short> Priorities =
    new Dictionary<ushort, short>
    {
      [6] = 220,
      [7] = 200,
      [8] = 260,
      [9] = 240,
      [12] = 550,
      [21] = 500,
      [22] = 300,
      [28] = 100,
      [37] = 400,
      [107] = 600,
      [108] = 620,
      [111] = 640,
      [129] = 675,
      [166] = 210,
      [167] = 230,
      [168] = 250,
      [169] = 270,
      [204] = 310,
      [211] = 700,
      [221] = 610,
      [222] = 630,
      [223] = 650,
      [227] = 750,
      [236] = 810,
      [404] = 150,
      [407] = 150,
      [441] = 500,
      [467] = 500,
      [468] = 500,
      [639] = 550,
      [656] = 760,
      [665] = 550,
      [701] = 760,
      [702] = 810,
      [751] = 770,
      [752] = 770
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, short> RegisterDefaults()
  {
    return Priorities;
  }

  public static bool TryGetPriority(ushort tileType, out short priority)
  {
    return Priorities.TryGetValue(tileType, out priority);
  }
}
