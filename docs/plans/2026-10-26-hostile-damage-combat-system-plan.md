# 通用敌对伤害 Combat 系统设计与执行计划

文档 ID：DOC-2026-10-26-HOSTILE-DAMAGE-COMBAT-PLAN  
需求性质：核心战斗流程与 ECS 系统边界变更  
建议变更级别：重大  
执行策略：spec  
状态：implemented；核心代码与本地端到端冒烟已完成，接入边界和剩余限制见文末  
日期：2026-10-26  
范围：`src/NSSLC/Terraria.Components.csproj` 及其本地验证项目  
明确排除：网络、客户端表现、`src/NSSLC.Infrastructure/`、持久化协议和客户端架构迁移

## 1. 需求原文与目标

用户需求：

> 编写设计计划 完成敌对伤害计算的通用 Combat 系统

目标是把当前只覆盖 Projectile 的局部命中链，演进为 Player、NPC、Projectile 都可以使用的
本地权威 Combat 伤害入口：

```text
攻击来源（Player / NPC / Projectile / 其他授权来源）
    ↓
物理或攻击领域产生接触/攻击事实
    ↓
DamageRequest
    ↓
来源和目标生命周期准入
    ↓
敌我关系与伤害策略判断
    ↓
DamageType 与双方 Attributes 参与计算
    ↓
唯一的 LifeComponent 提交
    ↓
DamageAppliedReceipt
    ↓
来源领域消费结果
```

其中 Projectile 消费结果处理穿透、命中免疫和终止；Player/NPC 消费同一结果处理攻击冷却、
资源或行为反馈。任何来源都不能直接修改目标 `LifeComponent`。

## 2. 当前基线与证据等级

### 2.1 已确认存在的能力

当前 NSSLC 已有一条 Projectile 局部闭环：

```text
ProjectileCollisionSystem
  → EntityCollisionSystem.TryGetContact
  → ProjectileHitCandidate
  → ProjectileCombatCommitSystem
  → DamageAdmissionSystem
  → DamageMitigationSystem
  → DamageCommitSystem
  → AcceptedHitReceipt
  → ProjectileHitResolutionSystem
  → ProjectileTerminationCommitSystem
```

已确认的责任如下：

| 能力 | 当前实现 | 证据状态 |
| --- | --- | --- |
| AABB 接触 | `EntityCollisionSystem` 只产生几何接触事实 | confirmed |
| Projectile 碰撞过滤 | Owner、同阵营、碰撞能力和生命状态过滤 | confirmed |
| 命中准入 | handle、目标生命快照、存活、非零伤害、保护窗口 | confirmed |
| 目标减伤 | `DamageReduction + Endurance`，并限制到 `[0, 1]` | confirmed |
| 生命提交 | `DamageCommitSystem` 是当前 Projectile 路径的唯一常规生命写入者 | confirmed |
| Projectile 后处理 | 命中免疫、穿透、PendingTerminate | confirmed |
| 宿主调度 | `EcsServer` 支持 Preparation/Simulation/Commit/Projection | confirmed（框架层） |
| 宿主完整装配 | 当前源码未找到实际 Projectile Combat 注册闭包 | partial |

### 2.2 已确认缺口

当前不存在或未形成通用契约的部分：

1. `ProjectileDamageComponent` 只有 `BaseDamage` 和 `Knockback`，没有明确的 `DamageType`。
2. `EntityFactionComponent` 只表达自身阵营；Projectile 只通过 `IgnoreOwner` 和
   `IgnoreSameFaction` 做局部过滤，没有共享的敌对伤害关系规则。
3. `DamageAdmissionSystem`、`DamageCommitSystem` 和 `AcceptedHitReceipt` 的 API 直接依赖
   `ProjectileHitCandidate`，Player/NPC 无法复用。
4. `DamageMitigationSystem` 没有使用攻击者伤害属性、伤害类型倍率、`Defense` 或
   `ArmorPenetration`。
5. Player 当前没有攻击伤害请求入口；NPC 的 `BeginCombatAction` 只更新行为阶段，没有生成
   通用伤害请求。
