using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>
/// Drains the combat results queued against one entity's runtime handle.
/// </summary>
/// <remarks>
/// 玩家侧与 NPC 侧各有一个"排空本域伤害结果"的系统，两者对排空的定义必须一致：
/// 先按实体句柄解析出运行时身份，再一次性取走该身份名下的全部结果。
/// 解析不到句柄时返回空列表而不是抛异常——实体可能在两次 tick 之间已经销毁。
/// 这段规则此前在 <c>PlayerCombatResultSystem</c> 与 <c>NpcCombatResultSystem</c>
/// 里各写了一遍，现在只在这里定义一次。
/// </remarks>
internal static class CombatResultDrain
{
  private static readonly IReadOnlyList<CombatDamageResult> Empty =
      Array.Empty<CombatDamageResult>();

  internal static IReadOnlyList<CombatDamageResult> Drain(
      CombatDamageResultQueueComponent queue,
      IEntityRuntimeHandleResolver handles,
      ArchEntity entity)
  {
    return handles.TryGetHandle(entity, out EntityRuntimeHandle source)
        ? queue.ConsumeResults(source)
        : Empty;
  }
}
