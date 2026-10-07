namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-718
// crossSubsystemOwner: buff catalog capacity and effect rebuild remain integration-review
/// <summary>
/// 保存玩家免疫的 Buff 定义集合。
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

  public IReadOnlyList<bool> ImmuneBuffTypes => _immuneBuffTypesView;

  public bool IsImmune(ContentId<BuffDefinition> effectType)
  {
    return effectType.Value >= 0 &&
      effectType.Value < _immuneBuffTypes.Length &&
      _immuneBuffTypes[effectType.Value];
  }

  public void SetImmunity(ContentId<BuffDefinition> effectType, bool isImmune)
  {
    if (effectType.Value < 0 || effectType.Value >= _immuneBuffTypes.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(effectType));
    }

    _immuneBuffTypes[effectType.Value] = isImmune;
  }

  public void ResetForTick()
  {
    Array.Clear(_immuneBuffTypes);
  }

  public void ResetForLifecycle()
  {
    ResetForTick();
  }
}
