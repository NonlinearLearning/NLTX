namespace Terraria.Npc;

public readonly record struct NpcDamageOverTimeResult(
  bool Accepted,
  NpcDamageRejectionReason RejectionReason,
  int RequestedDamage,
  int DirectDamageApplied,
  int LifeBefore,
  int LifeAfterDirectStage,
  bool TrackerRecorded,
  NpcDamageOverTimeTextIntent? CombatTextIntent,
  bool CombatTextPublished,
  bool RequiresForcedDeathStrike,
  NpcCombatResult? ForcedDeathStrikeResult,
  NpcDeathPacket28Intent? DeathPacket28Intent,
  int LifeAfterForcedDeathStrike,
  NpcInstanceId? LifeOwnerInstanceId = null)
{
  public bool CombatTextRequested => CombatTextIntent.HasValue;

  public bool DeathPacket28Requested => DeathPacket28Intent.HasValue;
}
