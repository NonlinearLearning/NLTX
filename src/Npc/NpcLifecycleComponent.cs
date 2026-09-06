using System;

namespace Terraria.Npc;

public sealed class NpcLifecycleComponent
{
  public NpcLifecycleComponent(
    bool isActive,
    int remainingActiveTicks,
    NpcLifecycleStage stage)
  {
    if (remainingActiveTicks < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingActiveTicks),
        "Remaining active ticks cannot be negative.");
    }

    if (stage == NpcLifecycleStage.Despawned && isActive)
    {
      throw new ArgumentException(
        "A despawned NPC cannot be active.",
        nameof(isActive));
    }

    IsActive = isActive;
    RemainingActiveTicks = remainingActiveTicks;
    Stage = stage;
  }

  // Compatibility constructor for the existing stage/despawn-ticks API.
  public NpcLifecycleComponent(NpcLifecycleStage stage, int remainingDespawnTicks)
  {
    if (remainingDespawnTicks < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingDespawnTicks),
        "Remaining despawn ticks cannot be negative.");
    }

    IsActive = stage != NpcLifecycleStage.Despawned;
    RemainingActiveTicks = remainingDespawnTicks;
    Stage = stage;
  }

  public bool IsActive { get; }

  public int RemainingActiveTicks { get; }

  public NpcLifecycleStage Stage { get; }

  // Compatibility alias retained while callers move to RemainingActiveTicks.
  public int RemainingDespawnTicks => RemainingActiveTicks;
}
