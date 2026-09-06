namespace Terraria.Npc;

public sealed class NpcEntityIdentityComponent
{
  public NpcEntityIdentityComponent(NpcInstanceId instanceId, NpcSlot legacySlot)
  {
    InstanceId = instanceId;
    LegacySlot = legacySlot;
  }

  public NpcInstanceId InstanceId { get; }

  public NpcSlot LegacySlot { get; }

  public bool HasAssignedSlot => LegacySlot.IsAssigned;
}
