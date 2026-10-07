namespace Terraria.Projectile;

/// <summary>
/// 保存射弹透明度、发光、显示层和预览状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：alpha（第 120 行）； glowMask（第 124 行）； light（第 170 行）； drawLayer（第 210 行）； usesOwnerLight（第
/// 212 行）； hide（第 214 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectilePresentationStateComponent
{
  public ProjectilePresentationStateComponent(
    int alpha = 0,
    short glowMask = 0,
    float light = 0.0f,
    bool isPreviewDummy = false,
    bool isPreviewDisplayDoll = false,
    int drawLayer = 0,
    bool usesOwnerLight = false,
    bool hide = false,
    int trailingMode = -1)
  {
    Alpha = alpha;
    GlowMask = glowMask;
    Light = light;
    IsPreviewDummy = isPreviewDummy;
    IsPreviewDisplayDoll = isPreviewDisplayDoll;
    DrawLayer = drawLayer;
    UsesOwnerLight = usesOwnerLight;
    Hide = hide;
    TrailingMode = trailingMode;
  }

  public int Alpha;
  public short GlowMask;
  public float Light;
  public bool IsPreviewDummy;
  public bool IsPreviewDisplayDoll;
  public int DrawLayer;
  public bool UsesOwnerLight;
  public bool Hide;
  public int TrailingMode;

  public float Opacity
  {
    readonly get => 1.0f - (float)Alpha / 255.0f;
    set => Alpha = (int)System.Math.Clamp((1.0f - value) * 255.0f, 0.0f, 255.0f);
  }
}
