using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Attributes;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>
/// Coordinates source-agnostic damage requests for one local ECS commit boundary.
/// </summary>
/// <remarks>
/// The coordinator owns no target state. It reads source/target facts, invokes deterministic rules,
/// and delegates the only LifeComponent write to DamageCommitSystem.
/// </remarks>
public sealed class CombatDamageSystem
{
  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;
  private readonly DamageAdmissionSystem _admission;
  private readonly DamageRelationSystem _relations;
  private readonly DamageMitigationSystem _mitigation;
  private readonly DamageCommitSystem _commit;

  public CombatDamageSystem(ArchWorld world, IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _admission = new DamageAdmissionSystem();
    _relations = new DamageRelationSystem();
    _mitigation = new DamageMitigationSystem();
    _commit = new DamageCommitSystem(_world, _resolver);
  }

  /// <summary>Processes a stable request order for one authoritative tick.</summary>
  public IReadOnlyList<DamageProcessResult> Process(
      IReadOnlyList<DamageRequest> requests,
      long tick)
  {
    ArgumentNullException.ThrowIfNull(requests);
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    List<IndexedDamageRequest> ordered = new(requests.Count);
    for (int index = 0; index < requests.Count; index++)
    {
      ordered.Add(new IndexedDamageRequest(requests[index], index));
    }

    ordered.Sort(IndexedDamageRequestComparer.Instance);
    DamageProcessResult[] results = new DamageProcessResult[requests.Count];
    HashSet<DamageRequestKey> processed = [];
    foreach (IndexedDamageRequest indexed in ordered)
    {
      results[indexed.OriginalIndex] = ProcessOne(indexed.Request, tick, processed);
    }

    return Array.AsReadOnly(results);
  }

