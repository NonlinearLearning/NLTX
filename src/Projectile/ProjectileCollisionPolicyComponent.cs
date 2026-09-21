using System;

namespace Terraria.Projectile;

public struct ProjectileCollisionPolicyComponent
{
  public ProjectileCollisionPolicyComponent(
    bool tileCollisionEnabled = true,
    bool ignoreWater = false,
    bool correctSlopeCollision = false,
    bool decidesManualFallThrough = false,
    bool shouldFallThrough = false,
    bool reflectsFromTiles = false,
    int maximumBounces = 0,
    float bounceVelocityMultiplier = 1.0f,
    float minimumBounceSpeed = 0.0f,
    bool ownerHitCheck = false,
    float ownerHitCheckDistance = 1000.0f,
    bool manualDirectionChange = false)
  {
    if (maximumBounces < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumBounces));
    }

    ValidateNonNegativeFinite(
      bounceVelocityMultiplier,
      nameof(bounceVelocityMultiplier));
    ValidateNonNegativeFinite(minimumBounceSpeed, nameof(minimumBounceSpeed));
    ValidateNonNegativeFinite(ownerHitCheckDistance, nameof(ownerHitCheckDistance));

    TileCollisionEnabled = tileCollisionEnabled;
    IgnoreWater = ignoreWater;
    CorrectSlopeCollision = correctSlopeCollision;
    DecidesManualFallThrough = decidesManualFallThrough;
    ShouldFallThrough = shouldFallThrough;
    ReflectsFromTiles = reflectsFromTiles;
    MaximumBounces = maximumBounces;
    BounceVelocityMultiplier = bounceVelocityMultiplier;
    MinimumBounceSpeed = minimumBounceSpeed;
    OwnerHitCheck = ownerHitCheck;
    OwnerHitCheckDistance = ownerHitCheckDistance;
    ManualDirectionChange = manualDirectionChange;
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

  public bool TileCollisionEnabled;
  public bool IgnoreWater;
  public bool CorrectSlopeCollision;
  public bool DecidesManualFallThrough;
  public bool ShouldFallThrough;
  public bool ReflectsFromTiles;
  public int MaximumBounces;
  public float BounceVelocityMultiplier;
  public float MinimumBounceSpeed;
  public bool OwnerHitCheck;
  public float OwnerHitCheckDistance;
  public bool ManualDirectionChange;
}
