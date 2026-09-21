using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileGlowMaskRegistry
{
  private const short DefaultMask = -1;

  private static readonly IReadOnlyDictionary<ushort, short> Masks =
    new Dictionary<ushort, short>
    {
      [129] = -2,
      [209] = 215,
      [350] = 94,
      [370] = 111,
      [381] = 126,
      [390] = 130,
      [410] = 201,
      [429] = 214,
      [445] = 214,
      [509] = 265,
      [517] = 258,
      [534] = 259,
      [535] = 260,
      [536] = 261,
      [537] = 262,
      [539] = 263,
      [540] = 264,
      [625] = 311,
      [626] = 312,
      [627] = 313,
      [628] = 314,
      [633] = 326,
      [658] = 333,
      [659] = 348,
      [667] = 349,
      [687] = 336,
      [688] = 337,
      [689] = 338,
      [690] = 339,
      [691] = 340,
      [692] = 341,
      [699] = 353,
      [708] = 359,
      [717] = 362,
      [720] = 368,
      [721] = 369,
      [725] = 371
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, short> RegisterDefaults()
  {
    return Masks;
  }

  public static short GetMask(int tileType)
  {
    return tileType >= 0 && tileType <= ushort.MaxValue &&
      Masks.TryGetValue((ushort)tileType, out short mask)
      ? mask
      : DefaultMask;
  }
}
