using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcInvasionBossCapPolicy
{
  public const int SourceDefaultMaxSpawns = 5;

  private const int MaximumActivePlayers = 255;
  private const float BaseSpawnMultiplier = 2.0f;
  private const float PerPlayerSpawnMultiplier = 0.3f;

  public static NpcInvasionBossCapDecision Evaluate(
    IReadOnlyList<NpcInvasionBossSlotAccount> accounts,
    int activePlayerCount,
    int defaultMaxSpawns = SourceDefaultMaxSpawns)
  {
    ArgumentNullException.ThrowIfNull(accounts);
    if (activePlayerCount < 0 || activePlayerCount > MaximumActivePlayers)
    {
      throw new ArgumentOutOfRangeException(nameof(activePlayerCount));
    }

    if (defaultMaxSpawns <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(defaultMaxSpawns));
    }

    float perPlayerLimitValue = defaultMaxSpawns *
      (BaseSpawnMultiplier + PerPlayerSpawnMultiplier * activePlayerCount);
    if (!float.IsFinite(perPlayerLimitValue) || perPlayerLimitValue <= 0.0f ||
        perPlayerLimitValue > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(defaultMaxSpawns));
    }

    int perPlayerLimit = (int)perPlayerLimitValue;
    long globalLimit = checked((long)activePlayerCount * perPlayerLimit);
    double activeBossSlotCost = 0.0;
    for (int index = 0; index < accounts.Count; index++)
    {
      NpcInvasionBossSlotAccount account = accounts[index];
      if (account.NpcType < 0 || account.NpcType >= LegacyNpcInvasionBossRegistry.NpcTypeCount)
      {
        throw new ArgumentOutOfRangeException(nameof(accounts));
      }

      if (!float.IsFinite(account.NpcSlotCost) || account.NpcSlotCost < 0.0f)
      {
        throw new ArgumentOutOfRangeException(nameof(accounts));
      }

      if (account.IsActive && LegacyNpcInvasionBossRegistry.IsInvasionBoss(account.NpcType))
      {
        activeBossSlotCost += account.NpcSlotCost;
      }
    }

    if (!double.IsFinite(activeBossSlotCost) || activeBossSlotCost > float.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(accounts));
    }

    float activeBossSlots = (float)activeBossSlotCost;
    return new NpcInvasionBossCapDecision(
      activeBossSlots >= globalLimit,
      activeBossSlots,
      perPlayerLimit,
      globalLimit,
      activePlayerCount);
  }
}
