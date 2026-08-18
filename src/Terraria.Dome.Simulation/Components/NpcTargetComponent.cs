using Arch.Core;

namespace Terraria.Dome.Simulation.Components;

public struct NpcTargetComponent
{
  public Entity Target;
  public bool HasTarget;
  public int StableTargetId;
  public NpcTargetLockReason LockReason;
}

public enum NpcTargetLockReason
{
  None = 0,
  NearestActivePlayer = 1,
  RetainedValidTarget = 2,
  NoValidTarget = 3
}
