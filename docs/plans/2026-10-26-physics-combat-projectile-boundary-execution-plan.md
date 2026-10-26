# Physics、Combat 与 Projectile 边界收敛执行计划

文档 ID：DOC-2026-10-26-PHYSICS-COMBAT-PROJECTILE-BOUNDARY-PLAN  
需求性质：跨领域 ECS 边界和一次性事实模型变更  
执行方略：`spec`  
状态：`in_progress`  
日期：2026-10-26  
范围：`src/NSSLC` 及其本地验证项目  
明确排除：网络、客户端表现、`src/NSSLC.Infrastructure/`、存档协议、最新客户端代码迁移

## 1. 需求与目标

本计划处理前一轮审计确认的同一类设计问题：把一次性事实、攻击载荷、目标快照和领域结果
长期附着到实体，或者让一个系统同时拥有 Physics、Combat 和生命周期的权威写入责任。
本轮先处理四项相互依赖的边界：

1. 去掉 Physics 对 Combat 状态的硬依赖；
2. 让所有实体接触都使用同一种带 Tick/空间版本的 Physics 接触事实；
3. 让 Projectile 的创建、请求终止和实际销毁经过唯一的生命周期提交边界；
4. 让 Player、NPC、Projectile 的攻击请求可以在同一 Tick 保留多个实例，并且不把完整
   `DamagePayload` 长期附着到 Projectile。

目标数据流：

```text
空间组件
  → PhysicsContactCaptureSystem
  → PhysicsContactBuffer（Tick 内事实，不附着到 Projectile）
  → Player/NPC/Projectile 领域适配器
  → CombatAttackIntentBuffer（Tick 内意图，不覆盖）
  → CombatAttackRequestSystem
  → CombatDamageRequestBuffer
  → CombatDamageSystem
  → DamageAppliedReceipt / DamageProcessResult
  → 来源领域结果消费者
  → ProjectileTerminationBuffer
  → ProjectileTerminationCommitSystem
  → ProjectileLifecycleSystem.CommitTermination（唯一 Destroy）
```

目标不是把所有系统合并成一个“大系统”。Physics 只产生几何事实，Combat 只执行敌我关系、
属性计算和生命提交，Projectile 只解释自己的命中、穿透、免疫和生命周期结果。

## 2. 当前基线和证据等级

以下结论基于当前工作树源码。当前工作树已有用户和历史未提交变更；执行时必须保留这些变更，
不得使用 `git reset --hard`、`git clean` 或覆盖式 checkout。

| 位置 | 当前行为 | 证据等级 | 本轮处理 |
| --- | --- | --- | --- |
| `Components/Share/Physics/SpatialSimulationCoordinatorSystem.cs:40-48` | 查询强制要求 `EntityFactionComponent` 和 `LifeComponent`；`Update` 以生命和阵营决定是否参与空间模拟 | confirmed | 移除 Combat 准入条件，只保留空间能力组合 |
| `Components/Projectile/ProjectileCollisionSystem.cs:40-45,84-105` | Projectile 碰撞系统读取目标阵营和生命，并在几何检查前做敌我/存活过滤 | confirmed | 移到 Physics 接触事实 + Combat/Projectile 适配器 |
| `Components/Npc/NpcAiIntentAdapter.cs:153-188` | AI 适配器解析目标实体、读取目标生命并直接调用 `EntityCollisionSystem.TryGetContact` | confirmed | AI 只产生攻击意图；接触由统一 Physics 事实提供 |
| `Components/Share/Physics/CollisionContactBufferComponent.cs` | 一次性接触列表附着在每个 Projectile 实体上 | confirmed partial | 改为 world-scoped Tick buffer；删除 Projectile 专属接触组件 |
| `Components/Projectile/ProjectileDamageRequestSystem.cs:23,47-88` | 从 Projectile 组件读取 `CombatDamageSourceComponent.Payload`，再构造 `DamageRequest` | confirmed | 改为按攻击定义键在请求生成时解析载荷 |
| `Components/Projectile/ProjectileLifecycleSystem.cs:118-157` | `TryTerminate` 设置状态后直接调用 `_world.Destroy`；另有 `ProjectileTerminationCommitSystem` | confirmed | `TryTerminate` 改为请求入口；只有 Commit 系统调用内部提交方法 |
| `Components/Projectile/ProjectileHitResolutionSystem.cs:140-148` | 命中结果直接写 `PendingTerminate` 和终止请求组件 | confirmed | 改为写入统一终止缓冲 |
| `Components/Projectile/ProjectileLifetimeTickSystem.cs:29-52` | 到期直接写 Projectile 终止请求组件 | confirmed | 改为写入统一终止缓冲 |
| `Components/Share/Combat/CombatAttackIntentComponent.cs:13-34,36-101` | 一个实体只有一个 pending 请求和一个 `_lastResult`；`Submit` 会覆盖前一个请求 | confirmed | 删除请求/结果聚合组件，使用 Tick 内多值缓冲 |
| `Components/Share/Combat/CombatDamageRequestBuffer.cs:17-75` | Damage 请求本身追加到列表，结果按 source 消费；可复用但缺少攻击意图层的多值合同 | confirmed partial | 明确 Tick 边界、RequestId 和逐请求结果消费 |
| `Components/Share/Combat/CombatDamageSystem.cs:82-150` | Combat 自身读取 Faction、Life、Attributes 并提交伤害 | confirmed | 保留；这是 Combat 的权威职责，不下沉到 Physics |
| `Components/Share/Combat/CombatSystemRegistration.cs:40-72` | 已存在局部阶段注册，但没有证明所有宿主都使用完整注册闭包 | partial | 迁移后增加接线验证，不把注册方法当成运行时闭环证据 |

