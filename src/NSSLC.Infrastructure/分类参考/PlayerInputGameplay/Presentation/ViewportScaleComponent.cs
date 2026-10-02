using System.Numerics;

namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class ViewportScaleComponent
{
  public float WantedScale { get; private set; } = 1f;

  public float UsedScale { get; private set; } = 1f;

  public Matrix3x2 ScaleMatrix => Matrix3x2.CreateScale(UsedScale);

  public void Set(float scale)
  {
    if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    WantedScale = scale;
    UsedScale = scale;
  }
}
