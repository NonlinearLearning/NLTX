using System;
using System.Numerics;
using Terraria.Player.Grapple;
using Terraria.Player.Jump;

namespace Terraria.Player.Mobility;

// status: isolated-core
// crossSubsystemOwner: Player.Update routing, Spatial, Collision, Projectile lifecycle, and effects
public sealed class PlayerMobilitySystem
{
  private const int SolarFlareGrappleType = 446;
  private const int GravityGrappleType = 652;
  private const int DirectionalGrappleType = 865;
  private const int SlimeGrappleSpeedType = 315;
  private const int WebGrappleSpeedType = 487;
  private const int GemGrappleSpeedMinimumType = 646;
  private const int GemGrappleSpeedMaximumType = 649;

  public void RefreshDoubleJumps(
    PlayerJumpAvailabilityComponent availability,
    PlayerJumpExecutionComponent execution)
  {
    ArgumentNullException.ThrowIfNull(availability);
    ArgumentNullException.ThrowIfNull(execution);

    execution.IsPerformingDownDash = false;
    availability.CanJumpAgainCloud |= availability.HasCloudOption;
    availability.CanJumpAgainSandstorm |= availability.HasSandstormOption;
    availability.CanJumpAgainBlizzard |= availability.HasBlizzardOption;
    availability.CanJumpAgainFart |= availability.HasFartOption;
    availability.CanJumpAgainSail |= availability.HasSailOption;
    availability.CanJumpAgainUnicorn |= availability.HasUnicornOption;
    availability.CanJumpAgainSantank |= availability.HasSantankOption;
    availability.CanJumpAgainWallOfFleshGoat |= availability.HasWallOfFleshGoatOption;
    availability.CanJumpAgainBasilisk |= availability.HasBasiliskOption;
  }

  public PlayerJumpParameterResult UpdateJumpParameters(
    PlayerJumpMobilityModifiersComponent modifiers,
    in PlayerJumpParameterInput input)
  {
    ArgumentNullException.ThrowIfNull(modifiers);

    int jumpHeight = input.JumpHeight;
    float jumpSpeed = input.JumpSpeed;
    if (input.MountActive)
    {
      jumpHeight = input.MountJumpHeight;
      jumpSpeed = input.MountJumpSpeed;
    }
    else
    {
      if (input.JumpBoost)
      {
        jumpHeight = Math.Max(jumpHeight, 20);
        jumpSpeed = Math.Max(jumpSpeed, 6.51f);
      }

      if (input.EmpressBrooch)
      {
        modifiers.JumpSpeedBoost += 1.8f;
      }

      if (input.FrogLegJumpBoost)
      {
        modifiers.JumpSpeedBoost += 2.4f;
        modifiers.ExtraFall += 15;
      }

      if (input.MoonLordLegs)
      {
        modifiers.JumpSpeedBoost += 1.8f;
        modifiers.ExtraFall += 10;
        jumpHeight++;
      }

      if (input.WereWolf)
      {
        jumpHeight += 2;
        jumpSpeed += 0.2f;
      }

      if (input.PortableStoolInUse)
      {
        jumpHeight += 5;
      }

      jumpSpeed += modifiers.JumpSpeedBoost;
    }

    if (input.Sticky)
    {
      jumpHeight /= 10;
      jumpSpeed /= 5f;
    }

    if (input.Dazed)
    {
      jumpHeight /= 5;
      jumpSpeed /= 2f;
    }

    return new PlayerJumpParameterResult(jumpHeight, jumpSpeed);
  }

  public void RefreshMovementAbilities(
    PlayerFlightStateComponent flight,
    PlayerRocketStateComponent rocket,
    bool doubleJumps = true,
    PlayerJumpAvailabilityComponent? availability = null,
    PlayerJumpExecutionComponent? execution = null)
  {
    ArgumentNullException.ThrowIfNull(flight);
    ArgumentNullException.ThrowIfNull(rocket);
    if (doubleJumps)
    {
      ArgumentNullException.ThrowIfNull(availability);
      ArgumentNullException.ThrowIfNull(execution);
    }

    flight.WingTime = flight.WingTimeMax;
    rocket.RocketTime = rocket.RocketTimeMax;
    rocket.RocketDelay = 0;
    if (doubleJumps)
    {
      RefreshDoubleJumps(availability!, execution!);
    }
  }

  public bool CanMoveForwardOnRope(
    bool destinationIsActiveRope,
    bool intersectsSolidCollision)
  {
    return destinationIsActiveRope && !intersectsSolidCollision;
  }