旧的 `DamagePayloadComponent` 和 `ProjectileHitCandidate` 已经从当前源码删除。当前的
`CombatDamageSourceComponent` 是同类遗留：它虽然只保存 payload，但仍把完整攻击载荷长期挂到
Projectile 实体上。本计划针对当前代码，不恢复旧类型。

## 3. 不变量和权威所有权

### 3.1 持续状态与一次性事实

| 数据 | 形式 | 权威 owner | 生命周期 |
| --- | --- | --- | --- |
| `LocationComponent`、`VelocityComponent`、`ColliderComponent` | ECS 组件 | Physics/Movement 提交系统 | 实体存活期间 |
| `EntityFactionComponent`、`LifeComponent`、Attributes | ECS 组件 | Entity/Combat/Attributes owner | 实体存活期间 |
| `PhysicsContactFact` | `readonly record struct` | Physics buffer | 当前 Tick，消费后清除 |
| `CombatAttackIntent` | `readonly record struct` | Attack intent buffer | 当前 Tick，转换后清除 |
| `DamageRequest` | `readonly record struct` | Combat request buffer | 当前 Tick，处理后清除 |
| `DamageProcessResult` | 结果 buffer 值 | Combat result buffer | 当前 Tick，来源消费后清除 |
| Projectile 终止请求 | `ProjectileTerminationRequest` 值 | Projectile lifecycle buffer | 当前 Tick，Commit 后清除 |
| Projectile 穿透计数、免疫目标列表 | ECS 组件 | Projectile 命中结果系统 | 跨 Tick 运行态 |
| Projectile 是否存活 | `ProjectileDispositionStateComponent` + Arch 实体 | Projectile lifecycle | 实体生命周期 |

一次性事实不能作为 Projectile、NPC 或 Player 的长期组件保存。若宿主需要调试或回放，使用
独立的 Tick 投影，并明确它不是权威写入源。

### 3.2 领域边界

```text
Physics
  读：位置、碰撞体、碰撞层/掩码、运行时句柄、空间版本
  写：PhysicsContactBuffer
  不读：Life、Faction、Attributes、DamagePayload、Projectile 终止状态

Combat
  读：DamageRequest、Life、Faction、Attributes、保护窗口
  写：目标 Life、护盾/属性中明确授权的提交、DamageProcessResult
  不写：几何接触、Projectile 穿透、Projectile 删除、NPC/Player 行为阶段

Projectile
  读：PhysicsContactFact、Projectile policy、攻击定义键、命中运行态
  写：Projectile 命中免疫/穿透、终止请求
  不写：目标 Life、Faction、Physics 几何事实

Lifecycle
  读：终止请求缓冲、当前 Projectile 实体和 Registry
  写：Disposition、Registry、Arch 结构
  唯一结构提交：`World.Destroy`
```

## 4. 目标模型

### 4.1 统一 Physics 接触事实

新增共享值类型 `PhysicsContactFact`，建议字段如下：

```text
First              EntityRuntimeHandle
Second             EntityRuntimeHandle
Contact            EntityCollisionContact（点、法线）
Tick               long
SpatialRevision    SpatialWorldRevision
Substep            int
```

`First` 和 `Second` 是几何配对端点，不使用 `Source`/`Target` 命名。谁是攻击源由 Projectile、
NPC 或 Player 适配器根据自己的关系和攻击规则决定，Physics 不替它们解释。

新增 world-scoped `PhysicsContactBuffer`：

- `BeginCapture(tick, revision)` 清理上一个 Tick 的事实；
- `Add(in PhysicsContactFact)` 校验 Tick、空间版本、句柄和子步；
- `Drain()` 返回不可变数组并清空当前批次；
- 在 World 清理时 `Clear()`，不留下旧 generation 的句柄；
- 不作为 Projectile、NPC 或 Player 组件附加。

保留 `EntityCollisionSystem` 的纯 AABB/Layer/Mask 计算，但让它只返回几何结果。若需要兼容
Arch 的世界级存储，允许把 `PhysicsContactBuffer` 挂在唯一的 Physics world singleton 上；
不得重新挂到每一个 Projectile 实体上。

### 4.2 Physics 捕获系统

新增 `PhysicsContactCaptureSystem`，查询最小的几何组合：

