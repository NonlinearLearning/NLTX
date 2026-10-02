using System;
using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Calendar;

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
