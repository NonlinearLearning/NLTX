namespace Terraria.Player;

/// <summary>
/// Consumes the lifecycle dead-tick result that belongs to the ghost state owner.
/// Respawn, inventory UI, spawn position and network effects remain external.
/// </summary>
public static class PlayerDeadTickAdapter
{
  public static PlayerLifecycleSystem.DeadTickResult Apply(
    ref PlayerLifecycleComponent lifecycle,
    PlayerGhostStateComponent ghostState,
    PlayerLifecycleSystem.DeadTickInput input)
  {
    ArgumentNullException.ThrowIfNull(ghostState);

    PlayerLifecycleSystem.DeadTickInput authoritativeInput = input with
    {
      IsGhost = ghostState.Ghost,
    };
    PlayerLifecycleSystem.DeadTickResult result =
      PlayerLifecycleSystem.AdvanceDeadTick(ref lifecycle, authoritativeInput);
    if (result.ShouldBecomeGhost)
    {
      ghostState.Ghost = true;
    }

    return result;
  }
}
