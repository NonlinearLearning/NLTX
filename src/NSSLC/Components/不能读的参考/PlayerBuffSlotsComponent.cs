namespace Terraria.Player;

/// <summary>
/// 保存玩家固定容量的状态效果编号与持续时间槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：buffType（第 1029 行）； buffTime（第 1031 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 134 行。</para>
/// <para>
/// 用途：接收网络效果列表或向读取方提供槽位快照，再由外部状态流程应用效果并消耗持续时间。
/// </para>
/// <para>
/// 当前实现：容量固定为 44。ReplaceNetworkSlots 先清空数组再依次写入输入类型，统一使用传入持续时间；输入数量须由调用方控制在容量内。Snapshot
///  复制数组后返回只读视图。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C112，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerBuffSlotsComponent
{
  /// <summary>
  /// 状态效果槽位固定容量 44。
  /// </summary>
  public const int MaximumSlotCount = 44;

  /// <summary>
  /// 内部可写槽位数组，保存效果类型和剩余持续时间。
  /// </summary>
  internal BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  /// <summary>
  /// 清空原槽位后按输入顺序写入类型，全部使用指定持续时间。
  /// </summary>
  internal void ReplaceNetworkSlots(IReadOnlyList<ushort> types, int durationTicks)
  {
    Array.Clear(Slots);
    for (int index = 0; index < types.Count; index++)
    {
      Slots[index] = new BuffSlot(
        new ContentId<BuffDefinition>(types[index]),
        durationTicks);
    }
  }

  /// <summary>
  /// 复制当前槽位数组并返回只读快照。
  /// </summary>
  public IReadOnlyList<BuffSlot> Snapshot()
  {
    return Array.AsReadOnly(Slots.ToArray());
  }
}
