namespace Terraria.WorldSession.Components;

/// <summary>
/// Explicit write boundary for the seven saved ore tiers.
/// </summary>
public interface ISavedOreTierCommitPort
{
  void CommitGeneration(WorldSavedOreTierGenerationCommit commit);

  void CommitAltar(WorldSavedOreTierAltarCommit commit);
}
