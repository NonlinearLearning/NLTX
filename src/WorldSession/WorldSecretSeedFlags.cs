using System;

namespace Terraria.WorldSession.Components;

[Flags]
public enum WorldSecretSeedFlags : ulong
{
  None = 0,
  Drunk = 1UL << 0,
  ForTheWorthy = 1UL << 1,
  TenthAnniversary = 1UL << 2,
  DontStarve = 1UL << 3,
  NotTheBees = 1UL << 4,
  Remix = 1UL << 5,
  NoTraps = 1UL << 6,
  Zenith = 1UL << 7,
  Skyblock = 1UL << 8,
  Vampire = 1UL << 9,
  Infected = 1UL << 10,
  TeamBasedSpawns = 1UL << 11,
  DualDungeons = 1UL << 12,
  GetGoodWorld = 1UL << 13,
}
