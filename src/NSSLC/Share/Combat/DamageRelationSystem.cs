using System;

namespace Terraria.Share.Combat;

/// <summary>Deterministic enemy/friendly relation rules shared by all damage sources.</summary>
public sealed class DamageRelationSystem
{
  public DamageRelationResult Check(
      Faction sourceFaction,
      Faction targetFaction,
      DamagePayload payload,
      bool sameEntity = false)
  {
    if (!Enum.IsDefined(sourceFaction) || !Enum.IsDefined(targetFaction) || !payload.IsValid)
    {
      return DamageRelationResult.CreateRejected(DamageRelationRejectionReason.InvalidInput);
    }

    if (sameEntity && !payload.AllowSelfDamage)
    {
      return DamageRelationResult.CreateRejected(DamageRelationRejectionReason.SelfDamageDisabled);
    }

    if (sameEntity && payload.AllowSelfDamage)
    {
      return DamageRelationResult.CreateAllowed();
    }

    if (sourceFaction == Faction.Neutral || targetFaction == Faction.Neutral)
    {
      return payload.AllowNeutralTargets
          ? DamageRelationResult.CreateAllowed()
          : DamageRelationResult.CreateRejected(DamageRelationRejectionReason.NeutralFaction);
    }

    if (sourceFaction == targetFaction)
    {
      return payload.AllowFriendlyFire
          ? DamageRelationResult.CreateAllowed()
          : DamageRelationResult.CreateRejected(DamageRelationRejectionReason.SameFaction);
    }

    return payload.Relation switch
    {
      DamageRelation.Hostile => DamageRelationResult.CreateAllowed(),
      DamageRelation.Friendly => payload.AllowFriendlyFire
          ? DamageRelationResult.CreateAllowed()
          : DamageRelationResult.CreateRejected(
              DamageRelationRejectionReason.FriendlyFireDisabled),
      DamageRelation.Neutral => DamageRelationResult.CreateRejected(
          DamageRelationRejectionReason.NeutralDamage),
      _ => DamageRelationResult.CreateRejected(DamageRelationRejectionReason.InvalidInput)
    };
  }
}

public readonly record struct DamageRelationResult(
    bool Allowed,
    DamageRelationRejectionReason RejectionReason)
{
  public static DamageRelationResult CreateAllowed()
  {
    return new DamageRelationResult(true, DamageRelationRejectionReason.None);
  }

  public static DamageRelationResult CreateRejected(DamageRelationRejectionReason reason)
  {
    return new DamageRelationResult(false, reason);
  }
}

public enum DamageRelationRejectionReason : byte
{
  None = 0,
  InvalidInput = 1,
  SameFaction = 2,
  NeutralFaction = 3,
  FriendlyFireDisabled = 4,
  NeutralDamage = 5,
  SelfDamageDisabled = 6
}
