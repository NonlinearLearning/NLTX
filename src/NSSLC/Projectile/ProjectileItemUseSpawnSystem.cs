using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Emission;
using Terraria.Share.Entities;
using Terraria.Share.Items;
using Terraria.Share.Player;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>
/// Turns applied item-use facts into emission intents.
/// </summary>
/// <remarks>
/// 这是 Projectile 域对 Player 域的<b>输入接缝</b>：Player 只发布"某玩家在某 tick 成功使用了
/// 某物品"，投射物域自己决定要不要发射、发射什么。它读共享目录
/// <see cref="ItemUseCatalog" />，不读玩家状态、不碰库存，也不写对方域的组件。
/// <para>
/// 本系统只<b>入队发射意图</b>，不产出生成请求：解析内容定义、推导阵营、构造生成请求
/// 都归 <see cref="EmissionDrainSystem" />。物品使用只需要知道"用这个内容、从这个位置发"，
/// 这正是 <see cref="EmissionRequest" /> 的三字段。
/// </para>
/// <para>
/// 队列按 tick 号排空，因此本系统必须与 <see cref="EmissionDrainSystem" /> 处于<b>同一相位</b>
/// （Preparation）并排在它之前；排空方再把请求交给生成接缝。宿主的接线见
/// <c>NSSLC.Server/Registration/CombatSystemRegistration.cs</c> 与 <c>Program.cs</c>。
/// </para>
/// </remarks>
public sealed class ProjectileItemUseSpawnSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly ItemUseCatalog _itemUses;
  private readonly List<ProjectileItemUseSpawnResult> _results = [];
  private PlayerItemUseFactQueueComponent? _facts;

  public ProjectileItemUseSpawnSystem(
      ArchWorld world,
      IEntityRuntimeResolver resolver,
      IEntityRuntimeHandleResolver handles,
      ItemUseCatalog itemUses)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _itemUses = itemUses ?? throw new ArgumentNullException(nameof(itemUses));
  }

  /// <summary>Gets the routing outcome of every fact drained by the last update.</summary>
  public IReadOnlyList<ProjectileItemUseSpawnResult> Results => _results;

  private PlayerItemUseFactQueueComponent Facts => _facts ??= PlayerItemUseFactQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    _results.Clear();
    IReadOnlyList<PlayerItemUseFact> facts = Facts.Drain();
    for (int index = 0; index < facts.Count; index++)
    {
      Apply(facts[index], context.Tick);
    }
  }

  private void Apply(in PlayerItemUseFact fact, long tick)
  {
    // 只有真正落地的使用才射击：被拒的请求只是"玩家按了一下"，没有内容语义。
    if (fact.Outcome != PlayerItemUseOutcomeKind.Applied)
    {
      return;
    }

    if (!_itemUses.TryGet(fact.ItemId, out ItemUse use))
    {
      Record(fact, ProjectileItemUseSpawnStatus.UseUnknown);
      return;
    }

    if (use.Shoot is not ItemShootProfile shoot)
    {
      Record(fact, ProjectileItemUseSpawnStatus.NoShootProfile);
      return;
    }

    if (!_resolver.TryResolve(fact.Player, out ArchEntity owner) ||
        !World.IsAlive(owner) ||
        !World.Has<LocationComponent>(owner) ||
        !_handles.TryGetHandle(owner, out EntityRuntimeHandle ownerHandle))
    {
      Record(fact, ProjectileItemUseSpawnStatus.OwnerUnavailable);
      return;
    }

    // 局部偏移在这里加上：发射意图只接受绝对世界坐标，不读发射者位置。
    Vector2 origin = World.Get<LocationComponent>(owner).Coordinates + shoot.Offset;
    EmissionQueue.Enqueue(
        World,
        new EmissionRequest(ownerHandle, shoot.ProjectileId, origin),
        tick);
    Record(fact, ProjectileItemUseSpawnStatus.Enqueued);
  }

  private void Record(in PlayerItemUseFact fact, ProjectileItemUseSpawnStatus status)
  {
    _results.Add(new ProjectileItemUseSpawnResult(fact.Player, fact.ItemId, status));
  }
}
