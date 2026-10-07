namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1411, P09-1412
// crossSubsystemOwner: loadout swap, item payload, network, and persistence remain integration-review
/// <summary>
/// 保存玩家装备配置集合和当前配置选择。
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
/// </remarks>
public sealed class PlayerLoadoutStateComponent
{
  public const int LoadoutCount = 3;

  private readonly EquipmentLoadoutState[] _loadouts =
    CreateEmptyLoadouts();

  public IReadOnlyList<EquipmentLoadoutState> Loadouts => _loadouts;

  public int CurrentLoadoutIndex { get; internal set; }

  public bool HasValidLoadoutSelection =>
    (uint)CurrentLoadoutIndex < (uint)_loadouts.Length;

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
