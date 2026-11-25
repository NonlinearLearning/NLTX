using System.Linq;
using System.Numerics;
using Arch.Core;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Ai;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Items;
using Terraria.Unshared.Npc;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

// Attributes 与 StatusEffect 边界的可执行探针。
//
// 固定四件事：
//   1. 属性键分类与准入规则（资源、冷却、身份、状态旗标不得作为普通贡献写入）；
//   2. 生命、护盾池、冷却、呼吸、坐骑身份各自独立于属性系统；
//   3. Regeneration / Poison / Invisibility 三条代表性效果的完整闭环；
//   4. 过期、刷新、来源撤销对属性修饰器与行为组件的结果一致。
int failures = 0;

void Check(string name, bool condition)
{
  Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {name}");
  if (!condition)
  {
    failures++;
  }
}

Check(
    "shield-is-resource-pool",
    AttributeKindMetadata.GetCategory(AttributeKind.Shield) == AttributeKindCategory.ResourcePool);
Check(
    "defense-is-computed-scalar",
    AttributeKindMetadata.GetCategory(AttributeKind.Defense) == AttributeKindCategory.ComputedScalar);
Check(
    "cooldown-key-is-not-a-computed-scalar",
    AttributeKindMetadata.GetCategory(AttributeKind.PotionCooldownTicks) ==
        AttributeKindCategory.Cooldown);
Check(
    "mount-key-is-identity",
    AttributeKindMetadata.GetCategory(AttributeKind.MountKind) == AttributeKindCategory.Identity);
Check(
    "status-flag-key-is-status-flag",
    AttributeKindMetadata.GetCategory(AttributeKind.StatusFlag) == AttributeKindCategory.StatusFlag);

AttributeSetComponent attributes = new();
attributes.SetBaseValue(new AttributeBaseValue(AttributeKind.Defense, 10f));
AttributeContributionSystem.Apply(
    attributes,
    new AttributeContributionSet(
        new AttributeSourceDescriptor(11, AttributeSourceCategory.Equipment),
        [new AttributeContribution(AttributeKind.Defense, AttributeModifierOperation.Additive, 2f)]));
Check("computed-value-and-source-revoke", AttributeCalculator.Resolve(attributes, AttributeKind.Defense) == 12f);
AttributeContributionSystem.Revoke(attributes, 11);
Check("source-revokes-without-touching-base", AttributeCalculator.Resolve(attributes, AttributeKind.Defense) == 10f);

bool rejectedResourceContribution = false;
try
{
  _ = new AttributeContribution(AttributeKind.Shield, AttributeModifierOperation.Additive, 1f);
}
catch (ArgumentException)
{
  rejectedResourceContribution = true;
}

Check("resource-cannot-enter-ordinary-contribution", rejectedResourceContribution);

bool rejectedCooldownBaseValue = false;
try
{
  attributes.SetBaseValue(new AttributeBaseValue(AttributeKind.PotionCooldownTicks, 5f));
}
catch (ArgumentException)
{
  rejectedCooldownBaseValue = true;
}

Check("cooldown-cannot-become-a-base-value", rejectedCooldownBaseValue);

bool rejectedMountModifier = false;
try
{
  _ = new AttributeModifier(
      AttributeKind.MountKind,
      AttributeSourceCategory.Mount,
      AttributeModifierOperation.Override,
      3f,
      sourceId: 12);
}
catch (ArgumentException)
{
  rejectedMountModifier = true;
}

Check("mount-identity-cannot-become-a-modifier", rejectedMountModifier);

LifeComponent life = new(current: 20, maximum: 20);
attributes.SetBaseValue(new AttributeBaseValue(AttributeKind.LifeRegeneration, 4f));
Check("life-remains-independent", life.Current == 20 && life.Maximum == 20);

ShieldPoolComponent shield = new(5);
Check("shield-pool-consume-is-independent", AttributeSystem.ConsumeShield(shield, 3) == 3 &&
    shield.Current == 2);

