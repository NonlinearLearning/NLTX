using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.Resolution;

public readonly record struct NpcKillAttemptSnapshot(
  EntityReference TargetReference,
  int NetworkNpcId,
  bool WasActive)
{
  public bool HasNetworkNpcId => NetworkNpcId >= 0;
}
