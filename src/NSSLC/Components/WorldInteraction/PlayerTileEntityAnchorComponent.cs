using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction;

// Stores the player's relation to an external TileEntity by identity and tile.
// Registry lookup, validity checks and network projection remain adapters.
/// <summary>
/// 保存玩家与外部方块实体的身份及位置关联。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：tileEntityAnchor（第 2292 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 492 行。</para>
/// <para>
/// 用途：玩家操作展示架、容器等方块实体时，记录实体引用和方块坐标，供后续交互流程定位操作对象。
/// </para>
/// <para>
/// 当前实现：HasAnchor 要求非空实体引用和坐标同时存在。注册表查找、对象仍存活的判断、关系释放和网络投影由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C172，L2（通用可选玩法）。分级理由：箱子、交互主体、方块锚点、身份、调度与结构占地是可选交互结构支持。普通固体地图不要求每格
/// 建实体。
/// </para>
/// </remarks>
public sealed class PlayerTileEntityAnchorComponent
{
  /// <summary>
  /// 玩家关联的方块实体引用，可为空。
  /// </summary>
  public EntityReference? AnchorEntity { get; set; }

  /// <summary>
  /// 玩家关联的方块实体坐标，可为空。
  /// </summary>
  public TileCoordinate? AnchorCoordinate { get; set; }

  /// <summary>
  /// 实体引用存在且非空，并且坐标存在时返回 true。
  /// </summary>
  public bool HasAnchor =>
    AnchorEntity.HasValue &&
    !AnchorEntity.Value.IsEmpty &&
    AnchorCoordinate.HasValue;
}
