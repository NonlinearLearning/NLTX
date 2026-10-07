namespace Terraria.Npc;

public readonly record struct NpcCombatResult(
  bool Applied,
  NpcDamageRejectionReason RejectionReason,
  int RequestedDamage,
  int AppliedDamage,
  int LifeBefore,
  int LifeAfter,
  bool TrackerRecorded,
  bool DeathTransitioned,
  NpcInstanceId? LifeOwnerInstanceId = null,
  int ResolvedDamage = 0)
{
  public NpcDeathLifecycleResult? DeathLifecycle { get; init; }
}
