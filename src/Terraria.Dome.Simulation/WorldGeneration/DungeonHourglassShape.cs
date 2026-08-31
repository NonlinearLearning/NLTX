using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonHourglassShape
{
  private const float LowerBound = 0f;
  private const float UpperBound = 1f;

  private readonly int _width;
  private readonly int _height;
  private readonly float _percentileAddon;

  public DungeonHourglassShape(int width, int height, float percentileAddon)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (float.IsNaN(percentileAddon) || float.IsInfinity(percentileAddon))
    {
      throw new ArgumentOutOfRangeException(nameof(percentileAddon));
    }

    _width = width;
    _height = height;
    _percentileAddon = percentileAddon;
  }

  public int Width => _width;

  public int Height => _height;

  public float PercentileAddon => _percentileAddon;

  public int GetHalfWidth(int verticalOffset)
  {
    int halfHeight = _height / 2;
    if (verticalOffset < -halfHeight || verticalOffset > halfHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(verticalOffset));
    }

    float percent = ((float)verticalOffset + halfHeight) / _height;
    float wrappedPercent = WrappedLerp(LowerBound, UpperBound, percent);
    float percentile = MultiLerp(
      wrappedPercent,
      1f,
      1f,
      0.75f,
      0.65f,
      0.45f,
      0.4f,
      0.35f,
      0.35f);
    float clampedPercentile = Math.Clamp(percentile + _percentileAddon, LowerBound, UpperBound);
    return (int)((float)_width * clampedPercentile) / 2;
  }

  public bool Contains(int horizontalOffset, int verticalOffset)
  {
    int halfHeight = _height / 2;
    if (verticalOffset < -halfHeight || verticalOffset > halfHeight)
    {
      return false;
    }

    int halfWidth = GetHalfWidth(verticalOffset);
    return horizontalOffset >= -halfWidth && horizontalOffset <= halfWidth;
  }

  private static float MultiLerp(float percent, params float[] values)
  {
    float segment = 1f / (values.Length - 1);
    int index = 0;
    while (percent / (segment * (index + 1)) > 1f && index < values.Length - 2)
    {
      index++;
    }

    float localPercent = (percent - segment * index) / segment;
    return values[index] + (values[index + 1] - values[index]) * localPercent;
  }

  private static float WrappedLerp(float value1, float value2, float percent)
  {
    float wrappedPercent = percent * 2f;
    if (wrappedPercent > 1f)
    {
      wrappedPercent = 2f - wrappedPercent;
    }

    return value1 + (value2 - value1) * wrappedPercent;
  }
}
