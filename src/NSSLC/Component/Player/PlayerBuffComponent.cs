namespace Terraria.Player;

/// <summary>
/// 保存玩家 Buff／Debuff 槽位和免疫集合。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：buffType（第 1029 行）； buffTime（第 1031 行）； buffImmune（第 1033 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 54 行。</para>
/// </remarks>
public sealed class PlayerBuffComponent
{
  public const int MaximumSlotCount = 44;

  public BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  // Buff immunity is distinct from damage immunity timers.
  public HashSet<ContentId<BuffDefinition>> ImmuneBuffTypes { get; } = [];

  internal void SetNetworkBuff(ushort buffType, int durationTicks)
  {
    ContentId<BuffDefinition> type = new(buffType);
    for (int index = 0; index < Slots.Length; index++)
    {
      if (Slots[index].Type == type || Slots[index].IsEmpty)
      {
        Slots[index] = new BuffSlot(type, durationTicks);
        return;
      }
    }
  }
}
