using System;
using System.Collections.Generic;

namespace Terraria.Npc;

public sealed class NpcDamageDefinitionCatalog
{
  private readonly Dictionary<NpcTypeId, NpcTypeId[]> _compositeGroups = new();
  private readonly Dictionary<NpcTypeId, NpcTypeId> _bossTypeByMobType = new();

  public static NpcDamageDefinitionCatalog CreateVersion4Snapshot()
  {
    var catalog = new NpcDamageDefinitionCatalog();
    catalog.RegisterMobsForBoss(new NpcTypeId(50), new NpcTypeId(1), new NpcTypeId(535));
    catalog.RegisterMobsForBoss(new NpcTypeId(4), new NpcTypeId(5));
    catalog.RegisterMobsForBoss(new NpcTypeId(222), new NpcTypeId(210), new NpcTypeId(211));
    catalog.RegisterCompositeBoss(new NpcTypeId(13), new NpcTypeId(14), new NpcTypeId(15));
    catalog.RegisterCompositeBoss(new NpcTypeId(266), new NpcTypeId(267));
    catalog.RegisterCompositeBoss(new NpcTypeId(35), new NpcTypeId(36));
    catalog.RegisterMobsForBoss(
      new NpcTypeId(113),
      new NpcTypeId(115),
      new NpcTypeId(116),
      new NpcTypeId(117),
      new NpcTypeId(118),
      new NpcTypeId(119));
    catalog.RegisterMobsForBoss(
      new NpcTypeId(657),
      new NpcTypeId(658),
      new NpcTypeId(659),
      new NpcTypeId(660));
    catalog.RegisterCompositeBoss(
      new NpcTypeId(126),
      new NpcTypeId(125));
    catalog.RegisterCompositeBoss(
      new NpcTypeId(127),
      new NpcTypeId(128),
      new NpcTypeId(129),
      new NpcTypeId(130),
      new NpcTypeId(131));
    catalog.RegisterMobsForBoss(new NpcTypeId(134), new NpcTypeId(139));
    catalog.RegisterMobsForBoss(new NpcTypeId(262), new NpcTypeId(264));
    catalog.RegisterCompositeBoss(
      new NpcTypeId(245),
      new NpcTypeId(246),
      new NpcTypeId(247),
      new NpcTypeId(248));
    catalog.RegisterMobsForBoss(
      new NpcTypeId(370),
      new NpcTypeId(372),
      new NpcTypeId(373));
    catalog.RegisterMobsForBoss(
      new NpcTypeId(439),
      new NpcTypeId(454),
      new NpcTypeId(455),
      new NpcTypeId(456),
      new NpcTypeId(457),
      new NpcTypeId(458),
      new NpcTypeId(459));
    catalog.RegisterCompositeBoss(
      new NpcTypeId(398),
      new NpcTypeId(396),
      new NpcTypeId(397));
    return catalog;
  }

  public bool TryGetBossTypeForMob(
    NpcTypeId mobType,
    out NpcTypeId bossType)
  {
    return _bossTypeByMobType.TryGetValue(mobType, out bossType);
  }

  public INpcDamageTrackingStrategy? CreateStrategy(
    NpcTypeId npcType,
    bool isBoss)
  {
    if (_compositeGroups.TryGetValue(npcType, out NpcTypeId[]? compositeTypes))
    {
      return new NpcDamageCompositeStrategy(compositeTypes);
    }

    if (!isBoss)
    {
      return null;
    }

    var includedTypes = new List<NpcTypeId>();
    foreach (KeyValuePair<NpcTypeId, NpcTypeId> mapping in _bossTypeByMobType)
    {
      if (mapping.Value == npcType)
      {
        includedTypes.Add(mapping.Key);
      }
    }

    return new NpcDamageSingleTypeStrategy(npcType, includedTypes);
  }

  private void RegisterCompositeBoss(params NpcTypeId[] npcTypes)
  {
    ValidateDistinctTypes(npcTypes);
    foreach (NpcTypeId npcType in npcTypes)
    {
      if (!_compositeGroups.TryAdd(npcType, npcTypes.ToArray()))
      {
        throw new InvalidOperationException(
          $"NPC type {npcType.Value} is already in a composite boss group.");
      }
    }
  }

  private void RegisterMobsForBoss(
    NpcTypeId bossType,
    params NpcTypeId[] mobTypes)
  {
    if (!bossType.IsValid)
    {
      throw new ArgumentException("Boss type must be valid.", nameof(bossType));
    }

    ValidateDistinctTypes(mobTypes);
    foreach (NpcTypeId mobType in mobTypes)
    {
      if (mobType == bossType || !_bossTypeByMobType.TryAdd(mobType, bossType))
      {
        throw new InvalidOperationException(
          $"NPC type {mobType.Value} already has a damage tracker boss mapping.");
      }
    }
  }

  private static void ValidateDistinctTypes(IReadOnlyList<NpcTypeId> npcTypes)
  {
    ArgumentNullException.ThrowIfNull(npcTypes);
    if (npcTypes.Count == 0)
    {
      throw new ArgumentException("At least one NPC type is required.", nameof(npcTypes));
    }

    var distinctTypes = new HashSet<NpcTypeId>();
    for (int index = 0; index < npcTypes.Count; index++)
    {
      if (!npcTypes[index].IsValid || !distinctTypes.Add(npcTypes[index]))
      {
        throw new ArgumentException(
          "NPC types must be valid and unique.",
          nameof(npcTypes));
      }
    }
  }
}
