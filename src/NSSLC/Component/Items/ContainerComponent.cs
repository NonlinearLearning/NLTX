using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>
/// 保存容器容量和内容。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Chest。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>主要源成员：item（第 42 行）。</para>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：bank（第 1089 行）； bank2（第 1091 行）； bank3（第 1093 行）； bank4（第 1095 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public sealed class ContainerComponent
{
  public ContainerComponent(int capacity, IReadOnlyList<EntityReference> contents)
  {
    Capacity = capacity;
    Contents = new List<EntityReference>(contents);
  }

  public int Capacity;
  public List<EntityReference> Contents;
}
