using System;
using System.Numerics;

namespace Terraria.Projectile;

public static class ProjectileTrailCacheSystem
{
  /// <summary>
  /// Returns the Version4 TrailingMode value for a projectile type.
  /// Unlisted projectile types use the source set's default value, -1.
  /// Source: Version4 Terraria.ID.ProjectileID.cs:291.
  /// </summary>
  public static int GetTrailingMode(int projectileType)
  {
    return projectileType switch
    {
      94 or 301 or 388 or 385 or 408 or 409 or 435 or 436 or 437 or 438 or 452 or 459 or
      462 or 502 or 503 or 532 or 533 or 573 or 582 or 585 or 592 or 601 or 617 or 636 or
      638 or 639 or 640 or 424 or 425 or 426 or 1037 or 660 or 661 or 664 or 666 or 668 or
      675 or 682 or 684 or 700 or 706 or 709 or 712 or 261 or 721 or 732 or 731 or 739 or
      740 or 741 or 742 or 743 or 744 or 745 or 746 or 747 or 748 or 749 or 750 or 751 or
      752 or 856 or 857 or 902 or 883 or 887 or 893 or 894 or 909 or 964 or 965 or 1026 or
      1047 or 1055 or 1089 or 1090 => 0,
      466 or 580 => 1,
      671 or 680 or 686 or 710 or 711 or 715 or 716 or 717 or 718 or 729 or 755 or 766 or
      767 or 768 or 769 or 770 or 771 or 811 or 814 or 822 or 823 or 824 or 826 or 827 or
      828 or 829 or 830 or 838 or 839 or 840 or 843 or 844 or 845 or 846 or 850 or 852 or
      853 or 864 or 873 or 872 or 833 or 834 or 835 or 818 or 916 or 931 or 946 or 977 or
      976 or 973 or 1020 or 1024 or 1039 or 1045 or 1097 => 2,
      34 or 16 or 79 or 85 or 1001 or 1106 => 3,
      933 or 1100 => 4,
      106 => 5,
      _ => -1,
    };
  }

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

  /// <summary>
  /// Records post-behavior trail history using the supplied Version4 mode inputs.
  /// Returns the mode 1 dust request for execution at the adapter boundary.
  /// </summary>
  public static ProjectileTrailDustRequest? Record(
    ref ProjectileTrailCacheComponent state,
    int trailingMode,
    int frameCounter,
    Vector2 position,
    float rotation,
    int spriteDirection,
    Vector2 velocity,
    int numUpdates,
    Vector2? ownerMovementDelta,
    int projectileType = 0)
  {
    switch (trailingMode)
    {
      case 0:
        ShiftPositionHistory(ref state);
        RecordPosition(ref state, position);
        return null;
      case 1:
        if (!ShouldRecordSparseTrail(ref state, frameCounter))
        {
          return null;
        }

        ShiftPositionHistory(ref state);
        RecordPosition(ref state, position);
        if (velocity == Vector2.Zero &&
          (projectileType == 466 || projectileType == 580))
        {
          int lastPositionIndex = state.OldPositions.Length - 1;
          return new ProjectileTrailDustRequest(
            projectileType,
            state.OldPositions[lastPositionIndex],
            rotation);
        }

        return null;
      case 2:
        RecordFullTrail(
          ref state,
          position,
          rotation,
          spriteDirection);
        return null;
      case 3:
        RecordSmoothedTrail(
          ref state,
          position,
          rotation,
          spriteDirection);
        return null;
      case 4:
        if (!ownerMovementDelta.HasValue)
        {
          throw new ArgumentNullException(nameof(ownerMovementDelta));
        }

        RecordPlayerOffsetTrail(
          ref state,
          position,
          rotation,
          spriteDirection,
          numUpdates,
          ownerMovementDelta.Value);
        return null;
      case 5:
        RecordVelocityRotationTrail(
          ref state,
          position,
          spriteDirection,
          velocity);
        return null;
      default:
        return null;
    }
  }

  private static void RecordFullTrail(
    ref ProjectileTrailCacheComponent state,
    Vector2 position,
    float rotation,
    int spriteDirection)
  {
    if (ShiftFullHistory(ref state) == 0)
    {
      return;
    }

    RecordFullTrailHead(ref state, position, rotation, spriteDirection);
  }