6. 当前 Projectile 碰撞输入是当前位置 AABB 接触，尚不能证明已经实现连续运动 Sweep。
7. `EcsServer` 的阶段能力已存在，但当前生产源码没有证明完整 Combat Tick 已由宿主注册。

上述缺口是本计划的输入，不把当前代码写成已完成的通用 Combat 系统。

## 3. 设计原则与边界

### 3.1 ECS 组织原则

- Player、NPC、Projectile 仍然由 Arch Entity 的组件组合表达，不建立 `PlayerEntity`、
  `NpcEntity` 或 `ProjectileEntity` 大型聚合对象。
- 共享模型按能力归属放在 `Components/Share/Combat`；Projectile 的穿透和免疫仍留在
  `Components/Projectile`。
- 不恢复 `EntityDefinitionReference` 或 `SpatialRevisionComponent`。
- 不新增客户端控制、摄像机、渲染、网络同步或协议字段。
- 不修改 `src/NSSLC.Infrastructure/`。

### 3.2 唯一写入责任

| 状态 | 唯一常规写入者 | 其他系统的权限 |
| --- | --- | --- |
| `LifeComponent` | `DamageCommitSystem`（以及既有明确的复活/治疗 owner） | 只能读取或提交受控请求 |
| `AttributeSetComponent` | Attributes 系统 | Combat 只读解析 |
| `EntityFactionComponent` | 实体创建/生命周期 owner | Combat 只读 |
| `ProtectionWindowComponent` | 保护窗口系统或 Combat 明确 API | Damage 准入只读 |
| Projectile 穿透/免疫/终止 | Projectile 命中结果系统 | Combat 只发布 receipt |
| Player/NPC 攻击阶段 | 各自领域适配器 | Combat 不写行为状态 |

### 3.3 计算与效果隔离

敌我关系、伤害类型映射和数值计算应是显式输入的确定性规则；Arch 查询、生命周期解析和
`LifeComponent` 写入集中在明确的系统提交边界。随机暴击、来源快照、时间和目标生命版本
必须成为请求或计算输入，不能由规则函数隐式读取全局状态。

## 4. 目标领域模型

### 4.1 伤害类型

新增共享值类型：

```csharp
public enum DamageType : byte
{
  Generic = 0,
  Melee = 1,
  Ranged = 2,
  Magic = 3,
  Summon = 4,
  Environmental = 5,
  True = 6,
  DamageOverTime = 7
}
```

第一批可以只实现 `Generic`、`Melee`、`Ranged`、`Magic`、`Summon` 和 `Environmental`；
`True` 与 `DamageOverTime` 必须在数值规则、保护窗口和生命周期语义明确后启用，不能只把
枚举值加入而宣称已经支持。

`DamageType` 表示一次命中的计算类型，不等于 `AttributeKind`。例如：

```text
DamageType.Ranged
  + AttributeKind.RangedDamage   // 攻击者的数值贡献

DamageType.Magic
  + AttributeKind.MagicDamage    // 攻击者的数值贡献
```

### 4.2 敌我关系

新增共享值类型：

```csharp
public enum DamageRelation : byte
{
  Hostile = 0,
  Friendly = 1,
  Neutral = 2
}
```

第一阶段的敌对伤害默认规则：

| 来源阵营 | 目标阵营 | `Hostile` 结果 |
| --- | --- | --- |
| Friendly | Hostile | Allowed |
| Hostile | Friendly | Allowed |
| Friendly | Friendly | Rejected |
| Hostile | Hostile | Rejected |
| Neutral | 任意 | Rejected，除非请求显式使用环境/中立策略 |
| 任意 | 自身 | Rejected，除非来源定义显式允许自伤 |

这张表必须由共享 `DamageRelationSystem`（或等价的纯规则入口）执行，不能继续由
`ProjectileCollisionSystem` 自己解释阵营关系。

`IgnoreOwner` 是 Projectile 的来源/碰撞策略，保留在 Projectile 领域；它不能替代
`DamageRelation`。`IgnoreSameFaction` 在迁移后应只作为候选生成的早期优化，最终敌我准入
仍由共享 Combat 规则再次确认。

