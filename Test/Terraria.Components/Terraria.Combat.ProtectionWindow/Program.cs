using System;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

// 无敌窗口链路的精简验证（type: chain）。
//
// 覆盖范围**刻意只取骨架**：边界两端、通道隔离、时长阶梯、攻击者分槽各一条。
// 计划 §1 的五条口径里「预检与裁决差额」与「NPC 全链路」留给后续批次，不在这里铺开。
//
// 判定口径：授予于 N、时长 D，受保护区间是 N..N+D-1，N+D 起放行——
// 与参考端一致（Player.cs 里 immuneTime 先递减再判定，tick N+D 递减到 0 即放行）。

int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

using ArchWorld world = ArchWorld.Create();
EntityRuntimeRegistry registry = new(worldScope: 1);

ArchEntity MakeActor(
    Faction faction,
    int life,
    bool withAttacker,
    out EntityRuntimeHandle handle)
{
  EntityUuid id = EntityUuid.Create();
  handle = registry.Allocate(id);
  ArchEntity entity = withAttacker
      ? world.Create(
          new EntityIdentityComponent(id),
          new EntityFactionComponent(faction),
          new LifeComponent(current: life, maximum: life),
          new AttributeSetComponent(),
          new LocationComponent(Vector2.Zero),
          new AttackerImmunityComponent())
      : world.Create(
          new EntityIdentityComponent(id),
          new EntityFactionComponent(faction),
          new LifeComponent(current: life, maximum: life),
          new AttributeSetComponent(),
          new LocationComponent(Vector2.Zero),
          new ImmunityWindowComponent());
  registry.Register(handle, entity);
  return entity;
}

ArchEntity sourceAEntity = MakeActor(Faction.Friendly, 100, withAttacker: false, out EntityRuntimeHandle sourceA);
ArchEntity sourceBEntity = MakeActor(Faction.Friendly, 100, withAttacker: false, out EntityRuntimeHandle sourceB);
ArchEntity targetEntity = MakeActor(Faction.Hostile, 100, withAttacker: false, out EntityRuntimeHandle target);
ArchEntity npcEntity = MakeActor(Faction.Hostile, 100, withAttacker: true, out EntityRuntimeHandle npcSlot);

CombatDamageSystem combat = new(world, registry);
ImmunityWindowExpirySystem expiry = new(world);

DamageProcessResult Hit(
    EntityRuntimeHandle source,
    EntityRuntimeHandle victim,
    long tick,
    long requestId,
    int damage = 10,
    ImmunityChannel channel = ImmunityChannel.General)
{
  registry.TryResolve(victim, out ArchEntity victimEntity);
  return combat.ProcessOne(new DamageRequest(
      source,
      victim,
      new DamagePayload(baseDamage: damage),
      world.Get<LifeComponent>(victimEntity),
      tick,
      DamageCause.PlayerAttack,
      requestId: requestId,
      channel: channel), tick);
}

// --- 1. 首次命中落地并开窗；窗口覆盖授予 tick 本身 ---
ImmunityWindowComponent window = world.Get<ImmunityWindowComponent>(targetEntity);
DamageProcessResult first = Hit(sourceA, target, 1, 1);
int granted = (int)(window.Until(ImmunityChannel.General) - 1);
Check(
    "hit-applies-and-opens-window",
    first.Applied && world.Get<LifeComponent>(targetEntity).Current < 100 &&
        window.IsActive(ImmunityChannel.General, 1) && granted > 0,
    $"applied={first.Applied} life={world.Get<LifeComponent>(targetEntity).Current} " +
        $"duration={granted}");

// --- 2. 边界两端：N+D-1 仍受保护，N+D 起放行 ---
long lastProtected = window.Until(ImmunityChannel.General) - 1;
bool tailProtected = window.IsActive(ImmunityChannel.General, lastProtected);
bool tailReleased = !window.IsActive(ImmunityChannel.General, lastProtected + 1);
DamageProcessResult atTail = Hit(sourceA, target, lastProtected, 2);
DamageProcessResult pastTail = Hit(sourceA, target, lastProtected + 1, 3);
Check(
    "window-boundary-is-exactly-duration-ticks",
    tailProtected && tailReleased && !atTail.Applied && pastTail.Applied,
    $"lastProtected={lastProtected} blockedAtTail={atTail.RejectionReason} " +
        $"appliedPastTail={pastTail.Applied}");

