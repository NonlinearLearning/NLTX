using System.Collections.Generic;
using Arch.Core;

namespace Terraria.Projectile;

/// <summary>
/// 按目标实体和射弹类型保存命中免疫的绝对到期 tick。
/// </summary>
/// <remarks>
/// <para>
/// 用途：同类射弹再次命中同一目标时，外部战斗流程查询 (Entity, ProjectileType) 对应的到期值，判断本次是否仍处于免疫期。
/// </para>
/// <para>
/// 数据含义：字典键使用完整 Arch Entity 句柄和射弹类型枚举；uint 是到期 tick 的存储类型。不同目标或不同射弹类型对应不同记录。
/// </para>
/// <para>
/// 当前实现：只提供可变哈希表。到期比较、重复命中时的合并规则和旧记录清理由外部流程负责；表的生命周期应与所属世界一致。
/// </para>
/// </remarks>
public sealed class InvincibilityFramesComponent {
  // 键：目标实体与射弹类型枚举；值：绝对到期 tick。
  //这里需要靠考虑下uint的期 tick的合并,以及Projectile是否需要提供uint的到期 tick值
  /// <summary>
  /// 键为目标实体与射弹类型，值为免疫绝对到期 tick；readonly 固定字典引用，字典内容仍可修改。
  /// </summary>
  public readonly Dictionary<(Entity TargetEntity, ProjectileType ProjectileType),uint> ExpiryTicks = new();
}
enum ProjectileType;