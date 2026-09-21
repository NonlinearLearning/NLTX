using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RoomNeedsTileRegistry
{
  private static readonly IReadOnlySet<int> ChairTileTypes = new HashSet<int>
  {
    15,
    79,
    89,
    102,
    487,
    497
  }.ToFrozenSet();
  private static readonly IReadOnlySet<int> TableTileTypes = new HashSet<int>
  {
    14,
    18,
    87,
    88,
    90,
    101,
    354,
    355,
    464,
    469,
    487,
    699
  }.ToFrozenSet();
  private static readonly IReadOnlySet<ushort> TableTileTypesAsUshort =
    TableTileTypes.Select(static tileType => checked((ushort)tileType)).ToFrozenSet();
  private static readonly IReadOnlySet<int> DoorTileTypes = new HashSet<int>
  {
    10,
    11,
    19,
    386,
    387,
    388,
    389,
    427,
    435,
    436,
    437,
    438,
    439
  }.ToFrozenSet();
  private static readonly IReadOnlySet<int> TorchTileTypes = new HashSet<int>
  {
    4,
    33,
    34,
    35,
    42,
    49,
    92,
    93,
    95,
    98,
    100,
    149,
    173,
    174,
    270,
    271,
    316,
    317,
    318,
    372,
    405,
    572,
    581,
    592,
    646,
    660
  }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterChairDefaults() => ChairTileTypes;

  public static IReadOnlySet<int> RegisterTableDefaults() => TableTileTypes;

  public static IReadOnlySet<ushort> RegisterTableTileDefaults() => TableTileTypesAsUshort;

  public static IReadOnlySet<int> RegisterDoorDefaults() => DoorTileTypes;

  public static IReadOnlySet<int> RegisterTorchDefaults() => TorchTileTypes;
}
