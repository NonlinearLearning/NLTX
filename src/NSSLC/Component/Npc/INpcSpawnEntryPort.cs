namespace Terraria.Npc;

public interface INpcSpawnEntryPort
{
  /// <summary>
  /// Reads and clears the pending no-spawn-cycle flag, returning its previous value.
  /// </summary>
  bool ConsumeNoSpawnCycle();

  void CheckRespawns();
}
