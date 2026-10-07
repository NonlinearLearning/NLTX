namespace Terraria.Npc;

public sealed record NpcAiProfileCoverageRegistration(
  string Name,
  NpcAiProfileIdentity Identity,
  NpcAiCoverageStage Stage,
  bool HasFiniteHandler,
  bool HasOpenWork);
