using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>
/// Applies accepted combat receipts to projectile-owned penetration and immunity state.
/// It never writes target life and leaves entity destruction to the lifecycle owner.
/// </summary>
public sealed class ProjectileHitResolutionSystem
{
  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;
  private ProjectileTerminationQueueComponent? _terminations;

  public ProjectileHitResolutionSystem(
      ArchWorld world,
      IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  /// <summary>
  /// World-scoped termination queue, resolved on first use so construction order never decides
  /// which instance a system observes.
  /// </summary>
  private ProjectileTerminationQueueComponent Terminations =>
      _terminations ??= ProjectileTerminationQueue.Require(_world);

  /// <summary>Decides whether this projectile's own hit budget still allows a hit on the target.</summary>
  /// <remarks>
  /// **目标免疫不得加进这个方法。** <see cref="ApplyCore" /> 会在 Combat 已开出窗口之后，
  /// 用同一份已提交回执再查一次准入；把目标免疫塞进来，刚落地的那份回执就会判自己非法，
  /// 穿透预扣永远确认不了。目标免疫是 <see cref="CanHit" /> 的职责，只用于「别入队」。
  /// </remarks>
  public ProjectileHitAdmissionResult CheckAdmission(
      EntityRuntimeHandle projectile,
      EntityRuntimeHandle target,
      long tick)
  {
    if (!_resolver.TryResolve(projectile, out ArchEntity entity) ||
        entity == ArchEntity.Null || !_world.IsAlive(entity))
    {
      return ProjectileHitAdmissionResult.ProjectileUnavailable;
    }

    if (!_world.Has<ProjectileDispositionStateComponent>(entity))
    {
      return ProjectileHitAdmissionResult.ProjectileUnavailable;
    }

    if (Terminations.Contains(projectile, tick))
    {
      return ProjectileHitAdmissionResult.ProjectileInactive;
    }

    ProjectileDispositionStateComponent disposition =
        _world.Get<ProjectileDispositionStateComponent>(entity);
    if (!disposition.IsActive)
    {
      return ProjectileHitAdmissionResult.ProjectileInactive;
    }

    if (_world.Has<ProjectileHitImmunityStateComponent>(entity))
    {
      ProjectileHitImmunityStateComponent immunity =
          _world.Get<ProjectileHitImmunityStateComponent>(entity);
      immunity.Prune(tick);
      if (immunity.AppliesTo(target, tick))
      {
        return ProjectileHitAdmissionResult.TargetImmune;
      }
    }

    return ProjectileHitAdmissionResult.Accepted;
  }

  /// <summary>
  /// Answers "can this projectile hit this target right now" without touching any state.
  /// </summary>
  /// <remarks>
  /// 这是**经济性预检，不是保证**：它在 Simulation 阶段跑，看到的是上一 tick 末的窗口；
  /// 真正的裁决在 Commit 阶段的 <see cref="Terraria.Share.Combat.DamageImmunityQuery" />。
  /// 因此预检可能放行而裁决拒绝（跨 tick 边界），但绝不允许反向——预检拒绝的命中不该被静默丢弃的
  /// 情况不存在，因为预检拒绝时压根没有入队。任何「只有预检、没有裁决」的实现都是缺陷。
  /// </remarks>
  public bool CanHit(
      EntityRuntimeHandle projectile,
      EntityRuntimeHandle target,
      ImmunityChannel channel,
      long tick,
      DamageType damageType = DamageType.Generic)
  {
    if (!_resolver.TryResolve(target, out ArchEntity targetEntity) ||
        targetEntity == ArchEntity.Null || !_world.IsAlive(targetEntity))
    {
      return false;
    }

    return DamageImmunityQuery.Check(
        _world,
        targetEntity,
        projectile,
        channel,
        tick,
        damageType) == DamageRejectionReason.None;
  }

  /// <summary>
  /// Requests admitted by the pre-check and later rejected by authority as immune.
  /// </summary>
  /// <remarks>
  /// 预检是经济性优化，不是保证：它在 Simulation 跑，裁决在 Commit 跑，中间隔一次归零系统，
  /// 所以跨 tick 边界允许出现「预检放行、裁决拒绝」。但这个差额必须可观测——
  /// 计数持续增长说明预检读数过期，长期为 0 说明预检没生效或被人删了。
  /// 绝不允许反向：预检拒绝的命中本可以被裁决放行，那种情况不该发生，因为预检拒绝时压根不入队。
  /// </remarks>
  public int PrecheckAdmittedAuthorityRejected { get; private set; }

  /// <summary>Records that authority refused a hit the pre-check had admitted.</summary>
  /// <remarks>
  /// 只统计「免疫类」拒绝。阵营、目标已死、伤害被减到 0 这些与预检无关——预检只回答免疫问题，
  /// 把它们算进差额会让这个计数变成「所有被拒命中」，那就不再是预检读数过期的证据。
  /// </remarks>
  public void NoteAuthorityRejection(DamageRejectionReason reason)
  {
    switch (reason)
    {
      case DamageRejectionReason.ProtectionWindowActive:
      case DamageRejectionReason.TargetImmune:
        PrecheckAdmittedAuthorityRejected++;
        break;
      default:
        break;
    }
  }

  /// <summary>Consumes a shared receipt produced for a Projectile damage request.</summary>
  public ProjectileHitAdmissionResult Apply(
      in DamageAppliedReceipt receipt,
      out ProjectileTerminationReason? terminationReason)
  {
    return ApplyCore(
        receipt.Source,
        receipt.Target,
        receipt.Tick,
        out terminationReason);
  }

  /// <summary>
  /// 伤害被拒（免疫 / 阵营 / 目标已死）时撤销预扣。
  /// </summary>
  /// <remarks>
  /// 不回滚就会出现"打空也扣穿透"：预扣发生在入队时，而拒绝要等 Combat 结算完才知道。
  /// 拒绝的原因不重要——只要没落地，预算就该原样退回。
  /// </remarks>
  public void Reject(EntityRuntimeHandle projectileHandle)
  {
    if (!_resolver.TryResolve(projectileHandle, out ArchEntity entity) ||
        entity == ArchEntity.Null ||
        !_world.IsAlive(entity) ||
        !_world.Has<ProjectilePenetrationStateComponent>(entity))
    {
      return;
    }

    ref ProjectilePenetrationStateComponent penetration =
        ref _world.Get<ProjectilePenetrationStateComponent>(entity);
    penetration = penetration.RollbackPending();
  }

  /// <summary>入队一次命中前的预扣，返回是否还有额度可用。</summary>
  public bool TryPreviewConsume(EntityRuntimeHandle projectileHandle)
  {
    if (!_resolver.TryResolve(projectileHandle, out ArchEntity entity) ||
        entity == ArchEntity.Null ||
        !_world.IsAlive(entity) ||
        !_world.Has<ProjectilePenetrationStateComponent>(entity))
    {
      // 没有预算组件的弹体按"无限"处理：不因为缺少记账就拒绝开火。
      return true;
    }

    ref ProjectilePenetrationStateComponent penetration =
        ref _world.Get<ProjectilePenetrationStateComponent>(entity);
    // 伤害判据与穿透判据分开：打空只表示"不能再穿"，是否继续造成伤害由内容策略决定。
    if (!penetration.CanDealDamage)
    {
      return false;
    }

    penetration = penetration.PreviewConsume();
    return true;
  }

  private ProjectileHitAdmissionResult ApplyCore(
      EntityRuntimeHandle projectileHandle,
      EntityRuntimeHandle targetHandle,
      long tick,
      out ProjectileTerminationReason? terminationReason)
  {
    terminationReason = null;
    ProjectileHitAdmissionResult admission = CheckAdmission(
        projectileHandle,
        targetHandle,
        tick);
    if (admission != ProjectileHitAdmissionResult.Accepted)
    {
      return admission;
    }

    if (!_resolver.TryResolve(projectileHandle, out ArchEntity entity) ||
        entity == ArchEntity.Null || !_world.IsAlive(entity) ||
        !_world.Has<ProjectileDispositionStateComponent>(entity))
    {
      return ProjectileHitAdmissionResult.ProjectileUnavailable;
    }

    if (!_world.Get<ProjectileDispositionStateComponent>(entity).IsActive)
    {
      return ProjectileHitAdmissionResult.ProjectileInactive;
    }

    // Immunity is checked before Combat admission. Once Combat returns an accepted receipt,
    // this method records that receipt and must not reject it after Life has already changed.
    AttributeSetComponent? attributes = _world.Has<AttributeSetComponent>(entity)
        ? _world.Get<AttributeSetComponent>(entity)
        : null;
    float effectiveImmunityTicksValue = attributes is null
        ? 0f
        : AttributeCalculator.Resolve(
            attributes,
            AttributeKind.ProjectileHitImmunityTicks);
    long effectiveImmunityTicks = ResolveTicks(
        effectiveImmunityTicksValue,
        nameof(effectiveImmunityTicksValue));
    if (_world.Has<ProjectileHitImmunityStateComponent>(entity))
    {
      ProjectileHitImmunityStateComponent immunity =
          _world.Get<ProjectileHitImmunityStateComponent>(entity);
      immunity.Record(
          targetHandle,
          tick,
          checked(tick + effectiveImmunityTicks));
    }

    if (_world.Has<ProjectilePenetrationStateComponent>(entity))
    {
      ref ProjectilePenetrationStateComponent penetration =
          ref _world.Get<ProjectilePenetrationStateComponent>(entity);
      // 回执已落地：把预扣转成已确认消耗。额度以生成时快照为准，
      // 不再每帧回读属性——中途改属性不该让已经飞出去的弹体换算法。
      penetration = penetration.ConfirmConsume();
      if (penetration.IsExhausted)
      {
        terminationReason = ProjectileTerminationReason.HitLimitReached;
        Terminations.Enqueue(
            projectileHandle,
            ProjectileTerminationReason.HitLimitReached,
            tick);
      }
    }

    return ProjectileHitAdmissionResult.Accepted;
  }

  private static long ResolveTicks(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f || value > long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return checked((long)MathF.Ceiling(value));
  }
}
