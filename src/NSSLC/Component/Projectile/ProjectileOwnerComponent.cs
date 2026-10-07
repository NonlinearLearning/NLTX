using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹所属玩家槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：owner（第 126 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-projectile-simulation-code-component-draft.md。</para>
/// <para>依据位置：第 166 行。</para>
/// </remarks>
public struct ProjectileOwnerComponent
{
  public ProjectileOwnerComponent(EntityReference owner)
  {
    Owner = owner;
  }

  public EntityReference Owner;
}