using (ArchWorld syncWorld = ArchWorld.Create())
{
  AttributeSetComponent syncedAttributes = new();
  syncedAttributes.SetBaseValue(new AttributeBaseValue(AttributeKind.MaximumLife, 10f));
  Arch.Core.Entity syncedEntity = syncWorld.Create(
      new LifeComponent(current: 20, maximum: 20),
      syncedAttributes,
      new ShieldPoolComponent(4));
  new LifeAttributeSynchronizationSystem(syncWorld)
      .Update(new EcsServerTickContext(syncWorld, 0, 1f / 60f));
  LifeComponent syncedLife = syncWorld.Get<LifeComponent>(syncedEntity);
  Check("maximum-life-sync-clamps-current", syncedLife.Maximum == 10 && syncedLife.Current == 10);
}

CooldownComponent cooldowns = new();
cooldowns.Start(CooldownKind.Potion, 2);
cooldowns.AdvanceOneTick();
Check("cooldown-is-independent-component", !cooldowns.IsReady(CooldownKind.Potion));
cooldowns.AdvanceOneTick();
Check("cooldown-expires-in-owner", cooldowns.IsReady(CooldownKind.Potion));

Check("regeneration-declares-periodic-heal",
    BuffContent.Regeneration.PeriodicEffects.Count == 1 &&
    BuffContent.Regeneration.PeriodicEffects[0].Kind == BuffBehaviorKind.PeriodicHeal);
Check("poison-declares-periodic-damage",
    BuffContent.Poison.PeriodicEffects.Count == 1 &&
    BuffContent.Poison.PeriodicEffects[0].Kind == BuffBehaviorKind.DamageOverTime);
Check("invisibility-is-concrete-behavior",
    BuffContent.Invisibility.Behaviors.Count == 1 &&
    BuffContent.Invisibility.Behaviors[0].Kind == BuffBehaviorKind.Invisibility);

// ---- B6/B7 生命周期与闭环：真实系统、真实 tick 顺序 ----
// 顺序与 Program.BuildRegistrations 一致：BuffApply → BuffTick → Behavior → Periodic，
// 再由 Commit 阶段的 LifeRestoreRequest / CombatDamageRequest 消费请求。
BuffRegistry definitions = new();
BuffContent.RegisterAll(definitions);
BuffSystem buffSystem = new(definitions);

