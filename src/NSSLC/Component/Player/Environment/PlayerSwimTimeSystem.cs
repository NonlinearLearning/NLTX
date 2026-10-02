namespace Terraria.Player.Environment;

// status: implemented-partial
// crossSubsystemOwner: P07 jump/input and PlayerFrame scheduling need integration review
public sealed class PlayerSwimTimeSystem
{
  private const int SwimTimeRefreshThreshold = 10;
  private const int SwimTimeDuration = 30;

  public void RefreshForMermanJump(
    PlayerEnvironmentMobilityStateComponent mobility,
    in PlayerMermanJumpInput input)
  {
    ArgumentNullException.ThrowIfNull(mobility);

    if (!input.HasActiveJumpCounter || !input.IsAirborne || !input.IsMerman ||
      (input.MountActive && input.CartMount))
    {
      return;
    }

    if (mobility.SwimTime <= SwimTimeRefreshThreshold)
    {
      mobility.SwimTime = SwimTimeDuration;
    }
  }

  public void RefreshForFlipperJump(
    PlayerEnvironmentMobilityStateComponent mobility,
    in PlayerFlipperJumpInput input)
  {
    ArgumentNullException.ThrowIfNull(mobility);

    if (input.CanStartNewJump && input.IsWet && input.HasFlippers && mobility.SwimTime == 0)
    {
      mobility.SwimTime = SwimTimeDuration;
    }
  }

  public void TickFrame(PlayerEnvironmentMobilityStateComponent mobility, bool isWet)
  {
    ArgumentNullException.ThrowIfNull(mobility);

    if (mobility.SwimTime > 0)
    {
      mobility.SwimTime--;
      if (!isWet)
      {
        mobility.SwimTime = 0;
      }
    }
  }
}
