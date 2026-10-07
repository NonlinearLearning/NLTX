namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: SpawnedFromStatue, CanBeReplacedByOtherNPCs
// crossSubsystemOwner: spawn-commit-and-replacement-integration-review
/// <summary>
/// 保存 NPC 雕像生成和允许被其他 NPC 替换的状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：SpawnedFromStatue（第 5949 行）； CanBeReplacedByOtherNPCs（第 5951 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p14-npc-spawn-eligibility-component-design.md。</para>
/// <para>依据位置：第 105 行。</para>
/// </remarks>
public sealed class NpcSpawnAndCritterStateComponent
{
  public NpcSpawnAndCritterStateComponent(
    bool spawnedFromStatue = false,
    bool canBeReplacedByOtherNpcs = false)
  {
    SpawnedFromStatue = spawnedFromStatue;
    CanBeReplacedByOtherNpcs = canBeReplacedByOtherNpcs;
  }

  public bool SpawnedFromStatue { get; }

  public bool CanBeReplacedByOtherNpcs { get; }
}