using (ArchWorld world = ArchWorld.Create())
{
  world.Create(new LifeRestoreRequestQueueComponent());
  world.Create(new CombatDamageRequestQueueComponent());
  world.Create(new CombatDamageResultQueueComponent());

  EntityRuntimeRegistry entities = new(worldScope: 1);
  EntityRuntimeHandle playerHandle = entities.Allocate(EntityUuid.Create());
  AttributeSetComponent playerAttributes = new();
  PlayerAttributeBaseline.Apply(playerAttributes, maximumLife: 100);
  ArchEntity player = world.Create(
      new EntityFactionComponent(Faction.Friendly),
      new LifeComponent(current: 50, maximum: 100),
      playerAttributes,
      new ShieldPoolComponent(),
      new BuffListComponent(),
      new BuffReceptionCapabilityComponent(BuffReceptionMode.AllowAll),
      new InventoryComponent(
          [new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 9999)]),
      new LocationComponent(Vector2.Zero));
  entities.Register(playerHandle, player);

  BuffApplyRequestSystem buffApply = new(world, buffSystem);
  BuffTickSystem buffTick = new(world, buffSystem);
  StatusEffectBehaviorSystem behavior = new(world, definitions);
  StatusEffectPeriodicSystem periodic = new(world, entities);
  LifeRestoreRequestSystem lifeRestore = new(world, entities);
  CombatDamageRequestSystem combatDamage = new(world, entities);

  void Tick(long tick)
  {
    EcsServerTickContext context = new(world, tick, 1f / 60f);
    buffApply.Update(context);
    buffTick.Update(context);
    behavior.Update(context);
    periodic.Update(context);
    lifeRestore.Update(context);
    combatDamage.Update(context);
  }

  // Regeneration：应用 → 行为组件 → 周期请求 → Life 提交。
  world.Add(player, new BuffApplyRequestComponent(
      BuffContent.Regeneration.Effect, sourceId: 9001, durationTicks: 600));
  Tick(1);
  Check("regeneration-applies-attribute-modifier",
      AttributeCalculator.Resolve(playerAttributes, AttributeKind.LifeRegeneration) == 4f);
  Check("regeneration-materializes-life-regeneration",
      world.Has<LifeRegenerationComponent>(player) &&
      world.Get<LifeRegenerationComponent>(player).TryGet(9001, out _));
  Check("regeneration-request-not-due-yet", world.Get<LifeComponent>(player).Current == 50);

  for (long tick = 2; tick <= 61; tick++)
  {
    Tick(tick);
  }

  Check("regeneration-closes-through-life-restore",
      world.Get<LifeComponent>(player).Current == 54 &&
      world.Get<LifeRestoreRequestQueueComponent>(
          FindSingleton<LifeRestoreRequestQueueComponent>(world)).Count == 0);

  // Poison：应用 → DamageOverTime → Combat 请求 → 生命提交 + 结果保留来源与伤害类型。
  world.Add(player, new BuffApplyRequestComponent(
      BuffContent.Poison.Effect, sourceId: 9002, durationTicks: 600));
  Tick(62);
  Check("poison-materializes-damage-over-time",
      world.Has<DamageOverTimeComponent>(player) &&
      world.Get<DamageOverTimeComponent>(player).TryGet(9002, out _));
  Check("poison-does-not-become-status-flag",
      !playerAttributes.Modifiers.Any(modifier => modifier.Attribute == AttributeKind.StatusFlag));

  for (long tick = 63; tick <= 121; tick++)
  {
    Tick(tick);
  }

  // 再生仍在生效（每 60 tick 回血一次），因此只断言毒伤在本 tick 造成的净变化。
  int beforePoisonTick = world.Get<LifeComponent>(player).Current;
  Tick(122);

  CombatDamageResultQueueComponent results =
      world.Get<CombatDamageResultQueueComponent>(
          FindSingleton<CombatDamageResultQueueComponent>(world));
  IReadOnlyList<CombatDamageResult> consumed = results.ConsumeResults(playerHandle, 9002);
  Check("poison-closes-through-combat-request",
      world.Get<LifeComponent>(player).Current == beforePoisonTick - 4 &&
      results.Count == 0 &&
      consumed.Count == 1);
  Check("poison-request-keeps-source-and-type",
      consumed.Count == 1 &&
      consumed[0].Request.Payload.Type == DamageType.DamageOverTime &&
      consumed[0].Request.EffectSourceId == 9002);

  // Invisibility：应用 → 具体行为组件 → AI 可见性投影可见 → 过期后行为组件移除。
  world.Add(player, new BuffApplyRequestComponent(
      BuffContent.Invisibility.Effect, sourceId: 9003, durationTicks: 600));
  Tick(123);
  Check("invisibility-materializes-behavior-component",
      world.Has<InvisibilityComponent>(player) &&
      world.Get<InvisibilityComponent>(player).IsActive);

  ArchEntity slime = world.Create(
      new EntityFactionComponent(Faction.Hostile),
      new LifeComponent(current: 25, maximum: 25),
      new LocationComponent(new Vector2(10f, 0f)));
  NpcAiSnapshotProvider snapshots = new(world);
  AiSnapshot slimeSnapshot = snapshots.Capture(slime, [typeof(NpcAiTargetFact)]);
  NpcAiTargetFact targetFact = slimeSnapshot.Get<NpcAiTargetFact>();
  Check("invisibility-visible-to-npc-projection",
      !targetFact.IsVisible && !targetFact.IsUsable && targetFact.IsAlive);

  // 到期：只能由 BuffTickSystem 推进，行为组件与属性修饰器同 tick 撤销。
  for (long tick = 124; tick <= 723; tick++)
  {
    Tick(tick);
  }

  Check("expiry-removes-behavior-component", !world.Has<InvisibilityComponent>(player));
  Check("expiry-removes-no-leftover-modifier",
      playerAttributes.Modifiers.Count == 0 &&
      !world.Has<DamageOverTimeComponent>(player) &&
      !world.Has<LifeRegenerationComponent>(player));

  // 刷新/叠层不重复：同一来源第二次应用只保留一个修饰器，最大值受定义约束。
  world.Add(player, new BuffApplyRequestComponent(BuffContent.Ironskin.Effect, sourceId: 9004));
  Tick(724);
  world.Add(player, new BuffApplyRequestComponent(BuffContent.Ironskin.Effect, sourceId: 9004));
  Tick(725);
  Check("refresh-replaces-source-contribution",
      playerAttributes.Modifiers.Count(modifier => modifier.SourceId == 9004) == 1 &&
      AttributeCalculator.Resolve(playerAttributes, AttributeKind.Defense) == 8f);

  // 两个来源独立撤销：撤销一个来源不影响另一个。
  world.Add(player, new BuffApplyRequestComponent(BuffContent.Ironskin.Effect, sourceId: 9005));
  Tick(726);
  AttributeContributionSystem.Revoke(playerAttributes, 9004);
  Check("source-revoke-is-isolated",
      AttributeCalculator.Resolve(playerAttributes, AttributeKind.Defense) == 8f);
  AttributeContributionSystem.Revoke(playerAttributes, 9005);
  Check("second-source-revoke-restores-base",
      AttributeCalculator.Resolve(playerAttributes, AttributeKind.Defense) == 0f);
}

