using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Relationships;
using Terraria.Attributes;

namespace Terraria.Combat;

/// <summary>
/// Sole conventional writer of target <see cref="LifeComponent"/> for damage requests.
/// </summary>
public sealed class DamageCommitSystem
{
  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;

  public DamageCommitSystem(ArchWorld world, IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  /// <summary>Commits a source-agnostic request after all combat rules have been calculated.</summary>
  public DamageCommitResult TryCommit(
      in DamageRequest request,
      in DamageMitigationResult mitigation,
      out DamageAppliedReceipt receipt)
  {
    receipt = default;
    if (!request.Source.IsValid || !request.Target.IsValid || !request.Payload.IsValid ||
        request.Tick < 0 || request.RequestId < 0 || !Enum.IsDefined(request.Cause) ||
        request.ExpectedTargetLife.Maximum <= 0 || mitigation.IncomingDamage < 0 ||
        !float.IsFinite(mitigation.Reduction) || mitigation.Reduction < 0f ||
        mitigation.Reduction > 1f || mitigation.AcceptedDamage < 0 ||
        mitigation.AcceptedDamage > mitigation.IncomingDamage ||
        request.Payload.BaseDamage <= 0)
    {
      return DamageCommitResult.InvalidDamage;
    }

    if (!mitigation.HasEffect)
    {
      return DamageCommitResult.NoDamage;
    }

    if (!_resolver.TryResolve(request.Source, out ArchEntity source) ||
        source == ArchEntity.Null || !_world.IsAlive(source))
    {
      return DamageCommitResult.SourceUnavailable;
    }

    if (_world.Has<LifeComponent>(source) && !_world.Get<LifeComponent>(source).IsAlive)
    {
      return DamageCommitResult.SourceInactive;
    }

    if (!_resolver.TryResolve(request.Target, out ArchEntity target) ||
        target == ArchEntity.Null || !_world.IsAlive(target) ||
        !_world.Has<LifeComponent>(target))
    {
      return DamageCommitResult.TargetUnavailable;
    }

    ref LifeComponent life = ref _world.Get<LifeComponent>(target);    if (!life.Equals(request.ExpectedTargetLife))
    {
      return DamageCommitResult.TargetChanged;
    }

    if (!life.IsAlive)
    {
      return DamageCommitResult.TargetChanged;
    }

    LifeComponent before = life;
    if (mitigation.ShieldAbsorbedDamage > 0)
    {
      // 护盾池的唯一写入点是 Attributes 域：Combat 只提交请求并读取结果。
      AttributeShieldConsumeResult shield = AttributeShieldCommands.Apply(
          _world,
          target,
          new AttributeShieldConsumeRequestComponent(mitigation.ShieldAbsorbedDamage));
      if (shield.HasAttributes && shield.Consumed != mitigation.ShieldAbsorbedDamage)
      {
        return DamageCommitResult.TargetChanged;
      }
    }

    int nextCurrent = Math.Max(life.Minimum, life.Current - mitigation.AcceptedDamage);
    LifeComponent after = life.WithCurrent(nextCurrent);
    life = after;
    receipt = new DamageAppliedReceipt(
        request,
        mitigation.AcceptedDamage,
        before,
        after,
        mitigation,
        request.Tick);
    return DamageCommitResult.Applied;
  }
}
