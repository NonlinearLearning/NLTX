using Terraria.Npc;

namespace Terraria.Npc.Queries;

public static class NpcSpawnTargetSelectionQuery
{
  /// <summary>
  /// Resolves the legacy unspecified-target sentinel against one spawn-attempt snapshot.
  /// </summary>
  public static int Select(
    int requestedTarget,
    in NpcSpawnTargetSelectionSnapshot snapshot)
  {
    return requestedTarget == 255 ? snapshot.DefaultTarget : requestedTarget;
  }
}
