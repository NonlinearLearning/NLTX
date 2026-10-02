using System.Numerics;

namespace Terraria.Input.LockOn;

public static class LockOnPredictionQuery
{
  private const float PredictionHorizonTicks = 45f;
  private const float CompensationDistance = 700f;

  public static Vector2 Predict(
    Vector2 origin,
    Vector2 targetPosition,
    Vector2 targetVelocity,
    bool hasTarget,
    int aimAboveTiles = 0,
    Func<Vector2, bool>? isSolid = null,
    float? compensation = null,
    float targetHeight = 0f)
  {
    if (!hasTarget)
    {
      return Vector2.Zero;
    }

    float distance = Vector2.Distance(origin, targetPosition);
    float horizon = distance / LockOnPolicy.DefaultRangePixels * PredictionHorizonTicks;
    Vector2 predicted = targetPosition + targetVelocity * horizon;
    for (int step = 0; step < aimAboveTiles && predicted.Y > 100f; step++)
    {
      Vector2 candidate = predicted + new Vector2(0f, -16f);
      if (isSolid?.Invoke(candidate) == true)
      {
        break;
      }

      predicted = candidate;
    }

    if (compensation.HasValue)
    {
      predicted = ApplyCompensation(
        origin,
        predicted,
        targetHeight,
        compensation.Value);
    }

    return predicted;
  }

  private static Vector2 ApplyCompensation(
    Vector2 origin,
    Vector2 predicted,
    float targetHeight,
    float compensation)
  {
    predicted.Y -= targetHeight / 2f;
    Vector2 offset = predicted - origin;
    float length = offset.Length();
    if (length <= 0f)
    {
      return predicted;
    }

    Vector2 direction = Vector2.Normalize(offset);
    direction.Y -= 1f;
    float curve = MathF.Pow(length / CompensationDistance, 2f) * CompensationDistance;
    predicted.Y += direction.Y * curve * compensation;
    predicted.X -= direction.X * curve * compensation;
    return predicted;
  }
}