```text
LocationComponent
ColliderComponent
EntityCollisionCapabilityComponent
EntityRuntimeHandle（通过 resolver 获取，不作为 Combat 条件）
```

它调用 `EntityCollisionSystem.FindContacts`，把每一对几何重叠写入 `PhysicsContactBuffer`。
它不要求 `LifeComponent`、`EntityFactionComponent`、Attributes、Projectile 组件或攻击定义。
实体是否死亡、是否可受伤、是否是友军，由后续领域适配器和 Combat 决定；若实体不应参与
空间，必须由生命周期或碰撞能力系统禁用 Collider，而不是让 Physics 偷读 Life。

### 4.3 攻击定义键替代 Projectile 载荷附着

新增共享不可变定义合同：

```text
CombatAttackDefinitionId
CombatAttackDefinition
CombatAttackDefinitionCatalog（注册后 Seal）
ProjectileAttackDefinitionReferenceComponent
```

`CombatAttackDefinition` 保存基础伤害、伤害类型、关系、击退、元素和穿透输入等定义值；
它不保存实体、目标、Life 快照或 Arch 引用。Catalog 是不可变内容目录。

Projectile 实体只附加：

```text
ProjectileAttackDefinitionReferenceComponent
  → CombatAttackDefinitionId
```

`ProjectileDefinition` 不再把完整 `DamagePayload` 作为实体 hydration 输入；它改为持有攻击
定义键。Projectile 请求适配器在生成当 Tick 的 `DamageRequest` 时解析 Catalog，构造一次性的
`DamagePayload`，再交给 Combat。Combat 仍在处理时读取来源和目标当前 Attributes，因此不会
把攻击快照或属性快照存进 Projectile。

Player 和 NPC 使用相同的攻击定义键。攻击意图只保存定义键、目标句柄、Tick、RequestId 和
可选接触事实；`ExpectedTargetLife` 在转成 `DamageRequest` 的边界读取，避免再次把 Life
快照附着到长期组件。

### 4.4 多请求攻击缓冲

删除 `CombatAttackIntentComponent` 的请求/结果聚合职责，新增：

```text
CombatAttackIntent
CombatAttackIntentBuffer
CombatDamageResultBuffer（或扩展现有结果 buffer 的逐 RequestId 消费 API）
```

缓冲要求：

- 一个来源同一 Tick 可以追加多个目标、多个攻击定义和多个 RequestId；
- `Enqueue` 只能追加，不能替换 pending 项；
- `BeginTick` 清理旧 Tick，拒绝跨 Tick 误消费；
- 结果按 `DamageRequestKey` 保存，按 source 和 RequestId 消费；
- 即使同一 source 的两个请求都命中同一目标，也分别产生结果，第二个由生命快照规则决定
  是否被拒绝，不能静默覆盖；
- Player/NPC 结果系统只更新攻击阶段、冷却或资源状态，不把结果写回一个 `LastResult` 槽位；
- Projectile 结果系统逐条消费 accepted receipt，按每条 receipt 更新免疫/穿透。

Player、NPC、Projectile 适配器共享这一缓冲，但不共享它们的持续行为组件。

### 4.5 统一 Projectile 生命周期提交

新增 world-scoped `ProjectileTerminationBuffer` 和值类型：

```text
ProjectileTerminationRequest(
  Handle,
  Reason,
  Tick,
  RequestId)
```

生命周期规则：

1. 寿命到期、穿透耗尽、管理员清理、Owner 清理都只调用 `RequestTermination`，追加请求；
2. `ProjectileTerminationCommitSystem` 在 Commit 阶段按 handle 稳定排序并去重；
3. Commit 系统验证 handle、generation、实体仍存活和当前 disposition；
4. Commit 系统调用 `ProjectileLifecycleSystem.CommitTermination`；
5. `CommitTermination` 是唯一可以执行 Registry 移除、标记已终止和 `_world.Destroy` 的方法；
6. 终止完成后清理该 Projectile 的命中状态、接触事实投影和缓冲索引；
7. 重复终止请求返回 `AlreadyTerminated`，不会第二次 Destroy。

迁移期间可暂时保留 `PendingTerminate` 枚举值，但所有生产者不得直接写
`ProjectileDispositionStateComponent` 来表示终止请求。`ProjectileTerminationRequestComponent`
在所有调用点迁移后删除；它是一次性请求，不应作为实体长期组件。

## 5. 分阶段执行计划

每个批次都必须通过本批验收后才能进入下一批。计划阶段不修改生产 C#；实施阶段必须逐批
执行并记录实际命令、退出码、警告/错误数和 Build 输出路径。

### Phase 0：基线、契约和静态门禁

**目标**：锁定当前行为、文件边界和可回滚批次。

**任务**

1. 记录当前 `ProjectileCollisionSystem`、`ProjectileDamageRequestSystem`、
   `ProjectileLifecycleSystem`、`CombatAttackIntentComponent` 和
   `SpatialSimulationCoordinatorSystem` 的源码 hash 与调用点。
