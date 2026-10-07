namespace Terraria.Player;

public static class PlayerTileInteractionReleaseSystem
{
  public const int GamepadTileInteractionLockTicks = 3;

  public static void ApplyGamepadTileReleaseLock(PlayerInteractionLockStateComponent state)
  {
    state.ReleaseUseTile = false;
    state.LockTileInteractionsTimer = GamepadTileInteractionLockTicks;
  }

  public static void Advance(
    in PlayerTileInteractionReleaseInput input,
    PlayerInteractionLockStateComponent state)
  {
    bool releaseUseTile = !input.TileInteractAttempted;
    // The lock gates release only until release was already observed.
    if (state.LockTileInteractionsTimer > 0 && !state.ReleaseUseTile)
    {
      releaseUseTile = false;
    }

    if (input.MouseInterface)
    {
      releaseUseTile = false;
    }

    state.ReleaseUseTile = releaseUseTile;
    if (state.LockTileInteractionsTimer > 0)
    {
      state.LockTileInteractionsTimer--;
    }
  }
}
