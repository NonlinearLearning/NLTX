using System;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 是否自然生成以及离开玩家有效范围的累计时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 NPC 的自然生成与离开玩家范围后的消失流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>重组说明：自然生成标记和离开范围累计时间是显式消失模型的状态，不是全局生成计数的搬迁。</para>
/// </remarks>
public struct NpcNaturalDespawnStateComponent
{
  public NpcNaturalDespawnStateComponent(
    bool isNaturallySpawned,
    int ticksOutsidePlayerRange = 0)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(ticksOutsidePlayerRange);
    IsNaturallySpawned = isNaturallySpawned;
    TicksOutsidePlayerRange = ticksOutsidePlayerRange;
  }

  public bool IsNaturallySpawned;

  public int TicksOutsidePlayerRange;
}
