using System;

namespace Terraria.WorldSession.Calendar;

public sealed class CloudFieldStateComponent
{
  public const int MaximumCloudCount = 200;

  public CloudFieldStateComponent(
    int weatherAdjustmentTimer = 0,
    int activeCount = MaximumCloudCount,
    int targetCountWorkset = MaximumCloudCount)
  {
    WeatherAdjustmentTimer = weatherAdjustmentTimer;
    ActiveCount = activeCount;
    TargetCountWorkset = targetCountWorkset;
    Validate();
  }

  public int WeatherAdjustmentTimer { get; internal set; }

  public int ActiveCount { get; internal set; }

  public int TargetCountWorkset { get; internal set; }

  public void Validate()
  {
    if (ActiveCount is < 0 or > MaximumCloudCount)
    {
      throw new ArgumentOutOfRangeException(nameof(ActiveCount));
    }
  }
}
