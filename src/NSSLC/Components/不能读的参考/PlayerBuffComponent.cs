namespace Terraria.Player;

/// <summary>
/// 同时保存玩家状态效果槽位和按定义编号记录的效果免疫集合。
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
/// <para>
/// 用途：状态系统读取效果与持续时间，添加效果时可读取免疫集合；两个容器分别保存现有效果和免疫资格。
/// </para>
/// <para>
/// 当前实现：固定 44 个槽位。SetNetworkBuff 从头查找同类型或空槽位并写入首个匹配项，满槽且无匹配时保持原样；持续时间递减和效果执行由外部系统处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C110，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerBuffComponent
{
  /// <summary>
  /// 效果槽位固定容量 44。
  /// </summary>
  public const int MaximumSlotCount = 44;

  /// <summary>
  /// 可修改的效果编号与持续时间槽位数组。
  /// </summary>
  public BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  // Buff immunity is distinct from damage immunity timers.
  /// <summary>
  /// 玩家免疫的 Buff 定义编号集合。
  /// </summary>
  public HashSet<ContentId<BuffDefinition>> ImmuneBuffTypes { get; } = [];

  /// <summary>
  /// 把网络效果写入首个同类型或空槽位；没有可用槽位时不写入。
  /// </summary>
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
