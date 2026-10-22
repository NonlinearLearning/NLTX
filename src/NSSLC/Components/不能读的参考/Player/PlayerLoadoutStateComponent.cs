namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1411, P09-1412
// crossSubsystemOwner: loadout swap, item payload, network, and persistence remain integration-review
/// <summary>
/// 保存玩家三套装备配置及当前选择索引。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：Loadouts（第 2484 行）； CurrentLoadoutIndex（第 2491 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// <para>
/// 用途：配置切换流程读取选中的装备、染料与饰品显示组合，再提交到实际装备关联。
/// </para>
/// <para>
/// 当前实现：构造三套空配置，HasValidLoadoutSelection 只检查索引范围；ReplaceLoadout 校验索引后替换单套配置。实际切换和物品转移由外
/// 部流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C132，L2（通用可选玩法）。分级理由：装备效果、负载配置和增益属性属于基本持物以外的可选规则。
/// </para>
/// </remarks>
public sealed class PlayerLoadoutStateComponent
{
  /// <summary>
  /// 固定装备配置数量 3。
  /// </summary>
  public const int LoadoutCount = 3;

  private readonly EquipmentLoadoutState[] _loadouts =
    CreateEmptyLoadouts();

  /// <summary>
  /// 三套配置的读取视图，反映后续替换结果。
  /// </summary>
  public IReadOnlyList<EquipmentLoadoutState> Loadouts => _loadouts;

  /// <summary>
  /// 当前选择的装备配置索引，默认 0。
  /// </summary>
  public int CurrentLoadoutIndex { get; internal set; }

  /// <summary>
  /// 当前选择索引位于配置数组范围内时返回 true。
  /// </summary>
  public bool HasValidLoadoutSelection =>
    (uint)CurrentLoadoutIndex < (uint)_loadouts.Length;

  /// <summary>
  /// 校验索引后替换指定配置，不自动切换当前装备。
  /// </summary>
  internal void ReplaceLoadout(
    int index,
    EquipmentLoadoutState loadout)
  {
    if ((uint)index >= (uint)_loadouts.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    _loadouts[index] = loadout;
  }

  private static EquipmentLoadoutState[] CreateEmptyLoadouts()
  {
    EquipmentLoadoutState[] loadouts = new EquipmentLoadoutState[LoadoutCount];
    for (int index = 0; index < loadouts.Length; index++)
    {
      loadouts[index] = new EquipmentLoadoutState(
        new ItemEntityRef[PlayerEquipmentRelationComponent.ArmorSlotCount],
        new ItemEntityRef[PlayerEquipmentRelationComponent.DyeSlotCount],
        new bool[PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount]);
    }

    return loadouts;
  }
}