2. 枚举所有 `World.Destroy`、所有 `ProjectileTerminationRequestComponent` 写入点，以及所有
   `CombatDamageSourceComponent` 创建点。
3. 固定 `PhysicsContactFact`、`CombatAttackIntent`、`DamageRequest` 和终止请求的字段合同。
4. 建立静态门禁脚本，至少检查：
   - `Components/Share/Physics` 不引用 `LifeComponent`、`EntityFactionComponent`、Attributes 或 Combat payload；
   - Projectile 领域除 `ProjectileLifecycleSystem.CommitTermination` 外不调用 `World.Destroy`；
   - Projectile 创建不再附加完整 `DamagePayload`；
   - 攻击意图不含 `ExpectedTargetLife`、`DamagePayload`、`LastResult`；
   - 所有一次性事实都有 Tick/清理边界。

**验收**：基线清单和静态门禁脚本可在当前工作树运行；不改变生产行为；未确认的宿主注册
入口标记为 `partial`，不写成完整闭环。

### Phase 1：解除 Physics → Combat 硬依赖

**目标**：Physics 可以模拟没有 Life/Faction 的移动实体，且不承担敌我准入。

**文件范围**

- 修改：`Components/Share/Physics/SpatialSimulationCoordinatorSystem.cs`
- 修改：`Components/Share/Physics/README.md`
- 可能修改：空间组件查询、Physics 相关验证项目

**任务**

1. 将 `SpatialSimulationCoordinatorSystem` 查询收敛到位置、速度、碰撞体、接地和空间能力；
   删除 `EntityFactionComponent`、`LifeComponent` 的查询和校验。
2. 将实体是否参与空间模拟改为 Collider/Movement capability 的职责；死亡实体若仍存在，
   由生命周期系统禁用其碰撞能力或移除实体。
3. 保持 `SpatialWorldRevision` 校验和空间提交逻辑不变，确保这批只改变准入边界。
4. 更新 README，明确 Physics 不负责生命、阵营和伤害。

**验收标准**

- 只有 Location、Velocity、Collider、Grounding 和必要空间能力的实体可以被模拟；
- 缺少 Life/Faction 的 Projectile 或世界物件仍能运行空间步骤；
- `rg` 检查 Physics 领域无 Combat 状态依赖；
- 受影响项目增量构建通过；
- 空间冒烟验证位置、速度、接地和液体提交未改变。

**回滚点**：仅回滚本批 Physics 文件和对应测试，不回滚 Combat/Projectile 工作树变更。

### Phase 2：建立统一 Physics 接触事实

**目标**：所有实体接触从同一 Physics buffer 产生，消除 Projectile 专属接触组件。

**文件范围**

- 新增：`Components/Share/Physics/PhysicsContactFact.cs`
- 新增：`Components/Share/Physics/PhysicsContactBuffer.cs`
- 新增：`Components/Share/Physics/PhysicsContactCaptureSystem.cs`
- 修改：`Components/Share/Physics/EntityCollisionSystem.cs`
- 修改/删除：`Components/Share/Physics/CollisionContactBufferComponent.cs`
- 修改：`Components/Share/Physics/README.md`

**任务**

1. 将当前 `CollisionContactFact(Source, Target, Contact, Substep)` 迁移为端点中立的
   `PhysicsContactFact(First, Second, Contact, Tick, Revision, Substep)`。
2. 实现 world-scoped `PhysicsContactBuffer`，按 Tick 和空间 revision 捕获、验证、Drain、Clear。
3. 让 `PhysicsContactCaptureSystem` 使用通用空间查询和 `EntityCollisionSystem.FindContacts`；
   不查询 Projectile、Faction、Life 或 Combat payload。
4. 规定端点方向由领域适配器解释；Projectile 不再要求 Physics 生成 `Source/Target` 语义。
5. 删除 `CollisionContactBufferComponent` 的生产附着和所有 `BeginCapture/Drain` 调用；在过渡期
   只保留值类型兼容转换，不保留实体上的长期缓冲。
6. 同一 Tick 的重复接触由适配器按 `(First, Second, Tick, Substep)` 去重；跨 revision 的事实
   直接丢弃并记录诊断结果。

**验收标准**

- Player、NPC、Projectile 两两重叠都能进入同一个 Physics buffer；
- ContactFact 含 Tick 和 SpatialRevision，旧 Tick/旧 generation/旧 revision 不会被消费；
- Physics buffer Drain 后为空，World 清理后不保留旧句柄；
- Physics 项目不引用 Combat 命名空间或生命/阵营类型；
- AABB 点、法线、层/掩码规则与现有 `EntityCollisionSystem` 冒烟保持一致。

**回滚点**：可恢复旧 `CollisionContactBufferComponent` 作为临时适配，但不得同时启用两套
事实源；回滚时整批恢复 capture、buffer 和消费者，避免只恢复类型不恢复清理边界。

