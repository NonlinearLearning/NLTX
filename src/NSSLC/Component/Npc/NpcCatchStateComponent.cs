namespace Terraria.Npc;

// status: partial
// sourceMembers: catchItem, releaseOwner
// crossSubsystemOwner: capture and release integration-review
/// <summary>
/// 保存 NPC 捕获物品类型和释放者。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：catchItem（第 5965 行）； releaseOwner（第 5967 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P12-npc-combat-network-damage-component-design.md。</para>
/// <para>依据位置：第 13 行。</para>
/// </remarks>
public sealed class NpcCatchStateComponent
{
  public NpcCatchStateComponent(short catchItem = 0, short releaseOwner = -1)
  {
    CatchItem = catchItem;
    ReleaseOwner = releaseOwner;
  }

  public short CatchItem { get; }

  public short ReleaseOwner { get; }
}
