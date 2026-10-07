using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.TileEntities;

/// <summary>
/// 保存人体模型的装备、染色、杂项物品和姿势。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Tile_Entities.TEDisplayDoll。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.Tile_Entities/TEDisplayDoll.cs。
/// </para>
/// <para>主要源成员：_equip（第 33 行）； _dyes（第 35 行）； _misc（第 37 行）； _pose（第 39 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-world-interaction-and-structures-component-design.md。
/// </para>
/// <para>依据位置：第 808 行。</para>
/// </remarks>
public sealed class DisplayDollComponent
{
  private readonly StoredItemState[] _equipment = new StoredItemState[9];
  private readonly StoredItemState[] _dyes = new StoredItemState[9];
  private readonly StoredItemState[] _miscellaneous = new StoredItemState[1];
  private readonly ReadOnlyCollection<StoredItemState> _equipmentView;
  private readonly ReadOnlyCollection<StoredItemState> _dyesView;
  private readonly ReadOnlyCollection<StoredItemState> _miscellaneousView;

  public DisplayDollComponent()
  {
    _equipmentView = Array.AsReadOnly(_equipment);
    _dyesView = Array.AsReadOnly(_dyes);
    _miscellaneousView = Array.AsReadOnly(_miscellaneous);
  }

  public IReadOnlyList<StoredItemState> Equipment => _equipmentView;
  public IReadOnlyList<StoredItemState> Dyes => _dyesView;
  public IReadOnlyList<StoredItemState> Miscellaneous => _miscellaneousView;
  public byte Pose { get; internal set; }
  public bool ContainsItems =>
      _equipment.Any(item => !item.IsEmpty) ||
      _dyes.Any(item => !item.IsEmpty) ||
      _miscellaneous.Any(item => !item.IsEmpty);
}
