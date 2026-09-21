namespace Terraria.Player.Mount;

public sealed class MountVariantStateComponent
{
  public MountVariantStateComponent()
  {
    Kind = MountVariantKind.None;
    BooleanValue = false;
    ShowFlyingFrames = false;
    AllowedToFly = false;
    ExtraFrame = 0;
    ExtraFrameCounter = 0f;
  }

  public MountVariantKind Kind { get; internal set; }

  public bool BooleanValue { get; internal set; }

  public bool ShowFlyingFrames { get; internal set; }

  public bool AllowedToFly { get; internal set; }

  public int ExtraFrame { get; internal set; }

  public float ExtraFrameCounter { get; internal set; }

  public enum MountVariantKind
  {
    None = 0,
    Boolean = 1,
    SelectiveFlying = 2,
    ExtraFrame = 3,
  }
}
