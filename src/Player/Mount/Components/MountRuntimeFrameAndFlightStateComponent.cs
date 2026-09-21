namespace Terraria.Player.Mount;

public sealed class MountRuntimeFrameAndFlightStateComponent
{
  public MountRuntimeFrameAndFlightStateComponent()
  {
    MountType = null;
    IsActive = false;
    FlipDraw = false;
    Frame = 0;
    FrameCounter = 0f;
    ExtraFrame = 0;
    ExtraFrameCounter = 0f;
    FrameState = MountFrameStateKind.Standing;
    FlightTimeRemainingTicks = 0;
    IdleTimeTicks = 0;
    NextIdleTimeTicks = -1;
    UsesSuperCartRules = false;
    WalkingGraceRemainingTicks = 0;
  }

  public ContentId<MountDefinition>? MountType { get; internal set; }

  public bool IsActive { get; internal set; }

  public bool FlipDraw { get; internal set; }

  public int Frame { get; internal set; }

  public float FrameCounter { get; internal set; }

  public int ExtraFrame { get; internal set; }

  public float ExtraFrameCounter { get; internal set; }

  public MountFrameStateKind FrameState { get; internal set; }

  public int FlightTimeRemainingTicks { get; internal set; }

  public int IdleTimeTicks { get; internal set; }

  public int NextIdleTimeTicks { get; internal set; }

  public bool UsesSuperCartRules { get; internal set; }

  public int WalkingGraceRemainingTicks { get; internal set; }

  public enum MountFrameStateKind
  {
    Standing = 0,
    Running = 1,
    InAir = 2,
    Flying = 3,
    Swimming = 4,
    Dashing = 5,
  }
}
