using System;
using Terraria.Share.Ai;

namespace Terraria.Unshared.Projectiles;

/// <summary>
/// Immutable parameters and binding metadata for one projectile behavior profile.
/// It contains no entity, world, planner or mutable execution state.
/// </summary>
public sealed class ProjectileBehaviorProfile
{
  public ProjectileBehaviorProfile(
      AiBehaviorId behaviorId,
      AiBehaviorProfileId profileId,
      Type behaviorType,
      AiRequirements requirements,
      float maximumSpeed,
      float turnRate,
      float range)
  {
    if (!behaviorId.IsValid)
    {
      throw new ArgumentException("A projectile behavior id is required.", nameof(behaviorId));
    }

    if (!profileId.IsValid)
    {
      throw new ArgumentException(
          "A projectile behavior profile id is required.", nameof(profileId));
    }

    ArgumentNullException.ThrowIfNull(behaviorType);
    if (!typeof(IAiBehavior).IsAssignableFrom(behaviorType))
    {
      throw new ArgumentException(
          $"The type {behaviorType.FullName} does not implement IAiBehavior.",
          nameof(behaviorType));
    }

    ArgumentNullException.ThrowIfNull(requirements);
    ValidateNonNegative(maximumSpeed, nameof(maximumSpeed));
    ValidateNonNegative(turnRate, nameof(turnRate));
    ValidateNonNegative(range, nameof(range));

    BehaviorId = behaviorId;
    ProfileId = profileId;
    BehaviorType = behaviorType;
    Requirements = requirements;
    MaximumSpeed = maximumSpeed;
    TurnRate = turnRate;
    Range = range;
  }

  public AiBehaviorId BehaviorId { get; }

  public AiBehaviorProfileId ProfileId { get; }

  public Type BehaviorType { get; }

  public AiRequirements Requirements { get; }

  public float MaximumSpeed { get; }

  public float TurnRate { get; }

  public float Range { get; }

  private static void ValidateNonNegative(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
