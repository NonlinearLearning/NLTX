using System.Numerics;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public readonly record struct ParticleRepelValue
{
  public ParticleRepelValue(
    Vector2 anchorPosition,
    Vector2 sourcePosition,
    float radius,
    bool isInWater)
  {
    if (float.IsNaN(radius) || float.IsInfinity(radius) || radius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(radius));
    }

    AnchorPosition = anchorPosition;
    SourcePosition = sourcePosition;
    Radius = radius;
    IsInWater = isInWater;
  }

  public Vector2 AnchorPosition { get; }

  public Vector2 SourcePosition { get; }

  public float Radius { get; }

  public bool IsInWater { get; }
}
