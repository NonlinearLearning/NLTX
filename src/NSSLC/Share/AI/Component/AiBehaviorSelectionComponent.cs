using System;

namespace Terraria.Share.Ai;

/// <summary>
/// Selects the content behavior and immutable profile used by an entity's AI adapter.
/// </summary>
public readonly record struct AiBehaviorSelectionComponent
{
  public AiBehaviorSelectionComponent(
      AiBehaviorId behaviorId,
      AiBehaviorProfileId profileId)
  {
    if (!behaviorId.IsValid)
    {
      throw new ArgumentException("An AI behavior id is required.", nameof(behaviorId));
    }

    if (!profileId.IsValid)
    {
      throw new ArgumentException("An AI behavior profile id is required.", nameof(profileId));
    }

    BehaviorId = behaviorId;
    ProfileId = profileId;
  }

  public AiBehaviorSelectionComponent(string behaviorId, string profileId)
      : this(new AiBehaviorId(behaviorId), new AiBehaviorProfileId(profileId))
  {
  }

  public AiBehaviorId BehaviorId { get; }

  public AiBehaviorProfileId ProfileId { get; }
}
