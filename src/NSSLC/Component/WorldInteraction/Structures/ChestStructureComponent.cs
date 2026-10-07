using Terraria.WorldStorage;
using StorageTileCoordinate = Terraria.WorldStorage.TileCoordinate;

namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存箱子的旧槽位、锚点和自定义名字。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Chest。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>主要源成员：x（第 44 行）； y（第 46 行）； name（第 52 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 944 行。</para>
/// </remarks>
public sealed class ChestStructureComponent
{
  public ChestSlot Slot { get; internal set; }

  public StorageTileCoordinate Anchor { get; internal set; }

  public string Name { get; internal set; } = string.Empty;

  public bool IsLegacyBankChest { get; internal set; }
}
