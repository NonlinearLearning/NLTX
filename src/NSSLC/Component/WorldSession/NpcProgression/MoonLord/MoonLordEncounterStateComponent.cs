using System;

namespace Terraria.WorldSession.NpcProgression.MoonLord;

/// <summary>
/// 保存月亮领主召唤倒计时和请求状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：MoonLordCountdown（第 5921 行）； MaxMoonLordCountdown（第 5923 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 126 行。</para>
/// </remarks>
public sealed class MoonLordEncounterStateComponent
{
  private int _maxMoonLordCountdown;
  private bool _spawnRequestIssued;

  public MoonLordEncounterStateComponent(int maxMoonLordCountdown)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maxMoonLordCountdown);
    _maxMoonLordCountdown = maxMoonLordCountdown;
  }

  public int MoonLordCountdown { get; private set; }

  public int MaxMoonLordCountdown => _maxMoonLordCountdown;

  public void StartCountdown(int countdown)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(countdown);
    _maxMoonLordCountdown = countdown;
    MoonLordCountdown = countdown;
    _spawnRequestIssued = false;
  }

  public void SetCountdown(int countdown)
  {
    if (countdown < 0 || countdown > _maxMoonLordCountdown)
    {
      throw new ArgumentOutOfRangeException(
        nameof(countdown),
        "Moon Lord countdown must remain within the encounter definition range.");
    }

    MoonLordCountdown = countdown;
    if (countdown > 0)
    {
      _spawnRequestIssued = false;
    }
  }

  public int TickCountdown()
  {
    if (MoonLordCountdown <= 0)
    {
      return 0;
    }

    MoonLordCountdown--;
    return MoonLordCountdown;
  }

  public bool TryIssueSpawnRequest()
  {
    if (MoonLordCountdown != 0 || _spawnRequestIssued)
    {
      return false;
    }

    _spawnRequestIssued = true;
    return true;
  }

  public void Reset()
  {
    MoonLordCountdown = 0;
    _spawnRequestIssued = false;
  }
}
