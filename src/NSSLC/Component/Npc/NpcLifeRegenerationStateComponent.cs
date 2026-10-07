namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 持续伤害与生命恢复使用的累积量。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：lifeRegenCount（第 6113 行）。</para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// </remarks>
public struct NpcLifeRegenerationStateComponent
{
  public int LifeRegenerationCount;
  public int Revision;
}
