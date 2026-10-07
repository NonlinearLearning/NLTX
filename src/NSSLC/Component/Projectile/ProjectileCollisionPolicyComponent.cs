using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹方块碰撞、落台、反弹及拥有者命中检查策略。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：ownerHitCheckDistance（第 98 行）； ignoreWater（第 202 行）； ownerHitCheck（第 216 行）；
/// manualDirectionChange（第 246 行）； correctSlopeCollision（第 250 行）； decidesManualFallThrough（第 252
/// 行）； shouldFallThrough（第 254 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
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