### 4.3 伤害载荷

新增不可变共享值类型 `DamagePayload`，建议最小字段：

```text
BaseDamage       非负基础伤害
DamageType       本次计算类型
Relation         Hostile / Friendly / Neutral
Knockback        非负击退输入
Critical         已经确定的暴击事实，不在准入阶段随机
ArmorPenetration 来源已确定的护甲穿透输入
```

持续存在并会主动产生命中的实体可以附加共享 `DamagePayloadComponent` 作为默认输入；
Projectile 的 `ProjectileDamageComponent` 迁移为该共享载荷或由适配器转换为 `DamagePayload`。
Player/NPC 的一次攻击不要求在实体上长期挂载伤害组件，可以由攻击意图在请求生成时构造载荷。

### 4.4 DamageRequest

`DamageRequest` 是同一 Tick 内的明确请求值，不直接修改 ECS：

```text
SourceHandle
TargetHandle
DamagePayload
ExpectedTargetLife
Tick
Cause
RequestId（来源本地序号；Projectile 使用碰撞子步）
Contact（可选的命中点、法线和接触来源）
```

`Cause` 至少区分：

```text
Projectile
PlayerAttack
NpcAttack
Environmental
```

来源必须提供有效 world scope/generation handle；目标生命快照用于检测同 Tick 竞争和过期
请求。请求的缓冲区由 Combat owner 按 Tick 管理，不能让 Projectile、Player 和 NPC 各自
维护另一份生命提交队列。

### 4.5 DamageAppliedReceipt

统一结果值替代 Projectile 专用的 `AcceptedHitReceipt`：

```text
SourceHandle
TargetHandle
Cause
DamageType
DamageRelation
IncomingDamage
MitigatedDamage
TargetLifeBefore
TargetLifeAfter
Tick
```

还需要明确拒绝结果 `DamageRejectedReceipt` 或等价结果码，使来源可以区分：

```text
关系拒绝、目标已死亡、保护窗口、生命版本冲突、来源失效、伤害为零、规则非法
```

Projectile 可以在过渡期通过 `ProjectileHitReceiptAdapter` 消费共享 receipt；最终不再让共享
Combat 类型反向依赖 `ProjectileHitCandidate`。

## 5. 目标系统分解

### 5.1 Physics / 攻击事实生产

物理系统继续只产生接触事实：

```text
Location / Collider / EntityCollisionCapability
    → ContactFact
```

Projectile 的碰撞适配器把接触事实和 `DamagePayloadComponent` 转为 `DamageRequest`。
Player 近战、技能或物品使用适配器直接生成 `DamageRequest`；NPC AI 通过 `NpcCombatIntent`
和攻击定义生成同一请求。任何生产者都不能在此阶段写目标生命。

连续 Sweep 不属于本计划的数值 Combat 核心，但 Projectile 接入批次必须明确当前使用的是
当前位置 AABB 还是 `PreviousLocation → CurrentLocation` Sweep，并将未实现的连续命中记录为
独立技术债。

### 5.2 DamageRequest 收集与排序

新增 `CombatDamageSystem`（或同等的 Combat owner）作为通用协调边界：

1. 接收本 Tick 的 Projectile、Player、NPC 和环境请求。
2. 固化请求快照，拒绝负 Tick、非法 handle、负伤害和非法枚举。
3. 按明确顺序处理请求；不得依赖 Arch 查询顺序表达优先级。
4. 对已经处理的 `(SourceHandle, TargetHandle, Tick, Cause, RequestId)` 做本 Tick 重复策略。
5. 调用准入、关系、计算和提交步骤。
6. 将成功/拒绝结果交给来源领域的结果消费者。

第一阶段不引入消息总线或事件溯源；普通同 Tick 请求可以由 Combat owner 持有的只读列表
和提交循环表示。

### 5.3 DamageAdmissionSystem

准入系统只检查事实有效性和目标当前状态：

```text
source handle 有效且实体仍 Active
target handle 有效且实体仍 Active
target 有 LifeComponent 且仍存活
ExpectedTargetLife 与当前生命一致
ProtectionWindow 未激活
DamagePayload 字段合法
```

