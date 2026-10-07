namespace Terraria.Items;

/// <summary>
/// 保存世界掉落物的生成来源和同步编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Item 的世界物品生成与同步流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>重组说明：SpawnSource 与 ReplicationId 是世界掉落物的来源关系和同步身份表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 520 行。</para>
/// </remarks>
public sealed class WorldItemStateComponent
{
  public WorldItemStateComponent(
    ReplicationId replicationId,
    LootSourceRef? spawnSource = null)
  {
    ReplicationId = replicationId;
    SpawnSource = spawnSource;
  }

  public LootSourceRef? SpawnSource;
  public ReplicationId ReplicationId;
}
