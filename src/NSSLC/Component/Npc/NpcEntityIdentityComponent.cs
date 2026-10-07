namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 实例身份与旧数组槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：whoAmI（第 8 行）。</para>
/// <para>重组说明：InstanceId 是 ECS 实例身份，LegacySlot 承接旧数组索引。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 71 行。</para>
/// </remarks>
public sealed class NpcEntityIdentityComponent
{
  public NpcEntityIdentityComponent(NpcInstanceId instanceId, NpcSlot legacySlot)
  {
    InstanceId = instanceId;
    LegacySlot = legacySlot;
  }

  public NpcInstanceId InstanceId { get; }

  public NpcSlot LegacySlot { get; }

  public bool HasAssignedSlot => LegacySlot.IsAssigned;
}