敌我关系检查可以作为准入中的明确步骤，但推荐由 `DamageRelationSystem` 保持可独立测试。
准入系统不得写 Life、Attributes 或 Projectile 状态。

### 5.4 DamageRelationSystem

该系统只读取：

```text
Source EntityFactionComponent
Target EntityFactionComponent
DamageRelation
来源的 SelfDamage / FriendlyFire / NeutralTarget 策略
```

输出 `Allowed` 或具名拒绝原因。所有 Player、NPC、Projectile 请求都使用同一矩阵；不能在
Projectile 系统中保留一份“同阵营过滤”，在 NPC 系统中再复制另一份关系判断。

### 5.5 DamageCalculation / DamageMitigation

第一批固定为显式、确定性的计算顺序，仍保持纯计算：

```text
ScaledDamage = BaseDamage × max(0, 1 + Damage + TypeDamage)
CriticalDamage = Critical ? ScaledDamage × 2 : ScaledDamage
Offense = min(Int32.MaxValue, floor(CriticalDamage))
EffectiveDefense = max(0, Target.Defense - Payload.ArmorPenetration
                                      - Source.ArmorPenetration)
AfterDefense = min(Int32.MaxValue, floor(max(0, Offense - EffectiveDefense × 0.5)))
FinalReduction = clamp(Target.DamageReduction + Target.Endurance, 0, 1)
AcceptedDamage = floor(AfterDefense × (1 - FinalReduction))
```

`Damage` 与类型伤害属性按加算分数贡献；`Melee/Ranged/Magic/Summon` 分别映射到其专属
Attribute。`True` 保留来源伤害加成和已确定暴击，但跳过目标 Defense、DamageReduction 和
Endurance。当前没有抗性 Attribute。`DamageOverTime` 表示按此规则结算的单次脉冲，请求生产者
负责脉冲时间与持续时长。Defense 系数 `0.5` 由具名常数表达，整数转换在各计算阶段向下取整并
饱和到 `Int32.MaxValue`；最终减伤仍 clamp 到 `[0, 1]`。

### 5.6 DamageCommitSystem

该系统是目标生命的唯一常规写入点：

1. 重新解析 Source/Target handle。
2. 重新验证目标生命快照和实体生命周期。
3. 校验计算结果未超过输入范围，处理零伤害和最小生命边界。
4. 写入目标 `LifeComponent`。
5. 生成 `DamageAppliedReceipt`。

它不负责：

```text
阵营选择
Projectile 穿透
Projectile 删除
Player/NPC 行为状态
网络广播
```

### 5.7 结果消费

共享 Combat 只发布本地 receipt，来源 owner 负责解释：

```text
ProjectileHitResolutionSystem
  → 命中免疫、穿透消费、PendingTerminate

PlayerAttackResultSystem
  → 攻击冷却、资源/弹药提交、攻击阶段完成

NpcAttackResultSystem
  → 行为阶段、攻击冷却、AI 反馈
```

Player/NPC 结果系统只在真正收到 `DamageAppliedReceipt` 后提交“攻击已生效”状态；关系拒绝
和目标版本冲突不能被当作命中成功。

## 6. Tick 阶段与依赖关系

目标调度顺序：

```text
Preparation
  ├─ 读取 Player/NPC 攻击意图
  ├─ ProjectileCollisionSystem 产生接触候选
  └─ 各来源适配器构造 DamageRequest

Simulation
  └─ 继续运动、AI、空间模拟；不写目标 Life

Commit
  ├─ CombatDamageSystem 收集并排序请求
  ├─ DamageAdmissionSystem
  ├─ DamageRelationSystem
  ├─ DamageCalculation / DamageMitigation
  ├─ DamageCommitSystem
  ├─ ProjectileHitResolutionSystem
  ├─ Player/Npc 结果消费者
  └─ ProjectileTerminationCommitSystem

Projection
  └─ 只构建本地观察或调试投影；本计划不加入网络复制
```

关键顺序约束：

