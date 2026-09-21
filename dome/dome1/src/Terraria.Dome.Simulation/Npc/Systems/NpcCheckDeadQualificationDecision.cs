namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadQualificationDecision(
  bool ShouldProcess,
  NpcCheckDeadQualificationRejectionReason RejectionReason);
