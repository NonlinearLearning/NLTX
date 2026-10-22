namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-718
// crossSubsystemOwner: buff catalog capacity and effect rebuild remain integration-review
/// <summary>
/// 保存按 Buff 定义编号索引的玩家免疫表，并提供受控读写。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：buffImmune（第 1033 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 136 行。</para>
/// <para>
/// 用途：在给玩家添加状态效果前按定义编号判断免疫，也可由装备和效果重建流程逐项登记免疫。
/// </para>
/// <para>
/// 当前实现：构造时确定表容量。IsImmune 对越界编号返回 false，SetImmunity 对越界编号抛异常；每轮和生命周期重置都会清空全部标记。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C111，L2（通用可选玩法）。分级理由：槽位、免疫、元素状态和恢复延迟只在启用状态效果时需要。
/// </para>
/// </remarks>
public sealed class PlayerBuffImmunityComponent
{
  private readonly bool[] _immuneBuffTypes;
  private readonly IReadOnlyList<bool> _immuneBuffTypesView;

  public PlayerBuffImmunityComponent(int buffTypeCount)
  {
    if (buffTypeCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(buffTypeCount));
    }

    _immuneBuffTypes = new bool[buffTypeCount];
    _immuneBuffTypesView = Array.AsReadOnly(_immuneBuffTypes);
  }

  /// <summary>
  /// 内部免疫表的只读视图，会反映后续 SetImmunity 的修改。
  /// </summary>
  public IReadOnlyList<bool> ImmuneBuffTypes => _immuneBuffTypesView;

  /// <summary>
  /// 查询指定效果是否免疫；编号越界时返回 false。
  /// </summary>
  public bool IsImmune(ContentId<BuffDefinition> effectType)
  {
    return effectType.Value >= 0 &&
      effectType.Value < _immuneBuffTypes.Length &&
      _immuneBuffTypes[effectType.Value];
  }

  /// <summary>
  /// 设置指定效果的免疫标记；编号必须位于已分配容量内。
  /// </summary>
  public void SetImmunity(ContentId<BuffDefinition> effectType, bool isImmune)
  {
    if (effectType.Value < 0 || effectType.Value >= _immuneBuffTypes.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(effectType));
    }

    _immuneBuffTypes[effectType.Value] = isImmune;
  }

  /// <summary>
  /// 清空整张免疫表，让本轮装备和状态效果重新登记。
  /// </summary>
  public void ResetForTick()
  {
    Array.Clear(_immuneBuffTypes);
  }

  /// <summary>
  /// 清空整张免疫表，用于初始化、重生或复用。
  /// </summary>
  public void ResetForLifecycle()
  {
    ResetForTick();
  }
}