### Phase 3：消除攻击载荷附着和请求覆盖

**目标**：Projectile 只保存攻击定义键；Player/NPC/Projectile 的请求均追加到多值缓冲。

**文件范围**

- 新增：`Components/Share/Combat/CombatAttackDefinitionId.cs`
- 新增：`Components/Share/Combat/CombatAttackDefinition.cs`
- 新增：`Components/Share/Combat/CombatAttackDefinitionCatalog.cs`
- 新增：`Components/Projectile/ProjectileAttackDefinitionReferenceComponent.cs`
- 新增：`Components/Share/Combat/CombatAttackIntent.cs`
- 新增：`Components/Share/Combat/CombatAttackIntentBuffer.cs`
- 修改/删除：`Components/Share/Combat/CombatAttackIntentComponent.cs`
- 修改：`Components/Share/Combat/CombatDamageRequestBuffer.cs`
- 修改：`Components/Share/Combat/README.md`

**任务**

1. 将 `ProjectileDefinition.Damage` 改为 `CombatAttackDefinitionId`；Definition Catalog 仍为
   不可变内容，实体 hydration 只附加 `ProjectileAttackDefinitionReferenceComponent`。
2. 移除 `ProjectileLifecycleSystem.TrySpawn` 中的 `new CombatDamageSourceComponent(definition.Damage)`。
3. 将 `CombatAttackIntentComponent` 的请求/结果字段移出实体。Player/NPC 的持续攻击阶段、
   冷却和资源状态留在各自领域组件；一次攻击进入 `CombatAttackIntentBuffer`。
4. `CombatAttackIntentBuffer.Enqueue` 追加完整 intent，生成 source-local RequestId；禁止用
   “当前 pending 槽位”替换同 Tick 前一项。
5. `CombatAttackRequestSystem` 从 intent 解析定义、读取目标当前 Life 快照并生成 `DamageRequest`；
   该系统不写 Life。
6. 扩展结果 buffer 使每个 RequestId 都有独立结果，Player/NPC/Projectile 消费后按请求删除；
   删除 `_lastResult` 语义。

**验收标准**

- 同一 Projectile 不带 `DamagePayload`、Life 快照或命中结果组件；
- 同一来源同一 Tick 追加两个目标请求后，两个请求都进入 buffer；
- 多个结果不会覆盖，结果顺序由稳定 RequestId/排序合同决定；
- 攻击定义目录封存后不可变，实体只保存键；
- 重复 `(Source, Target, Tick, Cause, RequestId)` 只处理一次；
- `DamageRequest` 仍是一次性值，Combat 的 Life 快照竞争规则保持有效。

**回滚点**：保留一个只读的旧构造器适配器直到全部调用者迁移；不得恢复实体上的
`CombatDamageSourceComponent` 作为新路径。

### Phase 4：Projectile 接入统一 Physics/Combat 管道

**目标**：Projectile 不再自己枚举目标、读取 Faction/Life 或计算几何；只解释通用事实和自身策略。

**文件范围**

- 删除或重构：`Components/Projectile/ProjectileCollisionSystem.cs`
- 修改：`Components/Projectile/ProjectileDamageRequestSystem.cs`
- 修改：`Components/Projectile/ProjectileDamageResultSystem.cs`
- 修改：`Components/Projectile/ProjectileHitResolutionSystem.cs`
- 修改：`Components/Projectile/README.md`
- 修改：`Components/Share/Combat/CombatSystemRegistration.cs`

**任务**

1. `ProjectileDamageRequestSystem` 从 `PhysicsContactBuffer` 读取端点中立事实，识别哪一端是
   Projectile，并应用 `IgnoreOwner`、自身实体排除和 Projectile disposition 规则。
2. 删除 Projectile 内部的 `target Life`、`target Faction` 和 `IgnoreSameFaction` 最终判定；
   友军/敌对关系统一交给 `DamageRelationSystem`。`IgnoreSameFaction` 若无独立需求则删除，
   若保留只能作为不改变结果的候选优化，不能成为 Combat 之外的权威规则。
3. 通过 `ProjectileAttackDefinitionReferenceComponent` 解析攻击定义键，创建
   `CombatAttackIntent`；不把完整载荷重新挂回实体。
4. `CombatAttackRequestSystem` 统一把 Projectile intent 转为 `DamageRequest`，交给现有
   `CombatDamageRequestSystem`、`DamageAdmissionSystem`、`DamageRelationSystem`、
   `DamageMitigationSystem` 和 `DamageCommitSystem`。
5. `ProjectileDamageResultSystem` 遍历每个 Projectile 的所有结果；只有 accepted receipt
   才更新穿透和命中免疫，拒绝结果不消耗命中次数。
6. 更新注册顺序：Physics capture → Projectile/Player/NPC intent adapters → Attack request
   conversion → Combat commit → result consumers。

**验收标准**

- Projectile 与没有 Life/Faction 的空间实体可以由 Physics 捕获接触，但只有 Combat 准入成功
  才会产生伤害；
