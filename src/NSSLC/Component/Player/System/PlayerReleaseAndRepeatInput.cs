namespace Terraria.Player;

public readonly record struct PlayerReleaseAndRepeatInput(
  bool ControlJump,
  bool ControlUp,
  bool ControlLeft,
  bool ControlRight,
  bool ControlDown,
  bool ControlDash,
  bool TryKeepingHoveringDown,
  bool TryKeepingHoveringUp)
{
  public static PlayerReleaseAndRepeatInput FromRawControls(
    in PlayerRawControlInputComponent rawControls,
    bool tryKeepingHoveringDown,
    bool tryKeepingHoveringUp)
  {
    return new PlayerReleaseAndRepeatInput(
      ControlJump: rawControls.ControlJump,
      ControlUp: rawControls.ControlUp,
      ControlLeft: rawControls.ControlLeft,
      ControlRight: rawControls.ControlRight,
      ControlDown: rawControls.ControlDown,
      ControlDash: rawControls.ControlDash,
      TryKeepingHoveringDown: tryKeepingHoveringDown,
      TryKeepingHoveringUp: tryKeepingHoveringUp);
  }
}
