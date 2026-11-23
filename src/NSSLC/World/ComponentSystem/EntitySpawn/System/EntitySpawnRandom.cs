using System;

namespace Terraria.EntitySpawn;

/// <summary>
/// Deterministic pseudo-random cursor for spawn rolls.
/// </summary>
/// <remarks>
/// The Components layer holds no shared <see cref="Random"/>, so each system derives its own
/// stream from facts it already has: the world seed, the tick and a per-purpose salt. Two servers
/// replaying the same ticks therefore produce the same spawns, which is what the world file and
/// the tick log need in order to be comparable. The state is a local cursor, never shared between
/// systems.
/// </remarks>
public struct EntitySpawnRandom
{
  private const ulong DefaultState = 0x9E3779B97F4A7C15UL;

  private ulong _state;

  public EntitySpawnRandom(ulong seed)
  {
    _state = seed == 0 ? DefaultState : seed;
  }

  /// <summary>Derives one stream for a tick, mixing the world seed so replays stay stable.</summary>
  public static EntitySpawnRandom ForTick(long worldSeed, long tick, ulong salt = 0)
  {
    ulong state = unchecked((ulong)worldSeed * 0x9E3779B97F4A7C15UL);
    state = unchecked(state ^ ((ulong)tick * 0xBF58476D1CE4E5B9UL));
    state = unchecked(state ^ (salt * 0x94D049BB133111EBUL));
    state = unchecked(state ^ (state >> 31));
    return new EntitySpawnRandom(state);
  }

  /// <summary>Version4 <c>Main.rand.Next(maxValue)</c>.</summary>
  public int Next(int maxExclusive)
  {
    if (maxExclusive <= 1)
    {
      return 0;
    }

    return (int)(NextUInt64() % (ulong)maxExclusive);
  }

  /// <summary>Version4 <c>Main.rand.Next(minValue, maxValue)</c>.</summary>
  public int Next(int minInclusive, int maxExclusive)
  {
    return maxExclusive <= minInclusive ? minInclusive : minInclusive + Next(maxExclusive - minInclusive);
  }

  private ulong NextUInt64()
  {
    // xorshift64*：状态非零即可产生 2^64-1 周期，不需要锁或额外状态。
    ulong value = _state;
    value ^= value >> 12;
    value ^= value << 25;
    value ^= value >> 27;
    _state = value;
    return unchecked(value * 0x2545F4914F6CDD1DUL);
  }
}