- Owner 排除仍有效，友军/敌对关系由统一 Combat 矩阵决定；
- 一个 Projectile 同 Tick 命中多个目标时，所有接触事实都被适配，不被单槽位覆盖；
- 目标版本冲突、保护窗口、关系拒绝和零伤害不会消费 Projectile 穿透；
- 普通命中造成的 Life 变化与当前 Combat 公式一致；
- `ProjectileCollisionSystem` 不再调用 `EntityCollisionSystem.TryGetContact` 或读取目标 Life/Faction。

**回滚点**：保留旧 Projectile adapter 的只读兼容入口，但所有新事实必须来自统一 Physics buffer；
不得并行维护第二套几何扫描。

### Phase 5：NPC/Player 攻击适配器解耦

**目标**：AI、Player 攻击输入和 Physics 接触分离，三类来源进入同一 intent/request 合同。

**文件范围**

- 修改：`Components/Npc/NpcAiIntent.cs`
- 修改：`Components/Npc/NpcAiIntentAdapter.cs`
- 修改/新增：`Components/Npc/NpcAttackRequestSystem.cs`
- 修改：`Components/Npc/NpcAttackResultSystem.cs`
- 修改：`Components/Player/PlayerAttackRequestSystem.cs`
- 修改：`Components/Player/PlayerAttackResultSystem.cs`
- 修改：`Components/Share/AI/README.md`

**任务**

1. `NpcAiIntent` 的攻击字段从 `DamagePayload?` 改为攻击定义键；AI 只输出目标和行为意图。
2. 删除 `NpcAiIntentAdapter.TryQueueAttack` 中的目标 Life 读取和直接 AABB 接触调用；它只把
   AI 结果追加到 intent buffer。
3. 需要接触才能攻击的 NPC，由统一接触适配器按 Physics fact 决定是否生成 intent；远程 NPC
   按自己的授权攻击规则生成 intent，不在 AI 中读取 Physics 服务。
4. Player/NPC 请求系统不再查询 `CombatAttackIntentComponent`，改为消费各自来源的 intent
   入口或向共享 buffer 写入；结果系统按 RequestId 逐条处理。
5. 失败结果只更新攻击阶段/冷却/资源的明确 owner，不把失败覆盖成最后一次成功或相反结果。

**验收标准**

- NPC AI 不再直接读取目标 Life、Collider 或调用 `TryGetContact`；
- 同一 NPC 同 Tick 的多个攻击意图均可追踪到独立 RequestId；
- Player/NPC/Projectile 使用同一 DamageRelation 和 DamageCommit 路径；
- 没有任何客户端控制组件回流到 NSSLC；
- AI、Player 和 Combat 的查询/写集在文档中可分别列出。

**回滚点**：保留 NPC/Player 旧适配器的入口签名仅用于迁移调用者，但旧实现不得继续写目标
Life 或直接调用碰撞系统。

### Phase 6：统一 Projectile 生命周期提交

**目标**：终止请求只有一个事实源，Arch Destroy 只有一个提交点。

**文件范围**

- 新增：`Components/Projectile/ProjectileTerminationRequest.cs`
- 新增：`Components/Projectile/ProjectileTerminationBuffer.cs`
- 修改：`Components/Projectile/ProjectileTerminationCommitSystem.cs`
- 修改：`Components/Projectile/ProjectileLifecycleSystem.cs`
- 修改：`Components/Projectile/ProjectileHitResolutionSystem.cs`
- 修改：`Components/Projectile/ProjectileLifetimeTickSystem.cs`
- 修改：Projectile owner cleanup 的调用点
- 删除：`Components/Projectile/ProjectileTerminationRequestComponent.cs`（迁移完成后）

**任务**

1. 把寿命、穿透、管理员清理和 Owner 清理统一改为 `TerminationBuffer.Enqueue`。
2. 把 `ProjectileLifecycleSystem.TryTerminate` 拆成公开的 `RequestTermination` 和内部的
   `CommitTermination`；公开方法不改变 Arch 结构。
3. `ProjectileTerminationCommitSystem` 在一次查询外 Drain 请求，按 handle 去重，再调用唯一
   `CommitTermination`。
4. 删除 `ProjectileLifecycleSystem.TerminateOwnedBy` 内部直接调用销毁的路径，改为批量 enqueue；
   若宿主要求同步结果，返回“已提交请求数”，而不是伪装成已经 Destroy。
5. 清理 Registry、终止历史、免疫列表和接触 buffer 的生命周期；旧 handle 在 Commit 后必须拒绝。
6. 将 `PendingTerminate` 限制为提交期间的派生态；终止请求不再通过实体上的 request component
   保存跨系统传递。

**验收标准**

- `rg` 结果显示 Projectile 领域只有 `CommitTermination` 包含 `_world.Destroy`；
- 同 Tick 的过期和命中耗尽请求只产生一次 Destroy；
- 重复、旧 generation、foreign world 的终止请求不会影响新实体；
- 终止后 Registry、ProjectileRuntimeRegistry 和 buffer 均无可消费残留；
- 任何来源都可以请求终止，但只有 Lifecycle Commit 系统执行结构改变。

