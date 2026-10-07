namespace Terraria.Projectile;

/// <summary>
/// 保存射弹每轮额外更新次数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：extraUpdates（第 196 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 17 行。</para>
/// </remarks>
public record struct ProjectileUpdateCadenceComponent(
  int ExtraUpdates = 0)
{
  public int MaxUpdates
  {
    readonly get => ExtraUpdates + 1;
    set => ExtraUpdates = value - 1;
  }
}
