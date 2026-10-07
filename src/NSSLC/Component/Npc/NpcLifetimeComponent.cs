namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 存活期限、种群占用和待消失原因。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：npcSlots（第 6051 行）； timeLeft（第 6319 行）； despawnEncouraged（第 6443 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spawn-lifecycle-and-loot-component-design.md。</para>
/// <para>依据位置：第 541 行。</para>
/// </remarks>
public sealed class NpcLifetimeComponent
{
  public NpcLifetimeComponent(
    int remainingTicks,
    float populationSlotCost,
    bool countsAgainstPopulation)
  {
    RemainingTicks = remainingTicks;
    PopulationSlotCost = populationSlotCost;
    CountsAgainstPopulation = countsAgainstPopulation;
  }

  public int RemainingTicks { get; set; }

  public float PopulationSlotCost { get; }

  public bool CountsAgainstPopulation { get; }

  public bool DespawnEncouraged { get; set; }

  public NpcDespawnReason PendingDespawnReason { get; set; }

  public bool IsExpired => RemainingTicks <= 0;

  public bool IsPendingDespawn => PendingDespawnReason != NpcDespawnReason.None;

  public float EffectivePopulationCost => CountsAgainstPopulation ? PopulationSlotCost : 0f;
}
