using Terraria.Items;
using Terraria.Npc;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeTargetSnapshotComponent
{
  public RevengeTargetSnapshotComponent(
    WorldPosition location,
    NpcNetId npcNetId,
    NpcTypeId npcTypeId,
    int legacyAiStyle,
    float lifeFraction,
    int coinValue,
    float baseValue,
    bool spawnedFromStatue)
  {
    Location = location;
    NpcNetId = npcNetId;
    NpcTypeId = npcTypeId;
    LegacyAiStyle = legacyAiStyle;
    LifeFraction = lifeFraction;
    CoinValue = coinValue;
    BaseValue = baseValue;
    SpawnedFromStatue = spawnedFromStatue;
  }

  public WorldPosition Location { get; }

  public NpcNetId NpcNetId { get; }

  public NpcTypeId NpcTypeId { get; }

  public int LegacyAiStyle { get; }

  public float LifeFraction { get; }

  public int CoinValue { get; }

  public float BaseValue { get; }

  public bool SpawnedFromStatue { get; }
}
