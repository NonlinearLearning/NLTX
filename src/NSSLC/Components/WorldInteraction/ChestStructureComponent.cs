using Terraria.WorldStorage;
using StorageTileCoordinate = Terraria.WorldStorage.TileCoordinate;

namespace Terraria.WorldInteraction.Structures;

/// <summary>
/// 保存箱子的槽位身份、方块锚点、名称和兼容容器标记。
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
/// <para>
/// 用途：箱子交互流程根据槽位和锚点定位结构，界面与持久化流程读取名称和容器类别。
/// </para>
/// <para>
/// 当前实现：只保存结构信息，名称默认空字符串。箱内物品、打开权限和世界注册由外部物品与交互流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C173，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class ChestStructureComponent
{
  /// <summary>
  /// 箱子在存储或兼容注册表中的槽位。
  /// </summary>
  public ChestSlot Slot { get; internal set; }

  /// <summary>
  /// 箱子结构在世界中的方块锚点。
  /// </summary>
  public StorageTileCoordinate Anchor { get; internal set; }

  /// <summary>
  /// 箱子的自定义名称，默认空字符串。
  /// </summary>
  public string Name { get; internal set; } = string.Empty;

  /// <summary>
  /// 是否属于原版个人存储兼容箱子。
  /// </summary>
  public bool IsLegacyBankChest { get; internal set; }
}
