using Terraria.Relationships;

namespace Terraria.Teleportation;

public struct PortalTraversalCooldownStateComponent
{
  public bool IsCoolingDown => RemainingTicks > 0;
  public int RemainingTicks;
  public EntityReference? LastPortal;
  public PortalSubjectKind SubjectKind;
  public int? CooldownGroup;
}
