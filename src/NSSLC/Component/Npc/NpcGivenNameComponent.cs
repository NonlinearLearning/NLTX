namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 被分配的名字。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：GivenName（第 6606 行）。</para>
/// <para>拆分依据目录：docs/system-decomposition/reports/。</para>
/// <para>
/// 拆分依据文件：2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md。
/// </para>
/// <para>依据位置：第 383 行。</para>
/// </remarks>
public sealed class NpcGivenNameComponent
{
  public NpcGivenNameComponent(string? givenName)
  {
    GivenName = givenName ?? string.Empty;
  }

  public string GivenName { get; private set; }

  public void SetGivenName(string? givenName)
  {
    GivenName = givenName ?? string.Empty;
  }
}
