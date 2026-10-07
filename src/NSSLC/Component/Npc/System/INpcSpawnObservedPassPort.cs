namespace Terraria.Npc;

/// <summary>Optional continuation contract for ports that can observe NPC allocation.</summary>
public interface INpcSpawnObservedPassPort : INpcSpawnPassPort
{
  NpcSpawnCreationObservation ContinueSpawnAttemptObserved(
    in NpcSpawnAcceptedCandidate candidate);
}
