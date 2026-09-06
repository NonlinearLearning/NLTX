using System;

namespace Terraria.Npc;

// status: proposed
// evidenceStatus: missing for Version4 stable instance identity
// crossSubsystemOwner: integration-review
public sealed class NpcIdentityComponent
{
  public NpcIdentityComponent(NpcInstanceId instanceId)
  {
    if (!instanceId.IsValid)
    {
      throw new ArgumentException(
        "NpcInstanceId must be valid for an initialized NPC entity.",
        nameof(instanceId));
    }

    InstanceId = instanceId;
  }

  public NpcInstanceId InstanceId { get; }
}
