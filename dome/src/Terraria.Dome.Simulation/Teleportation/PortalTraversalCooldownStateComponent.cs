using Arch.Core;

namespace Terraria.Dome.Simulation.Teleportation;

public struct PortalTraversalCooldownStateComponent
{
  public int RemainingTicks;
  public Entity? LastPortal;
  public PortalSubjectKind SubjectKind;
  public int? CooldownGroup;

  public bool IsCoolingDown => RemainingTicks > 0;
}
