using System;
using System.Collections.Generic;
using Terraria.Npc;

namespace Terraria.WorldSession.NpcProgression.Boss;

public sealed class BossEntityIndexRegistryComponent
{
  private readonly Dictionary<BossKind, BossEntityIndexKey> _entries = new();

  public int GolemBoss => GetLegacySlot(BossKind.Golem);

  public int PlantBoss => GetLegacySlot(BossKind.Plant);

  public int CrimsonBoss => GetLegacySlot(BossKind.Crimson);

  public int DeerclopsBoss => GetLegacySlot(BossKind.Deerclops);

  public bool TryRegister(BossKind bossKind, BossEntityIndexKey key)
  {
    ValidateBossKind(bossKind);
    ValidateKey(bossKind, key);
    return _entries.TryAdd(bossKind, key);
  }

  public bool TryReplace(
    BossKind bossKind,
    BossEntityIndexKey expectedCurrent,
    BossEntityIndexKey replacement)
  {
    ValidateBossKind(bossKind);
    ValidateKey(bossKind, expectedCurrent);
    ValidateKey(bossKind, replacement);

    if (!_entries.TryGetValue(bossKind, out BossEntityIndexKey current)
      || current != expectedCurrent
      || current == replacement)
    {
      return false;
    }

    _entries[bossKind] = replacement;
    return true;
  }

  public bool TryGet(BossKind bossKind, out BossEntityIndexKey key)
  {
    ValidateBossKind(bossKind);
    return _entries.TryGetValue(bossKind, out key);
  }

  public bool TryResolve(
    BossKind bossKind,
    BossEntityIndexKey currentNpc,
    bool isActive,
    out BossEntityIndexKey resolvedKey)
  {
    resolvedKey = default;
    ValidateBossKind(bossKind);
    if (!isActive || !currentNpc.IsValid)
    {
      return false;
    }

    if (!HasExpectedNpcType(bossKind, currentNpc.NpcType))
    {
      return false;
    }

    if (!_entries.TryGetValue(bossKind, out BossEntityIndexKey registeredKey)
      || registeredKey != currentNpc)
    {
      return false;
    }

    resolvedKey = registeredKey;
    return true;
  }

  public bool TryClear(BossKind bossKind, BossEntityIndexKey expectedCurrent)
  {
    ValidateBossKind(bossKind);
    ValidateKey(bossKind, expectedCurrent);

    if (!_entries.TryGetValue(bossKind, out BossEntityIndexKey current)
      || current != expectedCurrent)
    {
      return false;
    }

    return _entries.Remove(bossKind);
  }

  public int GetLegacySlot(BossKind bossKind)
  {
    ValidateBossKind(bossKind);
    return _entries.TryGetValue(bossKind, out BossEntityIndexKey key)
      ? key.LegacySlot.Value
      : -1;
  }

  public void Reset()
  {
    _entries.Clear();
  }

  public static NpcTypeId GetExpectedNpcType(BossKind bossKind)
  {
    return bossKind switch
    {
      BossKind.Golem => new NpcTypeId(245),
      BossKind.Plant => new NpcTypeId(262),
      BossKind.Crimson => new NpcTypeId(266),
      BossKind.Deerclops => new NpcTypeId(668),
      _ => throw new ArgumentOutOfRangeException(nameof(bossKind), bossKind, "Unknown boss kind."),
    };
  }

  private static bool HasExpectedNpcType(BossKind bossKind, NpcTypeId npcType)
  {
    return GetExpectedNpcType(bossKind) == npcType;
  }

  private static void ValidateBossKind(BossKind bossKind)
  {
    _ = GetExpectedNpcType(bossKind);
  }

  private static void ValidateKey(BossKind bossKind, BossEntityIndexKey key)
  {
    if (!key.IsValid)
    {
      throw new ArgumentException(
        "A boss registry key must contain an instance, slot, positive generation, and NPC type.",
        nameof(key));
    }

    if (!HasExpectedNpcType(bossKind, key.NpcType))
    {
      throw new ArgumentException(
        "The NPC type does not match the selected boss registry entry.",
        nameof(key));
    }
  }
}