**回滚点**：保留 `ProjectileTerminationRequestComponent` 只到所有生产者迁移完成；若回滚，整批恢复
buffer、Commit 系统和 Lifecycle API，不能只恢复一个直接 Destroy 调用。

### Phase 7：宿主接线、验证和文档收口

**目标**：证明阶段顺序和行为闭环，而不是仅证明类型能编译。

**任务**

1. 在 `CombatSystemRegistration` 或实际宿主注册入口明确以下顺序：

   ```text
   Preparation
     → intent 输入收集
   Simulation
     → SpatialSimulationCoordinatorSystem
     → PhysicsContactCaptureSystem
     → Projectile/NPC/Player intent adapters
   Commit
     → CombatAttackRequestSystem
     → CombatDamageRequestSystem
     → Projectile/Player/NPC result consumers
     → ProjectileTerminationCommitSystem
   Projection
     → 仅本地诊断/观察
   ```

2. 增加 focused verifier，覆盖 Physics 无 Life/Faction 模拟、接触事实 Tick/revision、双请求不覆盖、
   攻击载荷不附着、Combat 版本冲突、命中结果逐条消费和终止单点 Destroy。
3. 更新以下文档，删除“Projectile 保存 `ProjectileHitCandidate`/接触组件/完整 DamagePayload”
   等过时描述：
   - `Components/Share/Physics/README.md`
   - `Components/Share/Combat/README.md`
   - `Components/Projectile/README.md`
   - 本计划对应的实现记录和验收记录
4. 运行静态扫描，确认没有遗漏的旧组件、直接碰撞调用、直接 Destroy 或请求覆盖入口。

**验收标准**

- 受影响项目按仓库构建约束增量构建通过，输出位于 `Build/bin/`；
- focused verifier 的行为结果通过，不能只报告编译通过；
- 宿主接线入口和阶段顺序有源码证据；
- 所有未接线或未覆盖的路径标记为 `partial`/`unknown`，不写成行为等价；
- 文档与源码一致，旧名称只在迁移记录中出现。

## 6. 文件与依赖变更矩阵

| 批次 | 新增/修改核心类型 | 依赖方向 | 禁止依赖 |
| --- | --- | --- | --- |
| B1 | `SpatialSimulationCoordinatorSystem` | Physics → Movement/Entity geometry | Combat/Life/Faction |
| B2 | `PhysicsContactFact`、`PhysicsContactBuffer`、`PhysicsContactCaptureSystem` | Physics → Entity handle/geometry | Combat、Projectile、Attributes |
| B3 | Attack definition/catalog、intent buffer | Combat definitions → source adapters | Arch entity、目标 Life 快照长期存储 |
| B4 | Projectile contact/request adapters | Projectile → Physics facts + Combat intent | Physics 几何重算、Faction/Life 最终准入 |
| B5 | Player/NPC adapters | Player/NPC → intent buffer | 直接 Life 写入、直接 Physics 查询 |
| B6 | Termination buffer/commit | Projectile lifecycle → Arch structure/Registry | 其他系统直接 Destroy |
| B7 | Registration/verifier/docs | Host → ordered systems | 以文件枚举或查询顺序暗示 phase |

迁移时保持命名空间和已有公共 API 的兼容范围最小化；需要重命名或删除类型时，先迁移所有
生产调用点，再删除旧定义，并在同一批记录源路径、目标路径、引用影响和回滚方式。

## 7. 验证计划

### 7.1 构建门禁

按仓库约束，只构建受影响项目，不构建整个解决方案：

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore --nologo -v:q `
  -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
```

每批记录：命令、项目、exit code、warning/error 数、实际输出路径和未构建项目。若使用 focused
verifier，运行生成的 `Build/bin/` 输出，并使用 `--no-build --no-restore` 做第二次验证。

### 7.2 行为验证矩阵

| 场景 | 断言 |
| --- | --- |
| 无 Life/Faction 的实体移动 | Physics 空间模拟成功，未因 Combat 条件跳过 |
| 两实体几何重叠 | 一个 `PhysicsContactFact`，含当前 Tick 和 revision |
| 旧 revision 接触 | 事实被拒绝或丢弃，不进入 Combat |
| Projectile 与目标接触 | 适配器只产生 intent/request，未直接写 Life |
| 同一 Projectile 同 Tick 两目标 | 两个独立 RequestId，结果不覆盖 |
| 同一来源两次攻击同一目标 | 第一条可提交，第二条按 Life 快照返回 `TargetChanged` 或明确重复原因 |
| Player/NPC 多结果 | 每个结果都能按 RequestId 消费，不能只保留最后一个 |
| 友军/敌对关系 | 只由 `DamageRelationSystem` 决定，Physics 不读 Faction |
| 保护窗口/零伤害/目标死亡 | 不提交 Life，不消费 Projectile 穿透 |
| 过期和命中耗尽同 Tick | 终止请求去重，只调用一次 `CommitTermination` |
| 终止后旧句柄 | Registry 和 Projectile buffer 均拒绝旧 handle |
| World 清理 | Physics/Combat/Termination buffer 不残留上一世界数据 |

