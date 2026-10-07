namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 whoAmI/slot usage
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存 NPC 的旧数组槽位和槽位代数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：whoAmI（第 8 行）。</para>
/// <para>重组说明：Generation 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 143 行。</para>
/// </remarks>
public sealed class NpcLegacySlotComponent
{
  public NpcLegacySlotComponent(NpcSlot? legacySlot = null, uint generation = 0)
  {
    LegacySlot = legacySlot ?? new NpcSlot(-1);
    Generation = generation;
  }

  public NpcSlot LegacySlot { get; private set; }

  /// <summary>The generation of the legacy slot projection, or zero when not assigned.</summary>
  public uint Generation { get; private set; }

  public bool IsAssigned => LegacySlot.IsAssigned;

  public void SetLegacySlot(NpcSlot legacySlot, uint generation = 0)
  {
    LegacySlot = legacySlot;
    Generation = generation;
  }
}
