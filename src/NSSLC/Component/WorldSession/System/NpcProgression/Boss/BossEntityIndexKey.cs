using Terraria.Npc;

namespace Terraria.WorldSession.NpcProgression.Boss;

public readonly record struct BossEntityIndexKey(
  NpcInstanceId InstanceId,
  NpcSlot LegacySlot,
  int Generation,
  NpcTypeId NpcType)
{
  public bool IsValid =>
    InstanceId.IsValid
    && LegacySlot.IsAssigned
    && Generation > 0
    && NpcType.IsValid;
}
