namespace Terraria.Npc;

/// <summary>
/// Captures a hit until the next NPC AI tick consumes it.
/// </summary>
/// <remarks>
/// <para>职责：保存 NPC 本轮是否刚刚受击。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：justHit（第 6317 行）。</para>
/// </remarks>
public sealed class NpcHitStateComponent
{
  public bool JustHit { get; private set; }

  public void CommitHit()
  {
    JustHit = true;
  }

  public bool Consume()
  {
    bool justHit = JustHit;
    JustHit = false;
    return justHit;
  }
}