// --- 3. tick 0 的「未激活」哨兵不能被判成激活 ---
ArchEntity probeEntity = MakeActor(Faction.Hostile, 50, withAttacker: false, out _);
bool tickZeroActive = world.Get<ImmunityWindowComponent>(probeEntity)
    .IsActive(ImmunityChannel.General, 0);
Check("never-granted-window-is-not-active-at-tick-zero", !tickZeroActive, $"active={tickZeroActive}");

// --- 4. 通道隔离：General 窗口挡不住 Lava 通道的命中 ---
MakeActor(Faction.Hostile, 100, withAttacker: false, out EntityRuntimeHandle lavaTarget);
DamageProcessResult generalHit = Hit(sourceA, lavaTarget, 10, 4);
DamageProcessResult lavaHit = Hit(sourceA, lavaTarget, 10, 5, channel: ImmunityChannel.Lava);
DamageProcessResult generalAgain = Hit(sourceA, lavaTarget, 10, 6);
Check(
    "channel-isolation-lava-vs-general",
    generalHit.Applied && lavaHit.Applied && !generalAgain.Applied,
    $"general={generalHit.Applied} lava={lavaHit.Applied} generalAgain={generalAgain.RejectionReason}");

// --- 5. 时长阶梯由实际伤害反推，不是宿主常量 ---
ArchEntity chipEntity = MakeActor(Faction.Hostile, 100, withAttacker: false, out EntityRuntimeHandle chipTarget);
DamageProcessResult chipHit = Hit(sourceA, chipTarget, 20, 7, damage: 1);
long chipTicks = world.Get<ImmunityWindowComponent>(chipEntity)
    .Until(ImmunityChannel.General) - 20;
Check(
    "duration-ladder-chip-damage-is-shorter",
    chipHit.Applied && chipTicks == ImmunityDurationPolicy.ChipDamage &&
        chipTicks < granted,
    $"accepted={chipHit.Receipt?.AcceptedDamage} ticks={chipTicks} normal={granted}");

// --- 6. NPC 侧按攻击者分槽：A 的窗口不影响 B ---
DamageProcessResult npcFirst = Hit(sourceA, npcSlot, 30, 8);
DamageProcessResult npcAgainByA = Hit(sourceA, npcSlot, 31, 9);
DamageProcessResult npcByB = Hit(sourceB, npcSlot, 31, 10);
Check(
    "npc-attacker-scoped-immunity",
    npcFirst.Applied && !npcAgainByA.Applied && npcByB.Applied,
    $"first={npcFirst.Applied} sameAttacker={npcAgainByA.RejectionReason} otherAttacker={npcByB.Applied}");

// --- 7. 持续伤害不吃受击窗口：被点燃不等于免伤 ---
// 参考端灼烧走 hurtCooldowns[Lava]，前面的 `!immune` 前置条件根本不存在（Player.cs:27887）。
// 受击窗口只该挡「一次性打击」，挡 DoT 会让「被点燃」变成「免疫后续所有打击」。
{
  MakeActor(Faction.Hostile, 100, withAttacker: false, out EntityRuntimeHandle burnTarget);
  registry.TryResolve(burnTarget, out ArchEntity burnEntity);
  DamageProcessResult openingHit = Hit(sourceA, burnTarget, 40, 11);
  long expectedTicks = ImmunityDurationPolicy.Normal;
  // 请求形状镜像生产点 StatusEffectPeriodicSystem:79-92：自伤 + Hostile 关系。
  DamageProcessResult burn = combat.ProcessOne(new DamageRequest(
      burnTarget,
      burnTarget,
      new DamagePayload(
          baseDamage: 2,
          type: DamageType.DamageOverTime,
          relation: DamageRelation.Hostile,
          allowSelfDamage: true),
      world.Get<LifeComponent>(burnEntity),
      40,
      DamageCause.Environmental,
      requestId: 12,
      effectSourceId: 7), 40);
  Check(
      "damage-over-time-is-not-blocked-by-hit-window",
      openingHit.Applied && burn.Applied,
      $"windowTicks={expectedTicks} burn={burn.RejectionReason}");
}

// --- 8. 归零系统真的在清理过期戳，而不是空转 ---
expiry.Update(new EcsServerTickContext(world, 100_000, 1f / 60f));
Check(
    "expiry-system-clears-stale-ticks",
    window.Until(ImmunityChannel.General) == 0,
    $"until={window.Until(ImmunityChannel.General)}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: immunity window grants, isolates channels, ladders duration and scopes per attacker.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;