  /// <summary>Processes one request and uses the supplied duplicate set for the current tick.</summary>
  public DamageProcessResult ProcessOne(
      in DamageRequest request,
      long tick,
      ISet<DamageRequestKey>? processed = null)
  {
    if (tick < 0 || request.Tick != tick)
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.StaleTick);
    }

    ISet<DamageRequestKey> localProcessed = processed ?? new HashSet<DamageRequestKey>();
    if (!localProcessed.Add(request.Key))
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.DuplicateRequest);
    }

    if (!_resolver.TryResolve(request.Source, out ArchEntity source) ||
        source == ArchEntity.Null || !_world.IsAlive(source))
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.SourceUnavailable);
    }

    if (_world.Has<LifeComponent>(source) && !_world.Get<LifeComponent>(source).IsAlive)
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.SourceInactive);
    }

    if (!_resolver.TryResolve(request.Target, out ArchEntity target) ||
        target == ArchEntity.Null || !_world.IsAlive(target) ||
        !_world.Has<LifeComponent>(target))
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.TargetUnavailable);
    }

    if (!_world.Has<EntityFactionComponent>(source) ||
        !_world.Has<EntityFactionComponent>(target))
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.InvalidRequest);
    }

    LifeComponent targetLife = _world.Get<LifeComponent>(target);
    ProtectionWindowComponent? protection = _world.Has<ProtectionWindowComponent>(target)
        ? _world.Get<ProtectionWindowComponent>(target)
        : null;
    DamageRejectionReason admission = _admission.Check(
        in request,
        in targetLife,
        protection);
    if (admission != DamageRejectionReason.None)
    {
      return DamageProcessResult.Rejected(admission);
    }

    DamageRelationResult relation = _relations.Check(
        _world.Get<EntityFactionComponent>(source).Value,
        _world.Get<EntityFactionComponent>(target).Value,
        request.Payload,
        request.Source.Matches(request.Target));
    if (!relation.Allowed)
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.RelationRejected);
    }

    AttributeSetComponent? sourceAttributes = _world.Has<AttributeSetComponent>(source)
        ? _world.Get<AttributeSetComponent>(source)
        : null;
    AttributeSetComponent? targetAttributes = _world.Has<AttributeSetComponent>(target)
        ? _world.Get<AttributeSetComponent>(target)
        : null;
    DamagePayload payload = request.Payload;
    DamageMitigationResult mitigation = _mitigation.Calculate(
        in payload,
        sourceAttributes,
        targetAttributes,
        targetLife);
    if (!mitigation.HasEffect)
    {
      return DamageProcessResult.Rejected(DamageRejectionReason.NoDamageAfterMitigation);
    }

    DamageCommitResult committed = _commit.TryCommit(
        in request,
        in mitigation,
        targetAttributes,
        out DamageAppliedReceipt receipt);
    return committed switch
    {
      DamageCommitResult.Applied => DamageProcessResult.Accepted(in receipt),
      DamageCommitResult.TargetChanged => DamageProcessResult.Rejected(
          DamageRejectionReason.TargetChanged),
      DamageCommitResult.TargetUnavailable => DamageProcessResult.Rejected(
          DamageRejectionReason.TargetUnavailable),
      DamageCommitResult.NoDamage => DamageProcessResult.Rejected(
          DamageRejectionReason.NoDamageAfterMitigation),
      DamageCommitResult.InvalidDamage => DamageProcessResult.Rejected(
          DamageRejectionReason.InvalidDamage),
      _ => DamageProcessResult.Rejected(DamageRejectionReason.SourceUnavailable)
    };
  }

  private readonly record struct IndexedDamageRequest(
      DamageRequest Request,
      int OriginalIndex);

  private sealed class IndexedDamageRequestComparer : IComparer<IndexedDamageRequest>
  {
    public static IndexedDamageRequestComparer Instance { get; } = new();

    public int Compare(IndexedDamageRequest left, IndexedDamageRequest right)
    {
      DamageRequest leftRequest = left.Request;
      DamageRequest rightRequest = right.Request;
      int comparison = CompareHandles(leftRequest.Source, rightRequest.Source);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = CompareHandles(leftRequest.Target, rightRequest.Target);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Cause.CompareTo(rightRequest.Cause);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.RequestId.CompareTo(rightRequest.RequestId);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = rightRequest.Payload.BaseDamage.CompareTo(leftRequest.Payload.BaseDamage);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.Type.CompareTo(rightRequest.Payload.Type);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.Relation.CompareTo(rightRequest.Payload.Relation);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.Element.CompareTo(rightRequest.Payload.Element);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.ElementalPenetration.CompareTo(
          rightRequest.Payload.ElementalPenetration);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.Critical.CompareTo(rightRequest.Payload.Critical);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.ArmorPenetration.CompareTo(
          rightRequest.Payload.ArmorPenetration);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.Knockback.CompareTo(rightRequest.Payload.Knockback);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.AllowFriendlyFire.CompareTo(
          rightRequest.Payload.AllowFriendlyFire);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.AllowSelfDamage.CompareTo(
          rightRequest.Payload.AllowSelfDamage);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.Payload.AllowNeutralTargets.CompareTo(
          rightRequest.Payload.AllowNeutralTargets);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.ExpectedTargetLife.Current.CompareTo(
          rightRequest.ExpectedTargetLife.Current);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.ExpectedTargetLife.Maximum.CompareTo(
          rightRequest.ExpectedTargetLife.Maximum);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftRequest.ExpectedTargetLife.Minimum.CompareTo(
          rightRequest.ExpectedTargetLife.Minimum);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = CompareContacts(leftRequest.Contact, rightRequest.Contact);
      return comparison != 0 ? comparison : left.OriginalIndex.CompareTo(right.OriginalIndex);
    }

    private static int CompareHandles(
        EntityRuntimeHandle left,
        EntityRuntimeHandle right)
    {
      int comparison = left.WorldScope.CompareTo(right.WorldScope);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = left.Id.Value.CompareTo(right.Id.Value);
      return comparison != 0 ? comparison : left.Generation.CompareTo(right.Generation);
    }

    private static int CompareContacts(DamageContact? left, DamageContact? right)
    {
      int comparison = left.HasValue.CompareTo(right.HasValue);
      if (comparison != 0 || !left.HasValue)
      {
        return comparison;
      }

      DamageContact leftContact = left.GetValueOrDefault();
      DamageContact rightContact = right.GetValueOrDefault();
      comparison = leftContact.Point.X.CompareTo(rightContact.Point.X);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftContact.Point.Y.CompareTo(rightContact.Point.Y);
      if (comparison != 0)
      {
        return comparison;
      }

      comparison = leftContact.Normal.X.CompareTo(rightContact.Normal.X);
      return comparison != 0
          ? comparison
          : leftContact.Normal.Y.CompareTo(rightContact.Normal.Y);
    }
  }
}
