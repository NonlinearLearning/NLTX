using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>
/// 按伤害来源实体累计伤害贡献。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 NPCDamageTracker.AddDamage、GetOrAddEntry 的伤害归属流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/NPCDamageTracker.cs。</para>
/// <para>重组说明：按 EntityReference 累计贡献是 NLTX 的重组模型，不是原类字段的逐项搬迁。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-code-draft.md。</para>
/// <para>依据位置：第 767 行。</para>
/// </remarks>
public sealed class DamageContributionComponent
{
  private readonly Dictionary<EntityReference, int> _damageBySource = new();

  public int GetDamage(EntityReference source)
  {
    return _damageBySource.GetValueOrDefault(source);
  }

  public void AddDamage(EntityReference source, int amount)
  {
    if (source.IsEmpty || amount <= 0)
    {
      return;
    }

    _damageBySource[source] = GetDamage(source) + amount;
  }
}
