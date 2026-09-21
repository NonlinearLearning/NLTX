using System;

namespace Terraria.Projectile;

public static class ProjectileCollisionPolicySystem
{
  public static void Initialize(
    ref ProjectileCollisionPolicyComponent state,
    in ProjectileCollisionPolicyComponent policy)
  {
    Validate(policy);
    state = policy;
  }

  public static void SetTileCollisionEnabled(
    ref ProjectileCollisionPolicyComponent state,
    bool enabled)
  {
    state.TileCollisionEnabled = enabled;
  }

  public static void SetIgnoreWater(
    ref ProjectileCollisionPolicyComponent state,
    bool ignoreWater)
  {
    state.IgnoreWater = ignoreWater;
  }

  public static void SetCorrectSlopeCollision(
    ref ProjectileCollisionPolicyComponent state,
    bool correctSlopeCollision)
  {
    state.CorrectSlopeCollision = correctSlopeCollision;
  }

  public static void SetDecidesManualFallThrough(
    ref ProjectileCollisionPolicyComponent state,
    bool decidesManualFallThrough)
  {
    state.DecidesManualFallThrough = decidesManualFallThrough;
  }

  public static void SetShouldFallThrough(
    ref ProjectileCollisionPolicyComponent state,
    bool shouldFallThrough)
  {
    state.ShouldFallThrough = shouldFallThrough;
  }

  public static bool CanFallThrough(
    in ProjectileCollisionPolicyComponent state)
  {
    return state.DecidesManualFallThrough && state.ShouldFallThrough;
  }

  public static void SetReflectsFromTiles(
    ref ProjectileCollisionPolicyComponent state,
    bool reflectsFromTiles)
  {
    state.ReflectsFromTiles = reflectsFromTiles;
  }

  public static void SetBouncePolicy(
    ref ProjectileCollisionPolicyComponent state,
    int maximumBounces,
    float bounceVelocityMultiplier,
    float minimumBounceSpeed)
  {
    ValidateBouncePolicy(
      maximumBounces,
      bounceVelocityMultiplier,
      minimumBounceSpeed);

    state.MaximumBounces = maximumBounces;
    state.BounceVelocityMultiplier = bounceVelocityMultiplier;
    state.MinimumBounceSpeed = minimumBounceSpeed;
  }

  public static void SetOwnerHitCheck(
    ref ProjectileCollisionPolicyComponent state,
    bool ownerHitCheck)
  {
    state.OwnerHitCheck = ownerHitCheck;
  }

  public static void SetOwnerHitCheckDistance(
    ref ProjectileCollisionPolicyComponent state,
    float ownerHitCheckDistance)
  {
    ValidateNonNegativeFinite(ownerHitCheckDistance, nameof(ownerHitCheckDistance));
    state.OwnerHitCheckDistance = ownerHitCheckDistance;
  }

  public static void SetManualDirectionChange(
    ref ProjectileCollisionPolicyComponent state,
    bool manualDirectionChange)
  {
    state.ManualDirectionChange = manualDirectionChange;
  }

  private static void Validate(
    in ProjectileCollisionPolicyComponent state)
  {
    ValidateBouncePolicy(
      state.MaximumBounces,
      state.BounceVelocityMultiplier,
      state.MinimumBounceSpeed);
    ValidateNonNegativeFinite(
      state.OwnerHitCheckDistance,
      nameof(state.OwnerHitCheckDistance));
  }

  private static void ValidateBouncePolicy(
    int maximumBounces,
    float bounceVelocityMultiplier,
    float minimumBounceSpeed)
  {
    if (maximumBounces < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumBounces));
    }

    ValidateNonNegativeFinite(
      bounceVelocityMultiplier,
      nameof(bounceVelocityMultiplier));
    ValidateNonNegativeFinite(minimumBounceSpeed, nameof(minimumBounceSpeed));
  }

  private static void ValidateNonNegativeFinite(
    float value,
    string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
