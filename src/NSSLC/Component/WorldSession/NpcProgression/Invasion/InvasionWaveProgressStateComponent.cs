using System;

namespace Terraria.WorldSession.NpcProgression.Invasion;

/// <summary>
/// 保存世界入侵点数、波次击杀和波次编号。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：totalInvasionPoints（第 5937 行）； waveKills（第 5939 行）； waveNumber（第 5941 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 356 行。</para>
/// </remarks>
public sealed class InvasionWaveProgressStateComponent
{
  public float TotalInvasionPoints { get; private set; }

  public float WaveKills { get; private set; }

  public int WaveNumber { get; private set; }

  public void Commit(float totalInvasionPoints, float waveKills, int waveNumber)
  {
    ValidateFiniteNonNegative(totalInvasionPoints, nameof(totalInvasionPoints));
    ValidateFiniteNonNegative(waveKills, nameof(waveKills));
    ArgumentOutOfRangeException.ThrowIfNegative(waveNumber);

    if (waveNumber < WaveNumber)
    {
      throw new ArgumentOutOfRangeException(
        nameof(waveNumber),
        "Invasion wave number cannot move backwards without an explicit reset.");
    }

    TotalInvasionPoints = totalInvasionPoints;
    WaveKills = waveKills;
    WaveNumber = waveNumber;
  }

  public void Reset()
  {
    TotalInvasionPoints = 0.0f;
    WaveKills = 0.0f;
    WaveNumber = 0;
  }

  private static void ValidateFiniteNonNegative(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        "Invasion progress must be finite and non-negative.");
    }
  }
}
