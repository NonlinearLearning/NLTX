namespace Terraria.Npc;

public interface INpcSpawnPerPlayerFlagsPort : INpcSpawnRateRandomPort
{
  /// <summary>
  /// Captures values read before the legacy invasion check, including current player zones.
  /// </summary>
  NpcSpawnPerPlayerFlagsPrelude CapturePerPlayerFlagsPrelude(int playerIndex);

  /// <summary>
  /// Captures the invasion and player-position values consumed before the NPC-slot scan.
  /// </summary>
  NpcSpawnInvasionInputs CaptureInvasionInputs(int playerIndex);

  bool IsTownNpcSlot(int npcIndex);

  float GetNpcCenterX(int npcIndex);

  /// <summary>
  /// Captures the remaining player/tile values read after the invasion check.
  /// </summary>
  NpcSpawnPerPlayerFlagsPostlude CapturePerPlayerFlagsPostlude(int playerIndex);

  NpcSpawnRateInputs CaptureSpawnRateInputs(int playerIndex);
}
