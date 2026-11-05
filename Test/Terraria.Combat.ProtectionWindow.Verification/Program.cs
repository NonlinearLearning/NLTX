using System;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Relationships;

// 受击后无敌窗口的端到端验证（审计 §26 待决项：ProtectionWindowComponent 从未被挂载）。
//
// 此前 ProtectionWindowSystem 已注册，但全仓没有任何实体带 ProtectionWindowComponent，
// 因此它每帧匹配 0 个实体，伤害裁决里的保护窗口检查也就永远不触发。
// 现在玩家创建时挂上该组件，并由 CombatDamageSystem 在伤害提交成功后开始计时。
//
// 本验证固定四件事：
//   1. 首次受击生效、生命下降，并开始计时；
//   2. 窗口内的第二次受击被拒绝，理由是 ProtectionWindowActive，生命不变；
//   3. ProtectionWindowSystem 真的推进了它（不再是空转系统），到期后 IsActive 为 false；
//   4. 到期后的受击重新生效。
const int ProtectionTicks = 3;

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

EntityUuid sourceId = EntityUuid.Create();
EntityUuid targetId = EntityUuid.Create();
EntityRuntimeHandle sourceHandle = registry.Allocate(sourceId);
EntityRuntimeHandle targetHandle = registry.Allocate(targetId);

ArchEntity source = world.Create(
    new EntityIdentityComponent(sourceId),
    new EntityFactionComponent(Faction.Friendly),
    new LifeComponent(current: 100, maximum: 100),
    new LocationComponent(new Vector2(0f, 0f)));
ArchEntity target = world.Create(
    new EntityIdentityComponent(targetId),
    new EntityFactionComponent(Faction.Hostile),
    new LifeComponent(current: 100, maximum: 100),
    new LocationComponent(new Vector2(10f, 0f)),
    new ProtectionWindowComponent());
registry.Register(sourceHandle, source);
registry.Register(targetHandle, target);

CombatDamageSystem combat = new(world, registry, protectionWindowTicks: ProtectionTicks);
ProtectionWindowSystem windowSystem = new(world);

DamageRequest Hit(long tick, long requestId) => new(
    sourceHandle,
    targetHandle,
    new DamagePayload(baseDamage: 10),
    world.Get<LifeComponent>(target),
    tick,
    DamageCause.PlayerAttack,
    requestId: requestId);

// --- 1. 首次受击生效并开始计时 ---
DamageProcessResult first = combat.ProcessOne(Hit(1, 1), 1);
ProtectionWindowComponent protection = world.Get<ProtectionWindowComponent>(target);
Check(
    "first-hit-applied",
    first.Applied && world.Get<LifeComponent>(target).Current == 90,
    $"applied={first.Applied} life={world.Get<LifeComponent>(target).Current}");

Check(
    "hit-starts-protection-window",
    protection.IsActive && protection.RemainingTicks == ProtectionTicks,
    $"active={protection.IsActive} remaining={protection.RemainingTicks}");

// --- 2. 窗口内的第二次受击被拒绝 ---
DamageProcessResult second = combat.ProcessOne(Hit(1, 2), 1);
Check(
    "second-hit-blocked-by-window",
    !second.Applied &&
        second.RejectionReason == DamageRejectionReason.ProtectionWindowActive &&
        world.Get<LifeComponent>(target).Current == 90,
    $"reason={second.RejectionReason} life={world.Get<LifeComponent>(target).Current}");

// --- 3. 推进系统真的在推进它 ---
for (long tick = 2; tick <= 1 + ProtectionTicks; tick++)
{
  windowSystem.Update(new EcsServerTickContext(world, tick, 1f / 60f));
}

Check(
    "window-system-advances-and-expires",
    !protection.IsActive && protection.RemainingTicks == 0,
    $"active={protection.IsActive} remaining={protection.RemainingTicks}");

// --- 4. 到期后受击重新生效 ---
DamageProcessResult third = combat.ProcessOne(Hit(1 + ProtectionTicks + 1, 3), 1 + ProtectionTicks + 1);
Check(
    "hit-after-expiry-applied",
    third.Applied && world.Get<LifeComponent>(target).Current == 80,
    $"applied={third.Applied} life={world.Get<LifeComponent>(target).Current}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: post-hit protection window is granted, blocks and expires.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;
