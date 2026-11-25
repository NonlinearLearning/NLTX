# 缺陷：AI 行为工厂按类型注册，profile 上的数值对多 profile 内容不生效

```text
文档 ID：DEF-2026-11-24-projectile-homing-profile-values-not-applied
逻辑域：projectile
产物类型：defect-record
状态：open（严重度：中；当前因数值全为 0 而未触发）
发现于：CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam §5.1
证据入口：§2 代码证据、§4 根因、§7 闭合口径
canonical 路径：docs/defects/2026-11-24-projectile-homing-profile-values-not-applied.md
```

## 1. 结论

`AiSystem<TIntent>` 的行为工厂**按行为类型**注册（`Dictionary<Type, Func<...>>`），
而 `ProjectileAiRegistration.CreateSystem` 只接受**一个** `homingProfile` 并把它闭包进工厂。
于是任何 `ProfileId` 不同的追踪内容，拿到的仍是 `homing.default` 的 profile 实例；
适配器虽然查到了自己的 profile，却只用了它的 `BehaviorType`，数值与 requirements 全被丢弃。

当前不报错，**只因为这些 profile 的数值都被故意留成 0**（见 §2）。一旦有人给某个 profile
填上非 0 的 `turnRate`/`range`/`maximumSpeed`，所有追踪内容会一起被改，而且没有任何报错。

## 2. 现状与证据

行为侧优先用 profile 数值，profile 为 0 才落到能力组件：

```csharp
// src/NSSLC/Projectile/System/ProjectileHomingAiBehavior.cs:48-74
float rangeSquared = _profile.Range > 0f
    ? _profile.Range * _profile.Range
    : capability.Range * capability.Range;
...
float maximumSpeed = _profile.MaximumSpeed > 0f
    ? _profile.MaximumSpeed
    : velocity.LinearVelocity.Length();
float turnRate = _profile.TurnRate > 0f ? _profile.TurnRate : capability.TurnRate;
```

内容侧把三个数值全留 0，并写明理由：

```csharp
// src/NSSLC.Server/Program.cs:548-575
/// profile 里的 <c>turnRate</c> / <c>range</c> / <c>maximumSpeed</c> 故意留 0：0 表示"用能力组件上的
/// 内容值"。这样同一套行为可以服务多条内容条目，不必为每个数值新建 profile。
...
catalog.Register(new ProjectileBehaviorProfile(
    new AiBehaviorId("HomingProjectile"),
    new AiBehaviorProfileId("homing.default"),
    typeof(ProjectileHomingAiBehavior),
    ...,
    maximumSpeed: 0f, turnRate: 0f, range: 0f));
```

注册侧只固化一个实例：

```csharp
// src/NSSLC/Projectile/System/ProjectileAiRegistration.cs:11-30
public static AiSystem<ProjectileAiIntent> CreateSystem(
    ProjectileBehaviorCatalog catalog,
    ProjectileBehaviorProfile homingProfile)
{
  ...
  system.Register(() => new ProjectileHomingAiBehavior(homingProfile));   // ← 闭包固定为单个 profile
  return system;
}
```

`AiSystem` 的注册键是类型且拒绝重复：

```csharp
// src/NSSLC/Share/AI/AiSystem.cs
private readonly Dictionary<Type, Func<AiBehavior<TIntent>>> _factories = new();
public void Register<TBehavior>(Func<TBehavior> factory) ...
{
  if (!_factories.TryAdd(typeof(TBehavior), () => factory())) {
    throw new InvalidOperationException($"AI behavior {typeof(TBehavior).FullName} is already registered.");
  }
}
```

适配器查到了 profile，但只取类型：

```csharp
// src/NSSLC/Projectile/ProjectileAiAdapter.cs（Update 循环内）
if (!_catalog.TryGet(behavior.BehaviorId, behavior.ProfileId, out ProjectileBehaviorProfile profile))
{
  return;
}
...
_pendingAttachments.Add((entity, AiComponent.For(profile.BehaviorType)));   // ← 只有类型被用上
```

调用侧固化：

```csharp
// src/NSSLC.Server/Program.cs:692-697
ProjectileAiRegistration.CreateSystem(
    projectileBehaviors,
    projectileBehaviors.Get(new AiBehaviorId("HomingProjectile"),
                            new AiBehaviorProfileId("homing.default")))
```

