using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public static class InterceptionQuery
{
  public static InterceptionResult Calculate(
    Vector2 targetPosition,
    Vector2 targetVelocity,
    Vector2 chaserPosition,
    float chaserSpeed)
  {
    if (chaserSpeed <= 0)
    {
      return new InterceptionResult(false, Vector2.Zero, 0, Vector2.Zero);
    }

    Vector2 relativePosition = targetPosition - chaserPosition;
    float a = Vector2.Dot(targetVelocity, targetVelocity) - chaserSpeed * chaserSpeed;
    float b = 2 * Vector2.Dot(relativePosition, targetVelocity);
    float c = Vector2.Dot(relativePosition, relativePosition);
    float time = SolvePositiveTime(a, b, c);
    if (time < 0)
    {
      return new InterceptionResult(false, Vector2.Zero, 0, Vector2.Zero);
    }

    Vector2 position = targetPosition + targetVelocity * time;
    Vector2 velocity = Vector2.Normalize(position - chaserPosition) * chaserSpeed;
    return new InterceptionResult(true, position, time, velocity);
  }

  private static float SolvePositiveTime(float a, float b, float c)
  {
    const float Epsilon = 0.0001f;
    if (MathF.Abs(a) < Epsilon)
    {
      return MathF.Abs(b) < Epsilon ? -1 : MathF.Max(0, -c / b);
    }

    float discriminant = b * b - 4 * a * c;
    if (discriminant < 0)
    {
      return -1;
    }

    float root = MathF.Sqrt(discriminant);
    float first = (-b - root) / (2 * a);
    float second = (-b + root) / (2 * a);
    float best = float.MaxValue;
    if (first >= 0)
    {
      best = first;
    }

    if (second >= 0)
    {
      best = MathF.Min(best, second);
    }

    return best == float.MaxValue ? -1 : best;
  }
}
