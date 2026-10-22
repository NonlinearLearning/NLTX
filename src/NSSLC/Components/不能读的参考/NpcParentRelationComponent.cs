using System;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 与父 NPC 或主 NPC 的关联身份和建立时刻。
/// </summary>
/// <remarks>
/// <para>
/// 用途：让多部位敌人的子实体能够找到主实体，供共享生命、行为协调或生命周期处理使用。
/// </para>
/// <para>
/// 当前实现：构造时保存关系并要求 AttachedAtTick 非负。父实体是否仍然存在、共享伤害如何结算，由外部关系与战斗流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C096，L2（通用可选玩法）。分级理由：父子 NPC 共享生命／行为需要关系状态，单实体敌人可以省去。
/// </para>
/// </remarks>
public sealed class NpcParentRelationComponent
{
  public NpcParentRelationComponent(
    NpcInstanceId parentInstanceId,
    NpcSlot parentLegacySlot,
    long attachedAtTick)
  {
    if (attachedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(attachedAtTick),
        "Attached tick cannot be negative.");
    }

    ParentInstanceId = parentInstanceId;
    ParentLegacySlot = parentLegacySlot;
    AttachedAtTick = attachedAtTick;
  }

  /// <summary>
  /// 父 NPC 的实例身份，用于识别具体实例。
  /// </summary>
  public NpcInstanceId ParentInstanceId { get; }

  /// <summary>
  /// 父 NPC 在原版槽位体系中的兼容位置。
  /// </summary>
  public NpcSlot ParentLegacySlot { get; }

  /// <summary>
  /// 建立父子关联时的游戏 tick。
  /// </summary>
  public long AttachedAtTick { get; }

  /// <summary>
  /// 父 NPC 是否具有已分配的原版槽位；只读取槽位状态。
  /// </summary>
  public bool HasLegacyParentSlot => ParentLegacySlot.IsAssigned;
}
