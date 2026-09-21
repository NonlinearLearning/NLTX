using System;

namespace Terraria.WorldSession.Calendar;

public sealed class CalendarVisualPresentationStateComponent
{
  public CalendarVisualPresentationStateComponent(
    double visualTime = 0.0d,
    short sunOffsetY = 0,
    short moonOffsetY = 0)
  {
    VisualTime = visualTime;
    SunOffsetY = sunOffsetY;
    MoonOffsetY = moonOffsetY;
    Validate();
  }

  public double VisualTime { get; internal set; }

  public short SunOffsetY { get; internal set; }

  public short MoonOffsetY { get; internal set; }

  public void Validate()
  {
    if (!double.IsFinite(VisualTime))
    {
      throw new ArgumentOutOfRangeException(nameof(VisualTime));
    }
  }
}
