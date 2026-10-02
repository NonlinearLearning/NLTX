namespace Terraria.Npc;

public readonly record struct NpcDeathPhaseDecision(
  bool SuppressTerminalDeath,
  bool StateChanged,
  NpcDeathPhaseKind PhaseKind,
  NpcAiStateComponent AiAfter,
  bool RestoreLifeToMaximum,
  bool RejectAllDamage,
  bool RejectHostileDamage,
  bool ReplicationSyncRequested,
  NpcDeathPhaseSpawnIntent? SpawnIntent)
{
  public NpcDeathAnnouncementIntent? AnnouncementIntent { get; init; }

  public NpcLadyBugKilledIntent? LadyBugKilledIntent { get; init; }

  public NpcDeathSkeletronSpawnIntent? SkeletronSpawnIntent { get; init; }

  public NpcDeathWorldEffectIntent? WorldEffectIntent { get; init; }

  public bool RequiredInputMissing { get; init; }
}