// B4/B5：资源、冷却、身份与容量同步保持独立 owner。
using (ArchWorld capacityWorld = ArchWorld.Create())
{
  AttributeSetComponent capacityAttributes = new();
  capacityAttributes.SetBaseValue(new AttributeBaseValue(AttributeKind.MaxShield, 20f));
  ArchEntity wearer = capacityWorld.Create(
      new LifeComponent(current: 30, maximum: 30),
      capacityAttributes,
      new ShieldPoolComponent(15));
  LifeAttributeSynchronizationSystem synchronization = new(capacityWorld);
  synchronization.Update(new EcsServerTickContext(capacityWorld, 0, 1f / 60f));
  Check("shield-capacity-does-not-refill-pool",
      capacityWorld.Get<ShieldPoolComponent>(wearer).Current == 15);

  AttributeSystem.SetBaseValue(capacityAttributes, AttributeKind.MaxShield, 5f);
  synchronization.Update(new EcsServerTickContext(capacityWorld, 1, 1f / 60f));
  Check("shield-capacity-shrink-clamps-current",
      capacityWorld.Get<ShieldPoolComponent>(wearer).Current == 5 &&
      AttributeCalculator.Resolve(capacityAttributes, AttributeKind.MaxShield) == 5f);

  BreathComponent breath = new(maximum: 200);
  _ = breath.Consume(80);
  breath.Recover(30);
  Check("breath-tracks-current-and-maximum",
      breath.Current == 150 && breath.Maximum == 200 && !breath.IsDepleted);

  MountStateComponent mount = new();
  mount.Mount(3);
  bool mounted = mount.IsMounted && mount.MountKind == 3;
  mount.Dismount();
  Check("mount-identity-is-its-own-component",
      mounted && !mount.IsMounted && mount.MountKind == -1);
}

Console.WriteLine();
Console.WriteLine(
    failures == 0
        ? "PASS: attribute classification, resource owners and three status-effect closures hold."
        : $"FAIL: {failures} check(s) failed.");
return failures == 0 ? 0 : 1;

static ArchEntity FindSingleton<T>(ArchWorld world)
{
  ArchEntity found = default;
  world.Query(new QueryDescription().WithAll<T>(), (ArchEntity entity) => found = entity);
  return found;
}
