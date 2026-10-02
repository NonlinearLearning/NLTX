namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for protocol and scheduler integration
public struct PlayerRawControlInputComponent
{
  public bool ControlLeft;
  public bool ControlRight;
  public bool ControlUp;
  public bool ControlDown;
  public bool ControlJump;
  public bool ControlTorch;
  public bool ControlDash;
  public bool ControlDownHold;
}
