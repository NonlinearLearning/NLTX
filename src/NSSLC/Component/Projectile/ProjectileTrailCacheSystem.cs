using System;
using System.Numerics;

namespace Terraria.Projectile;

public static class ProjectileTrailCacheSystem
{
  public static void Reset(ref ProjectileTrailCacheComponent state)
  {
    ArgumentNullException.ThrowIfNull(state.OldPositions, nameof(state.OldPositions));
    ArgumentNullException.ThrowIfNull(state.OldRotations, nameof(state.OldRotations));
    ArgumentNullException.ThrowIfNull(
      state.OldSpriteDirections,
      nameof(state.OldSpriteDirections));
    ArgumentNullException.ThrowIfNull(state.WhipPoints, nameof(state.WhipPoints));

    Array.Clear(state.OldPositions, 0, state.OldPositions.Length);
    Array.Clear(state.OldRotations, 0, state.OldRotations.Length);
    Array.Clear(
      state.OldSpriteDirections,
      0,
      state.OldSpriteDirections.Length);
    state.WhipPoints.Clear();
  }

  public static void Record(
    ref ProjectileTrailCacheComponent state,
    Vector2 position,
    float rotation,
    int spriteDirection)
  {
    ArgumentNullException.ThrowIfNull(state.OldPositions, nameof(state.OldPositions));
    ArgumentNullException.ThrowIfNull(state.OldRotations, nameof(state.OldRotations));
    ArgumentNullException.ThrowIfNull(
      state.OldSpriteDirections,
      nameof(state.OldSpriteDirections));

    int historyLength = Math.Min(
      state.OldPositions.Length,
      Math.Min(state.OldRotations.Length, state.OldSpriteDirections.Length));
    for (int index = historyLength - 1; index > 0; index--)
    {
      state.OldPositions[index] = state.OldPositions[index - 1];
      state.OldRotations[index] = state.OldRotations[index - 1];
      state.OldSpriteDirections[index] = state.OldSpriteDirections[index - 1];
    }

    if (historyLength == 0)
    {
      return;
    }

    state.OldPositions[0] = position;
    state.OldRotations[0] = rotation;
    state.OldSpriteDirections[0] = spriteDirection;
  }
}
