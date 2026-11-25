using System;
using System.Collections.Generic;
using Terraria.Share.Ai;

namespace Terraria.Unshared.Projectiles;

/// <summary>Immutable-after-registration behavior profile directory.</summary>
public sealed class ProjectileBehaviorCatalog
{
  private readonly Dictionary<AiBehaviorProfileId, ProjectileBehaviorProfile> _profiles =
      [];
  private bool _sealed;

  public void Register(ProjectileBehaviorProfile profile)
  {
    ArgumentNullException.ThrowIfNull(profile);
    if (_sealed)
    {
      throw new InvalidOperationException("The projectile behavior catalog is sealed.");
    }

    if (_profiles.ContainsKey(profile.ProfileId))
    {
      throw new InvalidOperationException(
          $"Projectile behavior profile {profile.ProfileId} is already registered.");
    }

    foreach (ProjectileBehaviorProfile existing in _profiles.Values)
    {
      if (existing.BehaviorId == profile.BehaviorId &&
          existing.BehaviorType != profile.BehaviorType)
      {
        throw new InvalidOperationException(
            $"Projectile behavior {profile.BehaviorId} is bound to incompatible AI types.");
      }
    }

    _profiles.Add(profile.ProfileId, profile);
  }

  public void Seal()
  {
    _sealed = true;
  }

  public bool TryGet(
      AiBehaviorId behaviorId,
      AiBehaviorProfileId profileId,
      out ProjectileBehaviorProfile profile)
  {
    if (_profiles.TryGetValue(profileId, out ProjectileBehaviorProfile? candidate) &&
        candidate.BehaviorId == behaviorId)
    {
      profile = candidate;
      return true;
    }

    profile = null!;
    return false;
  }

  public ProjectileBehaviorProfile Get(
      AiBehaviorId behaviorId,
      AiBehaviorProfileId profileId)
  {
    if (!TryGet(behaviorId, profileId, out ProjectileBehaviorProfile profile))
    {
      throw new KeyNotFoundException(
          $"Projectile behavior profile {profileId} is not registered for {behaviorId}.");
    }

    return profile;
  }
}
