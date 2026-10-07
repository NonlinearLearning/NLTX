using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>
/// 保存主体的主手与副手物品引用。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：inventory（第 1083 行）； selectedItem（第 2960 行）。</para>
/// <para>重组说明：主手、副手关系是 NLTX 新增的表达；原玩家通过选中槽位取得手持物品。</para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：Version4玩家玩法与能力系统组件拆分报告.md。</para>
/// <para>依据位置：第 71 行。</para>
/// </remarks>
public struct HandsComponent
{
  public HandsComponent(EntityReference primary, EntityReference secondary)
  {
    Primary = primary;
    Secondary = secondary;
  }

  public EntityReference Primary;
  public EntityReference Secondary;

  public bool HasPrimary => !Primary.IsEmpty;
  public bool HasSecondary => !Secondary.IsEmpty;
}
