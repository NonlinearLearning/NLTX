namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: ItemUse/tile release ownership remains unresolved
public struct PlayerReleaseAndRepeatStateComponent
{
  public bool ReleaseJump;
  public bool ReleaseUp;
  public bool ReleaseLeft;
  public bool ReleaseRight;
  public bool ReleaseDown;
  public bool ReleaseDash;
  public bool TryKeepingHoveringDown;
  public bool TryKeepingHoveringUp;
  public int LeftTimer;
  public int RightTimer;
}