`ProjectileAiRegistration.Bind` 虽然接受 `Func<ProjectileBehaviorProfile, AiBehavior>` 工厂，
但**全仓没有调用点**，是条死代码路径——它本来正是补这个洞的接口。

## 3. 触发条件（为什么现在是"潜伏"）

新增一条追踪内容，其 `ai` 声明 `profileId = "homing.slow"`，并在
`CreateProjectileBehaviorProfiles` 里注册该 profile 且 `turnRate = 5f`：

- `ProjectileBehaviorCatalog.Get("HomingProjectile", "homing.slow")` 校验通过；
- 绑定阶段仍走 `homing.default` 的工厂 → `ProjectileHomingAiBehavior(homing.default)`，
  `_profile.TurnRate == 0` → 回落到 `capability.TurnRate`；
- 结论：**`homing.slow` 的 5f 完全不生效**，且没有任何异常或日志。

反向更糟：把 `homing.default` 的 `turnRate` 填成 5f，所有追踪内容（包括声明了
别的 profile 的）都被强制成 5f，内容侧以为自己在用能力组件。

## 4. 根因

两层键不一致：

- **查找层**（catalog）的键是 `(BehaviorId, ProfileId)`；
- **工厂层**（AiSystem）的键只有 `Type`。

`CreateSystem` 只能提供一个实例，于是 profile 维度在"实例化"这一步被压平。
补齐缺口所需的接口（按 profile 构造）已经存在（`Bind` 的 factory 参数、`catalog.Get`），
但注册路径没有用它，形成"接口在、接线没接"的半成品。

## 5. 期望行为

不同 `ProfileId` 的同类行为各自持有自己的 profile：数值与 requirements 都来自
`catalog.Get(BehaviorId, ProfileId)`，互不串味。

## 6. 影响面

- 内容层无法为不同追踪弹配置不同转向率/射程/上限，除非改代码；
- 一旦填非 0 值就是**全局污染**，且静默；
- `ProjectileAiRegistration.Bind` 是死代码，读者会以为 profile 已按需生效（误导性更强于缺功能）；
- 与已记录的"追踪速率漂移"叠加时难以归因：调参看不出效果，会误判成物理层问题。

## 7. 闭合口径

二选一：

- (a) 把 `AiSystem` 的工厂键从 `Type` 扩展为 `(Type, ProfileId)`
  （或在 `ProjectileAiAdapter` 改用 `ProjectileAiRegistration.Bind` 的按 profile 工厂路径），
  使绑定期构造出对应 profile 的实例；
- (b) 明确"一个行为类型只有一个 profile"，则**删除** `ProjectileBehaviorProfile` 上的
  `turnRate/range/maximumSpeed` 字段与 `AiBehaviorProfileId`，让能力组件成为唯一数值来源。
  ——这条更符合当前实际用法（数值全 0），成本也最低。

两条路都必须新增断言：注册两个 `ProfileId` 不同、数值不同的追踪 profile，
断言两个实例各自读到自己的数值（或断言该区分已被显式移除）。当前倾向 (b)。

## 8. 涉及文件

- `src/NSSLC/Share/AI/AiSystem.cs`（工厂键为 `Type`）
- `src/NSSLC/Projectile/System/ProjectileAiRegistration.cs`（`CreateSystem` 单 profile 闭包；`Bind` 无调用点）
- `src/NSSLC/Projectile/ProjectileAiAdapter.cs`（只用 `profile.BehaviorType`）
- `src/NSSLC/Projectile/System/ProjectileBehaviorProfile.cs`
- `src/NSSLC.Server/Program.cs`（:548-586 目录、:692-697 固化调用）
- `Test/Terraria.Components/Terraria.Projectile/Program.cs`

## 9. 状态历史

- 2026-11-24：P2 收尾记录为"多个追踪内容共用一个 homing profile 工厂"；
  核对代码后确认为"工厂键缺 profile 维度 + 适配器丢弃 profile 数值 + Bind 为死代码"，
  严重度定为中（当前数值全 0 故未触发）。

关闭后本文件删除（按 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §3.3，缺陷不转归档）。
