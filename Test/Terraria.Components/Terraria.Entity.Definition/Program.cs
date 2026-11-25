using System;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

// 实体定义框架的宿主验证（审计 §30 待决项 #6，用户选择 A：补宿主并收编）。
//
// 此前 8 个定义相关类型在目录外零引用：缺的不是消费者，而是 IEntityDefinitionHost 的实现，
// 因此 EntitySystem 连构造函数都调不通。本验证固定五件事：
//   1. 定义能被宿主实例化成真实实体，且声明过的组件全部存在；
//   2. 带 EntityIdentityComponent 的定义会自动登记运行时句柄，可被 resolver 解析；
//   3. 同一个定义两次生成得到互相独立的身份与句柄；
//   4. 销毁会同时移除句柄与实体；
//   5. 缺少 Required 组件时在 Build 阶段就被拒绝，不会拖到运行期。
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

EntityDefinition definition = new EntityBuilder(new EntityId("probe.entity"))
    .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
    .Add(() => new EntityFactionComponent(Faction.Hostile))
    .Add(() => new LifeComponent(current: 50, maximum: 50))
    .Add(() => new LocationComponent(new Vector2(3f, 4f)))
    .Require<EntityIdentityComponent>()
    .Require<LifeComponent>()
    .Build();

EntityRegistry definitions = new();
definitions.Register(definition);
ArchEntityDefinitionHost host = new(world, registry);
EntitySystem<ArchEntity> spawner = new(definitions, host);

EntityId id = new("probe.entity");
ArchEntity first = spawner.Spawn(id);

Check(
    "spawn-creates-entity",
    world.IsAlive(first),
    $"alive={world.IsAlive(first)}");

bool allComponents =
    world.Has<EntityIdentityComponent>(first) &&
    world.Has<EntityFactionComponent>(first) &&
    world.Has<LifeComponent>(first) &&
    world.Has<LocationComponent>(first);
Check("spawn-applies-declared-components", allComponents, "identity/faction/life/location");

bool handleRegistered =
    registry.TryGetHandle(first, out EntityRuntimeHandle firstHandle) &&
    registry.TryResolve(firstHandle, out ArchEntity resolved) &&
    resolved == first;
Check(
    "spawn-registers-runtime-handle",
    handleRegistered,
    $"handle={(handleRegistered ? firstHandle.ToString() : "none")}");

ArchEntity second = spawner.Spawn(id);
EntityUuid firstId = world.Get<EntityIdentityComponent>(first).Id;
EntityUuid secondId = world.Get<EntityIdentityComponent>(second).Id;
bool distinct =
    firstId != secondId && first != second &&
    registry.TryGetHandle(second, out EntityRuntimeHandle secondHandle) &&
    secondHandle != firstHandle;
Check(
    "spawn-yields-independent-instances",
    distinct,
    $"uuidEqual={firstId == secondId} handleEqual={(registry.TryGetHandle(second, out EntityRuntimeHandle h2) && h2 == firstHandle)}");

spawner.Despawn(first);
bool destroyed =
    !world.IsAlive(first) &&
    !registry.TryGetHandle(first, out _) &&
    registry.TryGetHandle(second, out _);
Check("despawn-removes-entity-and-handle", destroyed, "实体与句柄都已移除，另一个实例不受影响");

bool rejectedAtBuild = false;
try
{
  _ = new EntityBuilder(new EntityId("probe.broken"))
      .Add(() => new LifeComponent(current: 1, maximum: 1))
      .Require<EntityIdentityComponent>()
      .Build();
}
catch (InvalidOperationException)
{
  rejectedAtBuild = true;
}
Check(
    "missing-required-component-rejected-at-build",
    rejectedAtBuild,
    "Require<EntityIdentityComponent>() 未提供时 Build 抛 InvalidOperationException");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: entity definition host creates, registers, isolates and destroys.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;