- Physics 必须先于 Projectile 请求生成。
- Combat 请求必须在目标生命提交前固定快照。
- `DamageCommitSystem` 成功后才允许来源消费“命中成功”。
- Projectile 终止提交必须晚于穿透结果提交。
- 复活、治疗和死亡状态系统要与 Combat Commit 的先后关系在宿主注册处显式声明，不能靠
  文件顺序推断。

## 7. 分阶段执行计划

每一批都必须单独通过构建、核心冒烟和静态自检后才能进入下一批。实现前需建立正式变更
记录并确认重大变更的排期；本计划本身不创建代码分支，也不执行代码。

### Phase 0：锁定规则与迁移基线

**目标**：固定当前行为、名词、公式边界和唯一写入者。

任务：

1. 记录 `DamageType`、`DamageRelation`、`DamagePayload`、`DamageRequest`、Receipt 和
   拒绝原因的字段/枚举契约。
2. 生成当前 Projectile 路径调用图，确认所有 `LifeComponent` 写入点，区分复活/治疗与伤害。
3. 固定敌我关系矩阵和中立目标策略。
4. 锁定本节攻击倍率、类型倍率、Critical、Defense、ArmorPenetration 与最终减伤顺序。
5. 记录当前 `ProjectileDamageComponent`、`ProjectileHitCandidate` 和
   `AcceptedHitReceipt` 的迁移映射。

验收：每个状态有唯一 owner；关系矩阵有冒烟覆盖；公式系数使用具名常数；高级护盾、反射和
元素抗性保留为后续工作。

### Phase 1：共享 Combat 值模型与纯规则

**目标**：新增不依赖 Projectile 的值类型和确定性计算。

任务：

1. 新增 `DamageType`、`DamageRelation`、`DamageCause` 和请求/结果状态类型。
2. 新增 `DamagePayload`、`DamageRequest`、`DamageAppliedReceipt`。
3. 新增 `DamageRelationSystem` 的纯规则入口和关系拒绝原因。
4. 将 `DamageMitigationSystem` 拆为可测试的攻击贡献、类型映射和目标减伤步骤。
5. 不修改目标 Life，不移除旧 Projectile API。

验收：纯计算覆盖所有阵营组合、Owner/自伤策略、零/极限伤害、非法枚举、伤害类型映射、
减伤 clamp、Defense/ArmorPenetration 边界；主项目构建通过。

### Phase 2：通用 Damage Commit 核心

**目标**：让 Combat 能处理不带 Projectile 类型的通用请求。

任务：

1. 让 `DamageAdmissionSystem` 接受 `DamageRequest`，删除对 `ProjectileHitCandidate` 的核心
   依赖。
2. 让 `DamageCommitSystem` 接受通用请求和计算结果，只保留 `LifeComponent` 写入责任。
3. 将 `AcceptedHitReceipt` 迁移为 `DamageAppliedReceipt`；保留一个短期只读适配器以降低
   Projectile 批次风险。
4. 实现 `CombatDamageSystem` 的请求收集、稳定排序、一次提交和结果分发边界。
5. 以来源本地 `RequestId` 去重，并为目标生命快照冲突和来源失效定义确定性结果。

验收：非 Projectile 请求完成准入、关系、减伤、Life 提交和 receipt；相同 RequestId 不会重复
扣血；失败不会写入 Life；只有 `DamageCommitSystem` 写入伤害路径中的 Life。

### Phase 3：Projectile 适配迁移

**目标**：保持现有 Projectile 行为，同时改用共享 Combat 入口。

任务：

1. 将 `ProjectileDamageComponent` 映射到共享 `DamagePayload`；保留兼容构造器直到调用者迁移。
2. `ProjectileCollisionSystem` 只负责接触事实、Projectile 策略和候选转换，不负责敌我最终
   判定或生命提交。
3. 将 `ProjectileHitCandidate` 转换为 `DamageRequest`，填入 Projectile cause、source/target
   handle、ExpectedTargetLife、接触信息和 DamageType。
4. 删除已被通用 Combat 请求入口取代的 `ProjectileCombatCommitSystem` 专用提交路径。
5. 让 `ProjectileHitResolutionSystem` 消费 `DamageAppliedReceipt`，继续处理命中免疫、穿透
   和终止。
