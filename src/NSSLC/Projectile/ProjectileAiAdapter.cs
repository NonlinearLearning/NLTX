using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>把投射物 AI 的计算结果落成行为状态与一步移动意图。</summary>
/// <remarks>
/// 这个适配器不是"第二个 AI 引擎"：它只做三件事——取快照、交给绑定好的行为算、把结果写进
/// <see cref="ProjectileBehaviorStateComponent" /> 并投递移动意图。
/// 它<b>不写</b>位置、速度、生命或属性：那些分别归空间、战斗与属性域。
/// <para>
/// 为什么速度走 <see cref="MovementIntent" /> 而不是直接写
/// <see cref="Terraria.Share.Movement.VelocityComponent" />：空间协调系统是位移与速度的唯一写者，
/// 直接写会和它双写。原实现里投射物的意图恒为 <c>Stop</c>，AI 速度因此永远进不了空间管线
/// （缺陷 B5）；这里补上意图生产者，速度仍由协调系统按能力参数落地。
/// </para>
/// <para>
/// 意图的写权限不在本类手里：它只把 intent 投进
/// <see cref="MovementIntentRequestQueueComponent" />，由
/// <see cref="MovementIntentRequestSystem" /> 在空间求解前统一安装。投射物域因此与 NPC、
/// 玩家输入走同一条通道，<see cref="MovementIntent" /> 只有一个写者。
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
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      AiBehaviorSelectionComponent,
      ProjectileBehaviorStateComponent,
      ProjectileDispositionStateComponent>();

  public ProjectileAiAdapter(
      ArchWorld world,
      AiEntitySystem<ArchEntity, ProjectileAiIntent> ai,
      IAiSnapshotProvider<ArchEntity> snapshots,
      ProjectileBehaviorCatalog catalog,
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _ai = ai ?? throw new ArgumentNullException(nameof(ai));
    _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    float deltaTime = context.DeltaTime;
    // 队列在查询之外解析：Require 自身要跑一次查询，放进回调会形成嵌套查询。
    MovementIntentRequestQueueComponent movementIntents =
        MovementIntentRequestQueue.Require(World);

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
            // 生成期就该装配 AI 声明（D4）。缺了说明内容或生成路径有问题，
            // 查询里补结构是结构变更，不做——留给内容侧暴露。
            return;
          }

          if (!_ai.IsAttached(entity))
          {
            // Attach 只动内部字典，不是 ECS 结构变更，因此可以在查询回调里完成绑定，
            // 并在同一帧继续 Calculate——实体存在的第一个 tick 就产出速度意图（D4）。
            _ai.Attach(entity, aiComponent);
          }

          AiEntityResult<ArchEntity, ProjectileAiIntent> result = _ai.Calculate(
              entity,
              in aiComponent,
              _snapshots,
              deltaTime);
          IReadOnlyList<ProjectileAiIntent> intents = result.Result.Intents;
          if (intents.Count == 0)
          {
            // 兜底，不是设计：删掉 Stop 续命之后，空间系统按 intent.Tick 判过期，
            // 本帧一条速度意图都没有的弹体会原地不动。续当前速度让它继续飞，
            // 同时把"行为没产出意图"留给验证项目断言——那是内容缺陷。
            EmitFallback(entity, tick, movementIntents);
            return;
          }

          for (int index = 0; index < intents.Count; index++)
          {
            ProjectileAiIntent intent = intents[index];
            state = Apply(entity, intent, state, tick, movementIntents);
          }
        });

  }

  /// <summary>续一次当前速度：只在行为本帧没有给出速度意图时使用。</summary>
  private void EmitFallback(
      ArchEntity entity,
      long tick,
      MovementIntentRequestQueueComponent movementIntents)
  {
    if (!World.TryGet(entity, out VelocityComponent current) ||
        !_handles.TryGetHandle(entity, out EntityRuntimeHandle handle) || !handle.IsValid)
    {
      return;
    }

    movementIntents.Enqueue(new MovementIntentRequest(
        handle,
        new MovementIntent(
            current.LinearVelocity,
            tick,
            priority: 0,
            jumpRequested: false,
            hasDesiredVelocity: true,
            source: MovementIntentSource.Ai,
            sourceRevision: 0)));
  }

  /// <summary>
  /// 提交一次 AI 输出：状态推进一定发生，移动意图只在行为给出速度时投进共享队列。
  /// </summary>
  /// <remarks>
  /// 句柄解析不到时意图丢弃：那种弹体也不在空间管线里，写意图没有读者。
  /// </remarks>
  private ProjectileBehaviorStateComponent Apply(
      ArchEntity entity,
      in ProjectileAiIntent intent,
      ProjectileBehaviorStateComponent state,
      long tick,
      MovementIntentRequestQueueComponent movementIntents)
  {
    if (intent.Kind is ProjectileAiIntentKind.MaintainVelocity or ProjectileAiIntentKind.AdjustVelocity)
    {
      if (_handles.TryGetHandle(entity, out EntityRuntimeHandle handle) && handle.IsValid)
      {
        movementIntents.Enqueue(new MovementIntentRequest(
            handle,
            new MovementIntent(
                intent.DesiredVelocity,
                tick,
                priority: 0,
                jumpRequested: false,
                hasDesiredVelocity: true,
                source: MovementIntentSource.Ai,
                sourceRevision: intent.TargetRevision)));
      }
    }

    // 请求终止属于终止状态机（P5），这里只推进行为状态，不让适配器替生命周期做决定。
    return state.Advance(state.Phase);
  }
}
