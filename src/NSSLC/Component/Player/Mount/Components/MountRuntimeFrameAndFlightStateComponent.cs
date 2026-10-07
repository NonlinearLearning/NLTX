namespace Terraria.Player.Mount;

/// <summary>
/// 保存坐骑类型、活动、动画、飞行和闲置计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Mount。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>
/// 主要源成员：_flipDraw（第 325 行）； _frameCounter（第 329 行）； _frameExtra（第 331 行）； _frameExtraCounter（第
/// 333 行）； _frameState（第 335 行）； _idleTime（第 339 行）； _idleTimeNext（第 341 行）； _shouldSuperCart（第
/// 359 行）； _walkingGraceTimeLeft（第 361 行）； Active（第 387 行）； Type（第 389 行）； Frame（第 391 行）；
/// FlyTime（第 393 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P03-mount-vehicle-component-design.md。</para>
/// <para>依据位置：第 84 行。</para>
/// </remarks>
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

  internal void Reset()
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
}
