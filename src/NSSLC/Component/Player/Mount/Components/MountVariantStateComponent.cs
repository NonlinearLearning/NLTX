namespace Terraria.Player.Mount;

/// <summary>
/// 保存不同坐骑专用的变体状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Mount.BooleanMountData。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>主要源成员：boolean（第 62 行）。</para>
/// <para>拆分来源：Terraria.Mount.SelectiveFlyingMountData。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>主要源成员：showFlyingFrames（第 72 行）； allowedToFly（第 74 行）。</para>
/// <para>拆分来源：Terraria.Mount.ExtraFrameMountData。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>主要源成员：frame（第 85 行）； frameCounter（第 87 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P03-mount-vehicle-component-design.md。</para>
/// <para>依据位置：第 96 行。</para>
/// </remarks>
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

  internal void Reset()
  {
    Kind = MountVariantKind.None;
    BooleanValue = false;
    ShowFlyingFrames = false;
    AllowedToFly = false;
    ExtraFrame = 0;
    ExtraFrameCounter = 0f;
  }
}