6. 确认 `ProjectileTerminationCommitSystem` 仍是实际删除 owner。

验收：普通 Projectile 命中 NPC 的伤害值与迁移前一致；关系拒绝、保护窗口、目标版本冲突和
零伤害不会消耗穿透；成功命中才记录免疫和穿透；穿透耗尽才进入 PendingTerminate。

### Phase 4：Player 伤害来源接入

**目标**：Player 的权威攻击可以构造通用伤害请求，不引入客户端组件。

任务：

1. 定义服务器侧 Player 攻击意图的最小输入：目标、攻击定义/物品来源、伤害载荷、Tick 和
   必要的 Inventory/Attributes 快照。
2. 由 Player/Inventory/Item 适配器生成 `DamageRequest`；不能直接写目标 Life。
3. 从 Player 的 `AttributeSetComponent` 读取伤害贡献，从目标 Attributes 读取防御/减伤。
4. 增加 Player 成功/拒绝结果消费，明确冷却、资源和弹药提交时机。
5. 明确近战接触需要 Physics ContactFact，远程攻击可通过 Projectile 或其他授权来源进入
   同一入口。

验收：Player 对 Hostile NPC 可以造成伤害；Player 对 Friendly Player 默认拒绝；同一 Player
   攻击请求重复提交不会重复扣血；攻击失败不消耗成功命中结果；没有任何客户端控制组件被
   加回 NSSLC。

### Phase 5：NPC 伤害来源接入

**目标**：NPC 的 AI 攻击阶段可以产生通用敌对伤害。

任务：

1. 扩展现有 `NpcAiIntent`/`NpcBehaviorStateComponent` 的攻击适配，使 `BeginCombatAction`
   只表达意图，不直接修改目标。
2. 为 NPC 攻击定义提供 DamageType、基础伤害、关系和攻击距离/接触事实要求。
3. 由 NPC Combat 适配器生成 `DamageRequest`，统一走关系、Attributes、保护窗口和 Life 提交。
4. 由 NPC 结果消费者在 receipt 成功后推进攻击阶段和冷却。
5. 保留 NPC 的行为状态、目标关系和生命状态为独立组件，不创建 `NpcEntity` 聚合对象。

验收：Hostile NPC 可对 Friendly Player 造成一次伤害；同阵营 NPC 默认拒绝；Player 保护窗口
   生效；目标死亡或版本变化时请求被拒绝；NPC 只有在 receipt 成功后才认为攻击命中。

### Phase 6：宿主装配、旧接口收敛与完整验收

**目标**：把共享 Combat 系统装配到本地 ECS Tick，并删除重复 Projectile 责任。

任务：

1. 在不依赖文件枚举顺序的前提下，为 `EcsServerPhase` 注册 Preparation、Simulation、Commit
   的实际系统实例。
2. 明确 Combat 请求 buffer 的每 Tick 清空点、异常处理和重入禁止规则。
3. 删除已迁移到共享 Combat 的 Projectile 专用关系/计算提交路径。
4. 删除 `AcceptedHitReceipt` 和 Projectile 专用准入结果；保留 `ProjectileDamageComponent` 作为
   Projectile 向共享 DamagePayload 的输入适配来源。
5. 补齐本地 Player/NPC/Projectile 端到端冒烟和静态 owner 扫描。

验收：实际注册的 Tick 可以运行三类来源的通用伤害；目标 Life 只有一个伤害写入 owner；
Projectile、Player、NPC 都能消费统一 receipt；旧 Projectile 专用核心依赖为零；网络和
Infrastructure 目录没有被引入。

## 8. 验收与验证矩阵

### 8.1 纯规则验证

- Friendly → Hostile 的 Hostile damage 允许。
- Hostile → Friendly 的 Hostile damage 允许。
- 同阵营 Hostile damage 拒绝。
- Neutral 来源默认拒绝，显式环境策略按规则表处理。
- Owner/self hit 按来源策略处理。
- `DamageType` 到 Attributes 的映射完整且未知类型拒绝。
- 攻击者 Damage/类型倍率、目标 Defense/ArmorPenetration、DamageReduction/Endurance 的
  边界结果可重复。
