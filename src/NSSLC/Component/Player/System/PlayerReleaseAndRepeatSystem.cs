using System;

namespace Terraria.Player;

public static class PlayerReleaseAndRepeatSystem
{
  public const int DirectionRepeatWindowTicks = 7;

  public static PlayerReleaseAndRepeatStateComponent CreateResetState()
  {
    return new PlayerReleaseAndRepeatStateComponent
    {
      ReleaseJump = true,
      ReleaseUp = true,
      ReleaseLeft = true,
      ReleaseRight = true,
      ReleaseDown = true,
      ReleaseDash = true,
      TryKeepingHoveringDown = false,
      TryKeepingHoveringUp = false,
      LeftTimer = 0,
      RightTimer = 0,
    };
  }

  public static void Advance(
    in PlayerReleaseAndRepeatInput input,
    bool reset,
    ref PlayerReleaseAndRepeatStateComponent state)
  {
    if (reset)
    {
      state = CreateResetState();
      return;
    }

    state.ReleaseJump = !input.ControlJump;
    state.ReleaseUp = !input.ControlUp;
    state.ReleaseLeft = !input.ControlLeft;
    state.ReleaseRight = !input.ControlRight;
    state.ReleaseDown = !input.ControlDown;
    state.ReleaseDash = !input.ControlDash;
    state.TryKeepingHoveringDown = input.TryKeepingHoveringDown;
    state.TryKeepingHoveringUp = input.TryKeepingHoveringUp;
    state.LeftTimer = AdvanceDirectionTimer(state.LeftTimer, input.ControlLeft);
    state.RightTimer = AdvanceDirectionTimer(state.RightTimer, input.ControlRight);
  }

  private static int AdvanceDirectionTimer(int previousTimer, bool controlHeld)
  {
    int normalizedTimer = Math.Clamp(
      previousTimer,
      0,
      DirectionRepeatWindowTicks);
    if (!controlHeld)
    {
      return DirectionRepeatWindowTicks - 1;
    }

    return normalizedTimer > 0
      ? normalizedTimer - 1
      : DirectionRepeatWindowTicks;
  }
}