### 7.3 静态扫描门禁

```text
Physics 目录不存在 LifeComponent、EntityFactionComponent、CombatDamageSourceComponent 引用
Projectile 目录不存在 ProjectileHitCandidate、DamagePayloadComponent 和实体接触 buffer 附着
CombatAttackIntentComponent 不再保存 Payload、ExpectedTargetLife、LastResult
除 ProjectileLifecycleSystem.CommitTermination 外不存在 Projectile World.Destroy
NpcAiIntentAdapter 不调用 EntityCollisionSystem.TryGetContact
ProjectileDamageRequestSystem 不调用 EntityCollisionSystem.TryGetContact
所有 Buffer 都有 BeginTick/Drain/Clear 或等价生命周期入口
```

静态零命中只能说明未搜索到声明范围内的引用；动态注册、反射或外部宿主接线仍标记为
`partial`，不能据此宣布完整运行时闭环。

## 8. 风险、未决项和处理方式

1. **宿主注册入口不完整**：当前 `CombatSystemRegistration` 是局部注册合同，实际服务器是否
   使用它仍需源码追踪。Phase 7 前标记 `partial`，不得以注册方法存在代替行为验证。
2. **物理接触的世界级存储方式**：首选注入 world-scoped `PhysicsContactBuffer`；若宿主只能
   通过 Arch 组件传递，使用唯一 world singleton，不回到 Projectile 实体组件。两种实现必须
   共享同一 `PhysicsContactFact` 和清理合同。
3. **攻击定义热更新**：Catalog 封存后不允许运行时修改；若未来支持热更新，必须增加定义版本
   并把版本放进攻击意图，而不是把完整载荷重新附着到 Projectile。
4. **实体死亡与 Physics 接触**：Physics 可以捕获已经死亡但 Collider 尚未清理的几何事实；
   Combat admission 负责拒绝。若产品要求死亡实体立即失去碰撞，调整生命周期/Collider owner，
   不把 Life 重新加回 Physics 查询。
5. **连续 Sweep**：本计划统一事实格式和版本边界，不宣称已经实现连续轨迹 Sweep；Sweep 作为
   `PhysicsContactFact.Substep`/几何生产器的后续增强，不能在适配器中偷偷补算。
6. **结果副作用**：反射、吸血等属性计算结果仍由专门效果 owner 提交；本计划不把这些效果
   塞回 Projectile 或 CombatDamageCommit 的跨域写入。
7. **Registry 历史集合**：Projectile 终止单点提交后，继续检查 `ProjectileRuntimeRegistry`
   的终止历史是否有界；这属于同一生命周期清理批次的技术债，不可用“已统一 Destroy”替代。

## 9. 回滚与完成定义

### 9.1 回滚规则

- 每个 Phase 是一个可独立回滚批次；回滚只触及该批新增/修改文件。
- 不回滚用户已有的 Projectile、Combat、Attributes、Player、NPC、Physics 和 Entity 变更。
- 不使用全局 reset/clean；保留每批的验证输出和静态扫描记录。
- 兼容适配器只能短期存在，必须标注删除条件；不能形成第二套长期权威状态。

### 9.2 完成定义

只有以下条件全部满足，才能把本计划状态改为 `implemented`：

1. Physics 查询和 README 不再把 Life/Faction 当作空间参与条件；
2. 所有实体接触来自同一带 Tick/revision 的 Physics buffer；
3. Projectile 不附加完整 DamagePayload，不保存一次性接触列表；
4. Player/NPC/Projectile 攻击在同 Tick 多请求不覆盖，并能逐 RequestId 消费结果；
5. 所有 Projectile 终止请求经过唯一 `ProjectileTerminationCommitSystem`，只有
   `ProjectileLifecycleSystem.CommitTermination` 执行 Destroy；
6. Combat 是伤害关系、属性计算和 Life 提交的唯一 owner，Physics 不再读取这些状态；
7. focused verifier、增量构建、静态门禁和宿主接线验证均有新鲜证据；
8. 未覆盖的连续 Sweep、网络、客户端、反射/吸血实际效果和 Registry 历史上界被单独记录，
   不被本计划的通过结果掩盖。

实施记录见 [Physics、Combat 与 Projectile 边界收敛实施记录](../architecture/2026-10-26-physics-combat-projectile-boundary-implementation.md)。当前计划已进入实现阶段。生产项目、三个 focused smoke 项目和静态门禁已通过；宿主实际
注册入口仍只证明 `CombatSystemRegistration` 的局部合同，连续 Sweep、网络、客户端、反射/吸血
副作用和 Registry 历史上界仍保留为未决技术债，不能据此宣称完整游戏运行时闭环。