- 零伤害、最小生命、最大整数和非有限数值不破坏 Life 不变量。

### 8.2 Arch 集成验证

- Source/Target foreign world、stale generation、terminating entity 被拒绝。
- ExpectedTargetLife 冲突不写入生命。
- ProtectionWindow 阻止伤害且不消费 Projectile 穿透。
- 同 Tick 重复请求按明确策略只产生一次有效提交。
- DamageCommit 失败不产生成功 receipt。
- DamageCommit 成功后 receipt 的 before/after life 正确。
- 只有 Combat 生命提交 owner 写入目标 Life。

### 8.3 三类来源闭环

- Projectile → Hostile NPC：伤害生效、穿透减少、命中免疫和终止正确。
- Player → Hostile NPC：同一 DamageRequest/Commit/Receipt 链生效。
- Hostile NPC → Friendly Player：同一链生效，并由 NPC 结果消费者推进攻击状态。
- Player/NPC/Projectile 对同阵营目标默认拒绝。
- 目标死亡、保护窗口、目标版本变化和来源销毁都不会误扣生命。

### 8.4 静态与构建门禁

每批使用受影响项目的增量构建，不执行 solution 级 Rebuild。至少检查：

```text
dotnet build src/NSSLC/Terraria.Components.csproj --no-restore
dotnet build .agent-workplace/buff-smoke/Smoke.csproj --no-restore
dotnet run --project .agent-workplace/buff-smoke/Smoke.csproj --no-build --no-restore
git diff --check
```

若新增 Player/NPC 专属验证项目，应只构建实际受影响项目，并记录 exit code、warning/error
数量和 `Build/bin/` 输出路径。生产源码扫描必须证明：

```text
DamageCommitSystem 是伤害路径唯一 Life writer
ProjectileCollisionSystem 不再承担最终敌我关系
Combat 核心不依赖 ProjectileHitCandidate
没有 HarmReductionComponent、PlayerDefenseStateComponent 等重复数值组件
没有客户端控制组件
```

## 9. 风险、技术债与处理方式

| 风险 | 影响 | 处理 |
| --- | --- | --- |
| 数值公式未定 | 伤害结果无法验收 | Phase 0 先固定公式和边界；未定规则阻塞实现 |
| 同 Tick 多来源竞争 | 重复扣血或顺序漂移 | ExpectedTargetLife、显式排序和重复策略 |
| 关系规则复制 | Player/NPC/Projectile 结果不一致 | 统一 `DamageRelationSystem`，来源只提供载荷 |
| Attributes 快照过期 | 结果依赖隐式时序 | 请求捕获时明确读取边界；提交时只重验生命周期/生命版本 |
| Projectile 兼容迁移 | 大范围一次改动 | Phase 3 保留只读适配器，先保持结果再删除旧类型 |
| 连续 Sweep 尚未完成 | 高速弹幕漏判 | 记录为 Physics 独立技术债，不用 Combat 规则掩盖 |
| 宿主未注册 | 代码存在但运行不执行 | Phase 6 强制检查真实 `EcsServer` 注册闭包 |
| 保护窗口语义冲突 | 命中结果不一致 | 保护窗口只作为准入条件，成功 receipt 后才消费来源状态 |
| 复活/治疗与伤害并发 | Life writer 冲突 | 在宿主 Commit 阶段固定顺序，保留各自 owner，不合并成万能系统 |

## 10. 完成定义

只有同时满足以下条件，才能把计划标记为完成：

1. Player、NPC、Projectile 均能通过共享 `DamageRequest` 进入同一个 Combat 提交入口。
2. `DamageType`、`DamageRelation` 和 Attributes 计算规则有明确值语义和边界验证。
3. 敌对关系矩阵由一个共享规则入口决定，来源系统不再复制敌我判定。
4. `DamageCommitSystem` 是伤害路径中目标 `LifeComponent` 的唯一常规写入者；复活系统保留独立 owner。
5. 成功和拒绝结果均可被来源领域区分；Projectile、Player、NPC 只在成功 receipt 后提交
   各自后续状态。
