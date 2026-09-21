namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for ItemCheck and tile interaction facts
public struct PlayerReleaseAndRepeatStateComponent
{
  public bool ReleaseJump;
  public bool ReleaseUp;
  public bool ReleaseUseItem;
  public bool ReleaseUseTile;
  public bool ReleaseLeft;
  public bool ReleaseRight;
  public bool ReleaseDown;
  public bool ReleaseDash;
  public bool TryKeepingHoveringDown;
  public bool TryKeepingHoveringUp;
  public int LeftTimer;
  public int RightTimer;
}
