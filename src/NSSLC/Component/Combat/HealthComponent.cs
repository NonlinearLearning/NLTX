using System;

namespace Terraria.Combat;

/// <summary>
/// 保存当前生命值和生效的生命上限。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：statLifeMax2（第 1359 行）； statLife（第 1361 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：life（第 6341 行）； lifeMax（第 6343 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 50 行。</para>
/// </remarks>
public struct HealthComponent
{
  public HealthComponent(int current, int maximum)
  {
    Current = current;
    Maximum = maximum;
  }

  public int Current;
  public int Maximum;

  public bool IsDepleted => Current <= 0;

  public int Missing => Math.Max(0, Maximum - Current);
}