6. Projectile 的穿透、免疫和终止行为与迁移前等价，且不回写 Combat 的职责。
7. 同 Tick 重复、目标版本冲突、保护窗口、来源/目标销毁和 foreign handle 均有明确结果。
8. 实际 `EcsServer` Tick 注册已通过本地端到端冒烟；不能只以类型存在或单元测试通过代替。
9. 相关生产项目按构建约束通过，输出位于 `Build/bin/`；未修改 `src/NSSLC.Infrastructure/`。
10. `ProjectileHitCandidate`、`ProjectileDamageComponent` 不再作为共享 Combat 核心的必需类型；
    `AcceptedHitReceipt` 与 Projectile 专用准入结果已移除。

## 11. 暂不纳入本计划

- 网络伤害包、客户端预测、表现事件和出站复制。
- 伤害转移和 Boss 特殊规则仍不在范围内；元素抗性、护盾、反射、吸血和随机暴击已通过
  `DamagePayload`、Attributes 和 receipt 契约扩展。
- Projectile 连续 Sweep 的完整空间算法；只保留接入所需的事实接口和技术债。
- 全量复刻最新客户端或 MinimalImplementation 的类层次、数组和服务架构。

## 12. 相关文档

- [Projectile 实现说明](../reviews/audits/2026-10-25-projectile-implementation.md)
- [Player、NPC、Projectile 共享组件合并](../reviews/audits/2026-10-26-shared-component-consolidation.md)
- [生命组件边界](../cr/CR-2026-10-23-life-component-boundary.md)
- [ECS System 拆分技能](../../.agents/skills/ecs-system/SKILL.md)

## 实施结果（2026-10-26）

- Phase 0–6 核心切片已实现；Player 攻击意图、NPC AI 近身攻击和 Projectile 命中都进入同一
  `CombatDamageRequestSystem`，并通过 `DamageAppliedReceipt` 或具名拒绝原因反馈。
- 共享 `EntityRuntimeRegistry` 负责所有实体 handle；Projectile registry 只保留终止历史元数据。
- 生产构建、Buff/Projectile smoke 与共享 Combat smoke 均通过；Combat smoke 覆盖 Player、NPC AI、
  Projectile、关系拒绝、无接触、公式边界、请求排序和重复 RequestId。
- `PlayerAttackRequestSystem` 提供服务器攻击意图入口；具体物品/Inventory 使用工作流需由游戏宿主
  提交意图，本组件库没有现成的物品使用 application 流程可接入。
- 连续 Projectile Sweep、复杂伤害转移和网络仍按范围排除。`PlayerDeathRespawnSystem`
  继续拥有复活时的 Life 写入；伤害 Life 写入只由 `DamageCommitSystem` 承担。

## 高级伤害属性扩展（2026-10-26）

用户随后要求将元素抗性、护盾、反射、吸血和随机暴击全部纳入 Attributes 计算边界。该扩展已完成：

- `DamageElement`、元素穿透和元素抗性键进入共享 Attributes/Combat 值模型。
- `CriticalChance` 与 `CriticalMultiplier` 由 `AttributeCombatCalculator` 解析；`CriticalRollInput` 将世界种子、Tick、来源、攻击定义和 RequestId 作为显式输入，Combat 不读取隐式全局随机源。
- `Shield` 表示当前护盾池，`MaxShield` 表示容量/修饰来源；`AttributeSystem.ConsumeShield` 是护盾消费入口，护盾完全吸收时仍产生带吸收量的成功回执。
- `ReflectRatio` 与 `LifeStealRatio` 根据实际 `LifeDamage` 计算结果值；Combat 不直接写攻击者生命，也不在本扩展中递归创建反射请求或治疗提交。
- `DamageMitigationResult` 和 `DamageAppliedReceipt` 暴露原始伤害、防御后、抗性后、护盾吸收、生命伤害、反射、吸血和暴击事实。

`True` 伤害跳过防御、元素抗性和最终减伤，但仍经过护盾；如果需要穿透护盾，必须以后增加显式 `DamagePayload` 策略。

