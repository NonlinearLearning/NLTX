namespace Terraria.Player;

/// <summary>
/// 保存玩家 Buff／Debuff 编号和持续时间槽位。
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
/// </remarks>
public sealed class PlayerBuffSlotsComponent
{
  public const int MaximumSlotCount = 44;

  internal BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

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

  public IReadOnlyList<BuffSlot> Snapshot()
  {
    return Array.AsReadOnly(Slots.ToArray());
  }
}
