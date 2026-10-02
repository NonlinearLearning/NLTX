namespace Terraria.Npc;

public readonly record struct NpcSpawnPlayerEligibilityResult(
  NpcSpawnPlayerEligibilityReason Reason)
{
  public bool IsEligible => Reason == NpcSpawnPlayerEligibilityReason.Eligible;
}
