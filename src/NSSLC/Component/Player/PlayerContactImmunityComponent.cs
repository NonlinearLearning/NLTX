using System;

namespace Terraria.Player;

/// <summary>
/// 保存玩家接触伤害的剩余免疫时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：immune（第 968 行）； immuneTime（第 972 行）。</para>
/// <para>重组说明：RemainingTicks 是接触伤害通道独立化后的窗口表达。</para>
/// </remarks>
public readonly record struct PlayerContactImmunityComponent
{
  public PlayerContactImmunityComponent(int remainingTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(remainingTicks);
    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks { get; }
}
