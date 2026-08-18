using Arch.Core;

namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcTargetComponent
{
  public NpcTargetComponent(Entity target, int stableTargetId, NpcTargetLockReason lockReason)
  {
    Target = target;
    StableTargetId = stableTargetId;
    LockReason = lockReason;
    HasTarget = lockReason != NpcTargetLockReason.NoValidTarget;
  }

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
