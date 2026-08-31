using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyLargeFrameTileRegistry
{
  private static readonly IReadOnlyDictionary<ushort, byte> FrameSizes =
    new Dictionary<ushort, byte>
    {
      [273] = 1, [274] = 1, [284] = 1, [325] = 1, [357] = 1, [409] = 2,
      [618] = 1, [669] = 2, [670] = 2, [671] = 2, [672] = 2, [673] = 2,
      [674] = 2, [675] = 2, [676] = 2, [735] = 2, [736] = 1, [737] = 2,
      [741] = 2, [742] = 2, [743] = 2, [745] = 2, [746] = 2, [749] = 2
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, byte> RegisterDefaults()
  {
    return FrameSizes;
  }

  public static byte GetFrameSize(ushort tileType)
  {
    return FrameSizes.TryGetValue(tileType, out byte size) ? size : (byte)0;
  }
}
