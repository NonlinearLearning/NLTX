using Terraria.Relationships;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 目标实体引用。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：target（第 6321 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 375 行。</para>
/// </remarks>
public struct TargetingComponent
{
  public TargetingComponent(EntityReference target)
  {
    Target = target;
  }

  public EntityReference Target;
}
