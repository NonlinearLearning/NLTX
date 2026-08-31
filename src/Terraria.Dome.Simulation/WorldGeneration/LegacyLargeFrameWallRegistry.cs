using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyLargeFrameWallRegistry
{
  public const ushort WallTypeCount = 367;

  private static readonly IReadOnlyDictionary<ushort, byte> _frameSizes =
    new Dictionary<ushort, byte>
    {
      [146] = 1,
      [147] = 1,
      [167] = 1,
      [179] = 1,
      [185] = 2,
      [224] = 2,
      [274] = 2,
      [323] = 2,
      [324] = 2,
      [325] = 2,
      [326] = 2,
      [327] = 2,
      [328] = 2,
      [329] = 2,
      [330] = 2,
      [354] = 1,
      [355] = 2,
      [358] = 2,
      [359] = 2,
      [362] = 2,
      [363] = 2,
      [366] = 2
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, byte> RegisterDefaults()
  {
    return _frameSizes;
  }

  public static byte GetFrameSize(ushort wallType)
  {
    return _frameSizes.TryGetValue(wallType, out byte size) ? size : (byte)0;
  }
}
