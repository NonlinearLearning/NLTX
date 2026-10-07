namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界各难度撒旦军队事件的通关记录。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.DD2Event。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs。</para>
/// <para>
/// 主要源成员：DownedInvasionT1（第 45 行）； DownedInvasionT2（第 47 行）； DownedInvasionT3（第 49 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 109 行。</para>
/// </remarks>
public sealed class Dd2PersistentProgressStateComponent
{
  public bool DownedInvasionT1;
  public bool DownedInvasionT2;
  public bool DownedInvasionT3;
}
