using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>
/// 保存攻击来源对各目标的再次命中冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：meleeNPCHitCooldown（第 2481 行）。</para>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：localNPCImmunity（第 158 行）。</para>
/// <para>重组说明：按攻击来源和目标实体重组命中间隔；目标引用键是 ECS 关系表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 242 行。</para>
/// </remarks>
public sealed class HitCooldownComponent
{
  private readonly Dictionary<EntityReference, int> _remainingTicksByTarget = new();

  public int GetRemaining(EntityReference target)
  {
    return _remainingTicksByTarget.GetValueOrDefault(target);
  }

  public void Arm(EntityReference target, int remainingTicks)
  {
    if (remainingTicks <= 0)
    {
      return;
    }

    _remainingTicksByTarget[target] = remainingTicks;
  }

  public void Clear(EntityReference target)
  {
    _remainingTicksByTarget.Remove(target);
  }

  public void Tick()
  {
    foreach (EntityReference target in _remainingTicksByTarget.Keys.ToArray())
    {
      int remainingTicks = _remainingTicksByTarget[target] - 1;
      if (remainingTicks <= 0)
      {
        _remainingTicksByTarget.Remove(target);
      }
      else
      {
        _remainingTicksByTarget[target] = remainingTicks;
      }
    }
  }
}
