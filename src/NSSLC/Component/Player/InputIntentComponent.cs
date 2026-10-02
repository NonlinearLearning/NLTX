namespace Terraria.Player;

public struct InputIntentComponent
{
  public InputIntentComponent(
    bool moveLeft,
    bool moveRight,
    bool moveUp,
    bool moveDown,
    bool jump,
    bool useItem)
  {
    MoveLeft = moveLeft;
    MoveRight = moveRight;
    MoveUp = moveUp;
    MoveDown = moveDown;
    Jump = jump;
    UseItem = useItem;
    UseTile = false;
    ControlTorch = false;
    ControlDash = false;
    ControlDownHold = false;
    ReleaseJump = false;
    ReleaseUp = false;
    ReleaseLeft = false;
    ReleaseRight = false;
    ReleaseDown = false;
    ReleaseDash = false;
    AlternateUseMode = ItemUseMode.None;
    RequestedDirection = DirectionKind.None;
    IssuedAtTick = null;
    Sequence = 0;
    Source = InputIntentSource.None;
  }

  public bool MoveLeft;
  public bool MoveRight;
  public bool MoveUp;
  public bool MoveDown;
  public bool Jump;
  public bool UseItem;
  public bool UseTile;
  public bool ControlTorch;
  public bool ControlDash;
  public bool ControlDownHold;
  public bool ReleaseJump;
  public bool ReleaseUp;
  public bool ReleaseLeft;
  public bool ReleaseRight;
  public bool ReleaseDown;
  public bool ReleaseDash;
  public ItemUseMode AlternateUseMode;
  public DirectionKind RequestedDirection;
  public long? IssuedAtTick;
  public uint Sequence;
  public InputIntentSource Source;

  public bool WantsDropThroughPlatforms => MoveDown;
  public bool HasDirectionalInput => MoveLeft || MoveRight || MoveUp || MoveDown;
}
