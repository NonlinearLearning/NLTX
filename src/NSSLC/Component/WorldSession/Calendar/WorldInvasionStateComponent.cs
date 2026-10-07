using System;
using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界入侵的种类、位置、规模、波次和进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>
/// 主要源成员：invasionType（第 1059 行）； invasionX（第 1061 行）； invasionSize（第 1063 行）； invasionDelay（第
/// 1065 行）； invasionWarn（第 1067 行）； invasionSizeStart（第 1069 行）； invasionProgressIcon（第 1071 行）；
/// invasionProgress（第 1073 行）； invasionProgressMax（第 1075 行）； invasionProgressWave（第 1077 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-15-version4-非权威20分区组件实现审计报告.md。</para>
/// <para>依据位置：第 35 行。</para>
/// </remarks>
public sealed class WorldInvasionStateComponent
{
  public WorldInvasionStateComponent(
    InvasionType type = InvasionType.None,
    double positionX = 0.0d,
    int size = 0,
    int sizeStart = 0,
    int delay = 0,
    int warningTimer = 0,
    int progress = 0,
    int progressMax = 0,
    int progressIcon = 0,
    int progressWave = 0)
  {
    Type = type;
    PositionX = positionX;
    Size = size;
    SizeStart = sizeStart;
    Delay = delay;
    WarningTimer = warningTimer;
    Progress = progress;
    ProgressMax = progressMax;
    ProgressIcon = progressIcon;
    ProgressWave = progressWave;
    Validate();
  }

  public InvasionType Type;
  public double PositionX;
  public int Size;
  public int SizeStart;
  public int Delay;
  public int WarningTimer;
  public int Progress;
  public int ProgressMax;
  public int ProgressIcon;
  public int ProgressWave;

  public bool IsActive => Type != InvasionType.None && Size > 0;

  public void Validate()
  {
    if (!double.IsFinite(PositionX))
    {
      throw new ArgumentOutOfRangeException(nameof(PositionX));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(Size);
    ArgumentOutOfRangeException.ThrowIfNegative(SizeStart);
    ArgumentOutOfRangeException.ThrowIfNegative(Delay);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningTimer);
    ArgumentOutOfRangeException.ThrowIfNegative(Progress);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressMax);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressIcon);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressWave);

    if (IsActive && SizeStart < Size)
    {
      throw new ArgumentException(
        "An active invasion cannot exceed its starting size.",
        nameof(SizeStart));
    }

    if (!IsActive && Type == InvasionType.None && Size != 0)
    {
      throw new ArgumentException(
        "An inactive invasion cannot retain a non-zero size.",
        nameof(Size));
    }
  }
}
