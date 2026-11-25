using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Items;
using Terraria.Share.Physics;
using Terraria.Share.Player;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>
/// Turns applied item-use facts into projectile spawn requests.
/// </summary>
/// <remarks>
/// 这是 Projectile 域对 Player 域的<b>输入接缝</b>：Player 只发布"某玩家在某 tick 成功使用了
/// 某物品"，投射物域自己决定要不要生成、生成什么。它读共享目录
/// （<see cref="ItemUseCatalog" />、<see cref="ProjectileCatalog" />），不读玩家状态、不碰库存，
/// 也不写对方域的组件。
/// <para>
/// 生成走 <see cref="ProjectileSpawnRequests" />：请求进入 world-scoped 队列，由
/// <see cref="ProjectileSpawnRequestSystem" /> 在<b>同一 tick 的 Preparation</b> 排空。
/// 因为队列按 tick 号排空，本系统必须排在排空方之前、且与它处于同一相位；宿主的接缝接线
/// 见 <c>NSSLC.Server/Program.cs</c> 的注册项。
/// </para>
/// </remarks>
public sealed class ProjectileItemUseSpawnSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeResolver _resolver;
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly ItemUseCatalog _itemUses;
  private readonly ProjectileCatalog _projectiles;
  private readonly List<ProjectileItemUseSpawnResult> _results = [];
  private PlayerItemUseFactQueueComponent? _facts;

  public ProjectileItemUseSpawnSystem(
      ArchWorld world,
      IEntityRuntimeResolver resolver,
      IEntityRuntimeHandleResolver handles,
      ItemUseCatalog itemUses,
      ProjectileCatalog projectiles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
    _itemUses = itemUses ?? throw new ArgumentNullException(nameof(itemUses));
    _projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
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

    ProjectileId definitionId = new(shoot.ProjectileId);
    if (!_projectiles.TryGet(definitionId, out ProjectileRecipe recipe) ||
        !recipe.TryGetSection(out ProjectileBasicsSection basics))
    {
      // 没有平铺族的内容连默认碰撞体与取整尺寸都没有，等于无法生成：按定义未知处理。
      Record(fact, ProjectileItemUseSpawnStatus.DefinitionUnknown);
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

    Vector2 origin = World.Get<LocationComponent>(owner).Coordinates + shoot.Offset;
    ProjectileSpawnRequests.Enqueue(
        World,
        new ProjectileSpawnRequest(
            definitionId,
            ownerHandle,
            new ProjectileSourceMetadataComponent(
                sourceKind: "item-use",
                sourceId: fact.ItemId.ToString(),
                sourceRevision: fact.Slot),
            position: origin,
            velocity: shoot.Velocity,
            // 取整尺寸的唯一推导点在 section 构造函数里，这里只读标量，不再二次取整。
            collider: new ColliderComponent(new Vector2(basics.ColliderWidth, basics.ColliderHeight)),
            size: new EntitySizeComponent(basics.PixelWidth, basics.PixelHeight),
            faction: ResolveFaction(owner)),
        tick);
    Record(fact, ProjectileItemUseSpawnStatus.Enqueued);
  }

  /// <summary>
  /// 阵营跟随持有者：玩家发射的弹体属于 Friendly，敌对 NPC 发射的属于 Hostile。
  /// 缺阵营组件的持有者按 Neutral 处理，与共享 <see cref="Faction" /> 的默认值一致。
  /// </summary>
  private Faction ResolveFaction(ArchEntity owner)
  {
    return World.Has<EntityFactionComponent>(owner)
        ? World.Get<EntityFactionComponent>(owner).Value
        : Faction.Neutral;
  }

  private void Record(in PlayerItemUseFact fact, ProjectileItemUseSpawnStatus status)
  {
    _results.Add(new ProjectileItemUseSpawnResult(fact.Player, fact.ItemId, status));
  }
}