  private static void RecordSmoothedTrail(
    ref ProjectileTrailCacheComponent state,
    Vector2 position,
    float rotation,
    int spriteDirection)
  {
    int historyLength = ShiftFullHistory(ref state);
    if (historyLength == 0)
    {
      return;
    }

    RecordFullTrailHead(ref state, position, rotation, spriteDirection);
    const float InterpolationAmount = 0.65f;
    for (int index = historyLength - 1; index > 0; index--)
    {
      if (state.OldPositions[index] == Vector2.Zero)
      {
        continue;
      }

      Vector2 nextPosition = state.OldPositions[index - 1];
      if (Vector2.Distance(state.OldPositions[index], nextPosition) > 2.0f)
      {
        state.OldPositions[index] = Vector2.Lerp(
          state.OldPositions[index],
          nextPosition,
          InterpolationAmount);
      }

      Vector2 direction = NormalizeOrZero(
        nextPosition - state.OldPositions[index]);
      state.OldRotations[index] = (float)Math.Atan2(direction.Y, direction.X);
    }
  }

  private static void RecordPlayerOffsetTrail(
    ref ProjectileTrailCacheComponent state,
    Vector2 position,
    float rotation,
    int spriteDirection,
    int numUpdates,
    Vector2 ownerMovementDelta)
  {
    int historyLength = ShiftFullHistory(ref state);
    if (historyLength == 0)
    {
      return;
    }

    for (int index = 1; index < historyLength; index++)
    {
      if (numUpdates == 0 && state.OldPositions[index] != Vector2.Zero)
      {
        state.OldPositions[index] += ownerMovementDelta;
      }
    }

    RecordFullTrailHead(ref state, position, rotation, spriteDirection);
  }

  private static void RecordVelocityRotationTrail(
    ref ProjectileTrailCacheComponent state,
    Vector2 position,
    int spriteDirection,
    Vector2 velocity)
  {
    if (ShiftFullHistory(ref state) == 0)
    {
      return;
    }

    float velocityRotation = (float)Math.Atan2(velocity.Y, velocity.X);
    RecordFullTrailHead(ref state, position, velocityRotation, spriteDirection);
  }

  private static bool ShouldRecordSparseTrail(
    ref ProjectileTrailCacheComponent state,
    int frameCounter)
  {
    ArgumentNullException.ThrowIfNull(state.OldPositions, nameof(state.OldPositions));
    return state.OldPositions.Length > 0 &&
      (frameCounter == 0 || state.OldPositions[0] == Vector2.Zero);
  }

  private static void ShiftPositionHistory(ref ProjectileTrailCacheComponent state)
  {
    ArgumentNullException.ThrowIfNull(state.OldPositions, nameof(state.OldPositions));
    for (int index = state.OldPositions.Length - 1; index > 0; index--)
    {
      state.OldPositions[index] = state.OldPositions[index - 1];
    }
  }

  private static int ShiftFullHistory(ref ProjectileTrailCacheComponent state)
  {
    ArgumentNullException.ThrowIfNull(state.OldPositions, nameof(state.OldPositions));
    ArgumentNullException.ThrowIfNull(state.OldRotations, nameof(state.OldRotations));
    ArgumentNullException.ThrowIfNull(
      state.OldSpriteDirections,
      nameof(state.OldSpriteDirections));
    if (state.OldRotations.Length != state.OldPositions.Length ||
      state.OldSpriteDirections.Length != state.OldPositions.Length)
    {
      throw new InvalidOperationException(
        "Projectile trail history arrays must have matching lengths.");
    }

    for (int index = state.OldPositions.Length - 1; index > 0; index--)
    {
      state.OldPositions[index] = state.OldPositions[index - 1];
      state.OldRotations[index] = state.OldRotations[index - 1];
      state.OldSpriteDirections[index] = state.OldSpriteDirections[index - 1];
    }

    return state.OldPositions.Length;
  }

  private static void RecordPosition(
    ref ProjectileTrailCacheComponent state,
    Vector2 position)
  {
    if (state.OldPositions.Length > 0)
    {
      state.OldPositions[0] = position;
    }
  }

  private static void RecordFullTrailHead(
    ref ProjectileTrailCacheComponent state,
    Vector2 position,
    float rotation,
    int spriteDirection)
  {
    state.OldPositions[0] = position;
    state.OldRotations[0] = rotation;
    state.OldSpriteDirections[0] = spriteDirection;
  }

  private static Vector2 NormalizeOrZero(Vector2 vector)
  {
    return vector == Vector2.Zero || float.IsNaN(vector.X) || float.IsNaN(vector.Y)
      ? Vector2.Zero
      : Vector2.Normalize(vector);
  }
}