  public PlayerGrappleForcesResult GetGrapplingForces(
    in PlayerGrappleForcesInput input)
  {
    ArgumentNullException.ThrowIfNull(input.ProjectilesInSlotOrder);

    Vector2 targetPositionSum = Vector2.Zero;
    int validProjectileCount = 0;
    int? preferredDirection = null;
    Vector2 controlDirection = CreateControlDirection(input);

    foreach (PlayerGrappleProjectileSnapshot projectile in input.ProjectilesInSlotOrder)
    {
      if (projectile.Ai0 != 2f || projectile.HasNaNPosition)
      {
        continue;
      }

      Vector2 projectileCenter = projectile.Center;
      targetPositionSum += projectileCenter;
      validProjectileCount++;

      if (projectile.Type == SolarFlareGrappleType)
      {
        Vector2 controlOffset = controlDirection;
        if (controlOffset != Vector2.Zero)
        {
          controlOffset = Vector2.Normalize(controlOffset);
        }

        controlOffset *= 100f;
        Vector2 towardTarget = NormalizeAndReplaceNaNs(
          input.PlayerCenter - projectileCenter + controlOffset,
          new Vector2(0f, -1f));
        targetPositionSum += towardTarget * 200f;
      }
      else if (projectile.Type == GravityGrappleType)
      {
        Vector2 normalizedControl = NormalizeOrFallback(controlDirection, Vector2.Zero);
        Vector2 toGrapple = projectileCenter - input.PlayerCenter;
        Vector2 towardGrapple = NormalizeOrFallback(toGrapple, Vector2.Zero);
        Vector2 controlProjection = Vector2.Zero;
        if (normalizedControl != Vector2.Zero)
        {
          controlProjection = towardGrapple * Vector2.Dot(towardGrapple, normalizedControl);
        }

        float pullScale = 6f;
        if (Vector2.Dot(controlProjection, toGrapple) < 0f && toGrapple.Length() >= 600f)
        {
          pullScale = 0f;
        }

        targetPositionSum += -toGrapple + controlProjection * pullScale;
      }
      else if (projectile.Type == DirectionalGrappleType)
      {
        float angle = projectile.Rotation - (float)Math.PI / 2f;
        Vector2 ropeDirection = new(MathF.Cos(angle), MathF.Sin(angle));
        ropeDirection = NormalizeOrFallback(ropeDirection, Vector2.UnitY);
        targetPositionSum += -ropeDirection * 28f;
        if (ropeDirection.X != 0f)
        {
          preferredDirection = Math.Sign(ropeDirection.X);
        }
      }
    }

    if (validProjectileCount == 0)
    {
      return new PlayerGrappleForcesResult(false, null, input.CurrentVelocity);
    }

    Vector2 targetPosition = targetPositionSum / validProjectileCount;
    Vector2 preferredVelocity = targetPosition - input.FromPosition;
    float velocityLength = preferredVelocity.Length();
    float maximumSpeed = GetMaximumGrappleSpeed(input.ProjectilesInSlotOrder[0].Type);
    if (velocityLength > maximumSpeed)
    {
      preferredVelocity *= maximumSpeed / velocityLength;
    }

    return new PlayerGrappleForcesResult(true, preferredDirection, preferredVelocity);
  }

  private static Vector2 CreateControlDirection(in PlayerGrappleForcesInput input)
  {
    float horizontal = (input.ControlRight ? 1f : 0f) - (input.ControlLeft ? 1f : 0f);
    float vertical = (input.ControlDown ? 1f : 0f) - (input.ControlUp ? 1f : 0f);
    return new Vector2(horizontal, vertical * input.GravityDirection);
  }

  private static Vector2 NormalizeOrFallback(Vector2 value, Vector2 fallback)
  {
    if (value == Vector2.Zero || HasNaNs(value))
    {
      return fallback;
    }

    return Vector2.Normalize(value);
  }

  private static Vector2 NormalizeAndReplaceNaNs(Vector2 value, Vector2 fallback)
  {
    Vector2 normalized = Vector2.Normalize(value);
    return HasNaNs(normalized) ? fallback : normalized;
  }

  private static bool HasNaNs(Vector2 value)
  {
    return float.IsNaN(value.X) || float.IsNaN(value.Y);
  }

  private static float GetMaximumGrappleSpeed(int projectileType)
  {
    if (projectileType == SlimeGrappleSpeedType)
    {
      return 14f;
    }

    if (projectileType == WebGrappleSpeedType)
    {
      return 12f;
    }

    if (projectileType >= GemGrappleSpeedMinimumType &&
      projectileType <= GemGrappleSpeedMaximumType)
    {
      return 16f;
    }

    return 11f;
  }
}
