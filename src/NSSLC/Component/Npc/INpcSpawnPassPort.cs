using System.Collections.Generic;

namespace Terraria.Npc;

public interface INpcSpawnPassPort :
  INpcSpawnTileSearchPort,
  INpcSpawnChosenTileFlagsPort,
  INpcSpawnPerPlayerFlagsPort
{
  NpcSpawnPlayerEligibilitySnapshot CapturePlayerEligibility(int playerIndex);

  IReadOnlyList<NpcSpawnScreenPlayerSnapshot> CaptureScreenPlayers();

  NpcSpawnPostCheckInputs CapturePostCheckInputs(
    int playerIndex,
    in NpcSpawnTileSearchResult tileSearchResult);

  bool IsSlimeRainActive { get; }

  void SpawnSlimeRainForPlayer(int playerIndex);

  void ContinueSpawnAttempt(in NpcSpawnAcceptedCandidate candidate);
}
