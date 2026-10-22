namespace MinimalImplementation.Server.Player;

/// <summary>Version4 player-buff facts used by the packet-50 admission boundary.</summary>
public static class Version4PlayerBuffCatalog
{
  public const int BuffCount = 389;

  public const ushort StinkyBuffType = 120;
  public const ushort LuckPotionBuffType = 257;

  public static bool IsValidBuff(ushort buffType)
  {
    return buffType > 0 && buffType < BuffCount;
  }

  public static bool IsStinky(ushort buffType)
  {
    return buffType == StinkyBuffType;
  }

  public static bool TimeLeftDoesNotDecrease(ushort type)
  {
    return type is 28 or 334 or 29 or 159 or 150 or 93 or 348 or 366;
  }

  public static bool PersistsAfterDeath(ushort type) {
    return IsMeleeBuff(type);
  }

  public static bool IsFedState(ushort type)
  {
    return type is 26 or 206 or 207 or 332 or 333 or 334;
  }

  public static int AdditiveTimeCap(ushort type)
  {
    return type switch { 94 => 600, 383 => 43_200, _ => 0 };
  }

  public static bool IsExtendedByPotionStation(ushort type)
  {
    return type is >= 1 and <= 18 or >= 104 and <= 117 or >= 121 and <= 124
      or 257 or 343;
  }

  public static bool IsExtendedByDifficulty(ushort type)
  {
    return type is 20 or 22 or 23 or 24 or 323 or 30 or 31 or 32 or 33 or 35 or 36
      or 39 or 44 or 324 or 46 or 47 or 69 or 70 or 80;
  }

  public static bool IsDebuff(ushort type)
  {
    return type is 158 or 160 or >= 20 and <= 25 or 28 or >= 30 and <= 39 or 44
      or 46 or 47 or >= 67 and <= 70 or 80 or >= 86 and <= 89 or 94 or 103
      or 119 or 120 or 137 or >= 144 and <= 149 or 156 or 157 or 350 or 163 or 164
      or >= 194 and <= 197 or 199 or 215 or 320 or 321 or >= 332 and <= 334
      or 353 or 323 or 324 or 43 or 153 or 203 or 204 or 169 or 189 or 183
      or 186 or 344 or 72;
  }

  public static bool IsMeleeBuff(ushort type)
  {
    return type is 71 or >= 73 and <= 79;
  }

  public static bool IsLightPet(ushort type)
  {
    return type is 19 or 155 or 27 or 101 or 102 or 57 or 190 or 152 or 201
      or 294 or 298 or 299;
  }

  public static bool IsVanityPet(ushort type)
  {
    return type is >= 40 and <= 42 or 45 or >= 50 and <= 56 or 61 or 154
      or 65 or 66 or 81 or 82 or 84 or 85 or 91 or 92 or 127 or 136 or 191 or 202
      or 200 or 218 or 219 or 217 or >= 258 and <= 262 or 264 or 266 or 267 or 268
      or 274 or >= 284 and <= 293 or 295 or 296 or 297 or >= 300 and <= 304
      or 317 or 341 or >= 327 and <= 331 or 345 or 349 or 351 or 352 or 354
      or 373 or 382 or 356 or 371 or 372;
  }
}
