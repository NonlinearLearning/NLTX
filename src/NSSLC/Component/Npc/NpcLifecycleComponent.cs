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

    _isActive = isActive;
    RemainingActiveTicks = remainingActiveTicks;
    _stage = stage;
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

    _isActive = stage != NpcLifecycleStage.Despawned;
    RemainingActiveTicks = remainingDespawnTicks;
    _stage = stage;
  }

  private bool _isActive;

  private NpcLifecycleStage _stage;

  public bool IsActive => _isActive;

  public int RemainingActiveTicks { get; }

  public NpcLifecycleStage Stage => _stage;

  // Compatibility alias retained while callers move to RemainingActiveTicks.
  public int RemainingDespawnTicks => RemainingActiveTicks;

  internal bool TryCommitDespawn()
  {
    if (!_isActive || _stage == NpcLifecycleStage.Despawned)
    {
      return false;
    }

    _isActive = false;
    _stage = NpcLifecycleStage.Despawned;
    return true;
  }
}
