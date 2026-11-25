using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Entities;
using Terraria.Share.Movement;

namespace Terraria.Unshared.Projectiles;

/// <summary>把投射物 AI 的计算结果落成行为状态与一步移动意图。</summary>
/// <remarks>
/// 这个适配器不是"第二个 AI 引擎"：它只做三件事——取快照、交给绑定好的行为算、把结果写进
/// <see cref="ProjectileBehaviorStateComponent" /> 与 <see cref="MovementIntent" />。
/// 它<b>不写</b>位置、速度、生命或属性：那些分别归空间、战斗与属性域。
/// <para>
/// 为什么速度走 <see cref="MovementIntent" /> 而不是直接写
/// <see cref="Terraria.Share.Movement.VelocityComponent" />：空间协调系统是位移与速度的唯一写者，
/// 直接写会和它双写。原实现里投射物的意图恒为 <c>Stop</c>，AI 速度因此永远进不了空间管线
/// （缺陷 B5）；这里补上意图生产者，速度仍由协调系统按能力参数落地。
/// </para>
/// <para>
/// <see cref="AiComponent" /> 在首次看到弹体时补挂（结构变更统一放在查询之后执行）。
/// 生成期挂载需要内容侧提供"行为类型"，而项目符号域不希望为此把 AI 插件类型塞进内容定义里；
/// 让适配器补挂，AI 接入点就只有一个。
/// </para>
/// </remarks>
public sealed class ProjectileAiAdapter : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly AiEntitySystem<ArchEntity, ProjectileAiIntent> _ai;
  private readonly IAiSnapshotProvider<ArchEntity> _snapshots;
  private readonly ProjectileBehaviorCatalog _catalog;
  private readonly List<(ArchEntity Entity, AiComponent Component)> _pendingAttachments = [];
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      AiBehaviorSelectionComponent,
      ProjectileBehaviorStateComponent,
      ProjectileDispositionStateComponent>();

  public ProjectileAiAdapter(
      ArchWorld world,
      AiEntitySystem<ArchEntity, ProjectileAiIntent> ai,
      IAiSnapshotProvider<ArchEntity> snapshots,
      ProjectileBehaviorCatalog catalog)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _ai = ai ?? throw new ArgumentNullException(nameof(ai));
    _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  /// <summary>Gets how many entity bindings were created by the last update.</summary>
  public int AttachedLastUpdate { get; private set; }

  public override void Update(in EcsServerTickContext context)
  {
    AttachedLastUpdate = 0;
    _pendingAttachments.Clear();
    long tick = context.Tick;
    float deltaTime = context.DeltaTime;

    World.Query(
        in _query,
        (ArchEntity entity,
            ref AiBehaviorSelectionComponent behavior,
            ref ProjectileBehaviorStateComponent state,
            ref ProjectileDispositionStateComponent disposition) =>
        {
          if (!disposition.IsActive)
          {
            return;
          }

          if (!_catalog.TryGet(behavior.BehaviorId, behavior.ProfileId, out ProjectileBehaviorProfile profile))
          {
            // 内容里的行为 id / profile id 无法解析：不猜类型，交给内容侧修正。
            return;
          }

          if (!World.TryGet(entity, out AiComponent aiComponent))
          {
            _pendingAttachments.Add((entity, AiComponent.For(profile.BehaviorType)));
            return;
          }

          if (!_ai.IsAttached(entity))
          {
            _pendingAttachments.Add((entity, aiComponent));
            return;
          }

          AiEntityResult<ArchEntity, ProjectileAiIntent> result = _ai.Calculate(
              entity,
              in aiComponent,
              _snapshots,
              deltaTime);
          IReadOnlyList<ProjectileAiIntent> intents = result.Result.Intents;
          if (intents.Count == 0)
          {
            return;
          }

          for (int index = 0; index < intents.Count; index++)
          {
            ProjectileAiIntent intent = intents[index];
            state = Apply(entity, intent, state, tick);
          }
        });

    for (int index = 0; index < _pendingAttachments.Count; index++)
    {
      (ArchEntity entity, AiComponent component) = _pendingAttachments[index];
      if (!World.IsAlive(entity))
      {
        continue;
      }

      if (!World.Has<AiComponent>(entity))
      {
        World.Add(entity, component);
      }

      _ai.Attach(entity, component);
      AttachedLastUpdate++;
    }

    _pendingAttachments.Clear();
  }

  /// <summary>
  /// 提交一次 AI 输出：状态推进一定发生，移动意图只在行为给出速度时写。
  /// </summary>
  private ProjectileBehaviorStateComponent Apply(
      ArchEntity entity,
      in ProjectileAiIntent intent,
      ProjectileBehaviorStateComponent state,
      long tick)
  {
    if (intent.Kind is ProjectileAiIntentKind.MaintainVelocity or ProjectileAiIntentKind.AdjustVelocity)
    {
      World.Get<MovementIntent>(entity) = new MovementIntent(
          intent.DesiredVelocity,
          tick,
          priority: 0,
          jumpRequested: false,
          hasDesiredVelocity: true,
          source: MovementIntentSource.Ai,
          sourceRevision: intent.TargetRevision);
    }

    // 请求终止属于终止状态机（P5），这里只推进行为状态，不让适配器替生命周期做决定。
    return state.Advance(state.Phase);
  }
}
