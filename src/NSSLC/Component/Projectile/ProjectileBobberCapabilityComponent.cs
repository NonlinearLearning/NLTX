namespace Terraria.Projectile;

/// <summary>
/// 保存射弹是否作为浮标及其浮标类型。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：bobber（第 104 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileBobberCapabilityComponent
{
  public ProjectileBobberCapabilityComponent(
    int bobberType = 0,
    bool isBobber = false)
  {
    BobberType = bobberType;
    IsBobber = isBobber;
  }

  public int BobberType;
  public bool IsBobber;
}
