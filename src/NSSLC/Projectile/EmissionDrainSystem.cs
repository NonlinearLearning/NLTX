using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Emission;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>把共享层的发射意图排空成投射物域自己的生成请求。</summary>
/// <remarks>
/// 这是 Projectile 域对 Emission 接缝的<b>消费者</b>，也是 Emission 与生成之间唯一的翻译层：
/// <c>EmissionRequest</c>（内容 id 文本、绝对坐标）在这里被解析成
/// <c>ProjectileSpawnRequest</c>（<see cref="ProjectileId"/>、归属、阵营）。
/// <para>
/// 为什么两级队列而不是合并：共享层不得依赖非共享领域类型，所以意图只能用 id 文本表达；
/// 解析、存在性校验、阵营推导都是投射物域的知识，留在这一侧。代价是同 tick 多一次拷贝，
/// 换来的是 Share 对 Unshared 的零反向依赖。
/// </para>
/// <para>
/// <b>队列按 tick 号排空</b>：本系统必须与 <see cref="EmissionQueueComponent"/> 的入队方
/// 处于同一相位（Preparation），且排在 <c>ProjectileSpawnRequestSystem</c> 之前——
/// 生成请求同样按 tick 号排空，跨相位会让请求永远取不到。
/// </para>
/// <para>
/// 失败不抛异常：tick 管线里抛异常会打断整帧，而「内容 id 拼错」属于内容问题，
/// 应在加载或验证期暴露。逐条去向记进 <see cref="Results"/>。
/// </para>
/// </remarks>
public sealed class EmissionDrainSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ProjectileCatalog _definitions;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly IEntityRuntimeResolver _resolver;
  private readonly List<EmissionDrainResult> _results = [];

  public EmissionDrainSystem(
      ArchWorld world,
      ProjectileCatalog definitions,
      IEntityRuntimeHandleResolver handles,
      IEntityRuntimeResolver resolver)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  /// <summary>The routing outcome of every intent drained by the last update.</summary>
  public IReadOnlyList<EmissionDrainResult> Results => _results;

  public override void Update(in EcsServerTickContext context)
  {
    _results.Clear();
    long tick = context.Tick;
    IReadOnlyList<EmissionRequest> requests = EmissionQueue.Require(World).Drain(tick);
    for (int index = 0; index < requests.Count; index++)
    {
      Drain(requests[index], tick);
    }
  }

  private void Drain(in EmissionRequest request, long tick)
  {
    ProjectileId definitionId = new(request.ContentId);
    if (!_definitions.TryGet(definitionId, out _))
    {
      Record(request, EmissionDrainStatus.DefinitionUnknown);
      return;
    }

    // 发射者必须是本世界注册过的实体：归属与阵营都从它推导，解析不到就不猜。
    if (!_resolver.TryResolve(request.Emitter, out ArchEntity emitter) ||
        !World.IsAlive(emitter) ||
        !_handles.TryGetHandle(emitter, out EntityRuntimeHandle emitterHandle))
    {
      Record(request, EmissionDrainStatus.EmitterUnavailable);
      return;
    }

    ProjectileSpawnRequests.Enqueue(
        World,
        new ProjectileSpawnRequest(
            definitionId,
            emitterHandle,
            new ProjectileSourceMetadataComponent(
                sourceKind: "emission",
                sourceId: request.ContentId),
            position: request.Origin,
            faction: ResolveFaction(emitter)),
        tick);
    Record(request, EmissionDrainStatus.Enqueued);
  }

  /// <summary>
  /// 阵营跟随发射者：玩家发射的弹体属于 Friendly，敌对 NPC 发射的属于 Hostile，
  /// 缺阵营组件的按 Neutral 处理，与共享 <see cref="Faction" /> 的默认值一致。
  /// </summary>
  private Faction ResolveFaction(ArchEntity emitter)
  {
    return World.Has<EntityFactionComponent>(emitter)
        ? World.Get<EntityFactionComponent>(emitter).Value
        : Faction.Neutral;
  }

  private void Record(in EmissionRequest request, EmissionDrainStatus status)
  {
    _results.Add(new EmissionDrainResult(request.ContentId, status));
  }
}
