# Projectile 链路修复执行计划

**文档 ID**：PLAN-2026-11-25-PROJECTILE-CHAIN-REPAIR
**逻辑域**：plans
**产物类型**：plan
**策略**：sequential（P0 是硬前置；P1–P4 相互独立、可分批验收；P5 依赖 P1 的意图语义与 P2 的绑定期工厂）
**状态**：proposed（本轮只出计划，未改代码；批准后按 §4 逐批实施，每批独立提交）
**上游依据**：
[Projectile 组件系统补齐执行计划](2026-11-24-projectile-component-systems-execution-plan.md)、
[投射物内容与射击接缝 CR](../cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md)、
四份缺陷记录（见 §2.2）、
[AI 声明接口收口执行计划](2026-11-25-ai-declaration-interface-execution-plan.md)（绑定期工厂与声明目录键的边界）
**适用约束**：
[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、
[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)、
[组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)、
[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md)、
[构建与验证约束](../../Context/项目约束/构建与验证约束.md)、
[progress 信息保存约束](../../Context/项目约束/progress信息保存约束.md)、
[Test 验证项目组织](../../Test/README.md)
**范围**：`src/NSSLC/Projectile/**`、`src/NSSLC/Share/Physics/SpatialSimulation/**`、
`src/NSSLC/Share/Physics/ContactFact/**`、`src/NSSLC/Share/AI/AiEntitySystem.cs`（只补安装入口）、
`src/NSSLC/Share/Physics/Movement/Command/MovementIntent.cs`、`src/NSSLC.Server/Program.cs` 与
`src/NSSLC.Server/Registration/**`、`Test/Terraria.Components/Terraria.Projectile/**`
**排除范围**：不改参考客户端 `Projectile.cs`；不改 `src/NSSLC.Infrastructure/**`；不改协议包号与载荷格式；
不做投射物网络投影（§13.3）；不接客户端物品使用输入与瞄准方向（§13.1、§13.2）；不开 P5 内容菜单

---

## 1. 执行目标

把 2026-11-24 已接通、但**带着四条未闭合缺陷**的投射物链路，修到「语义唯一、参数生效、终止唯一、
可被断言固定」的状态。判定口径五条，缺一条不算达成：

1. **速度守恒**：追踪弹转向不掉速，`|speed - 出厂速度| < 0.05`，且与转向频率无关（连续 40 tick 转向
   后仍成立）。断言不得再用实测值当容差。
2. **参数生效**：同一行为类型下，两个 `ProfileId` 的不同 `turnRate` / `range` / `maximumSpeed`
   各自生效；profile 数值为 0 时才回落到能力组件。不匹配的 profile 与行为类型组合在建服期失败。
3. **语义唯一**：接触事实的子步语义只有一种表达，且由断言固定；不允许「瓦片有子步、实体恒 0」
   这种同名不同义。
4. **终止唯一**：同句柄同 tick 的第二条终止请求被队列内部吸收（不靠调用方自觉），原因按集中定义的
   优先级取高者；端到端用例断言 `Drain(tick).Count == 1`。
5. **有证据**：每批给出构建命令、项目、退出码、警告/错误数、产物路径与验证项目 PASS 行；
   未执行项一律记 unknown。缺陷闭合后按 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md)
   §3.3 删除对应 `docs/defects/` 文件。

本计划**不做**新能力：不做网络投影、不做客户端输入、不做 P5 内容族。它只修「已经接通但不正确」的部分，
外加两处会持续制造新缺陷的接缝（§2.2 第 5、6 条）。

## 2. 当前基线与证据等级

> 证据等级：代码 + 缺陷记录 + 2026-11-25 的验证项目实测（`Terraria.Projectile` 87 条 PASS / 0 FAIL）。
> 没有真实客户端联调证据——客户端输入与网络投影都不在本计划内（§13）。

### 2.1 已成立（有代码与实测证据）

| 能力 | 证据 |
| --- | --- |
| 生成链路 | `PlayerItemUseFactQueue`（Simulation 产事实）→ `ProjectileItemUseSpawnSystem`（Preparation，`before: combat.projectile.spawn`）→ `ProjectileSpawnRequests` → `ProjectileSpawnRequestSystem`（Preparation，`CombatSystemRegistration:55-56`）→ `ProjectileLifecycleSystem.TrySpawn`；代价是使用事实到弹体差一 tick，与验收口径一致 |
| 内容与定义 | 4 条真实内容经 LVCS 加载（`wooden-arrow`、`homing-spark`、`bouncer-orb`、`gem-bolt`，同在 `projectile.json`）；`ProjectileCatalog` 封存后可 `TryGet` |
| AI 运行入口 | `Program.cs:711-734`：`ProjectileAiRegistration.CreateSystem` + `ProjectileAiSnapshotProvider` + `ProjectileTargetAcquisitionSystem` + `ProjectileAiAdapter`，均在 Preparation，适配器排在位移意图刷新与目标获取之后 |
| 子步与瓦片接触 | `SubstepCapabilityComponent`（按 `extraUpdates + 1` 装配）、`SpatialCommit.Substep`、`TileContactFact` + `TileContactQueueComponent` |
| 反弹与穿透预算 | `ProjectileBouncePolicyComponent` + `ProjectileTileResolutionSystem`（按 `ConsumeOrder` 扣预算并反射）+ `ProjectileHitResolutionSystem`（Applied 扣减 / Rejected 回滚） |
| 伤害链路 | 接触捕获 → `ProjectileDamageRequestSystem` → `CombatAttackIntentQueue` → `DamageCommitSystem` → `ProjectileDamageResultSystem` → `ProjectileHitResolutionSystem` |
| 验证面 | `Terraria.Projectile` 87 条 PASS；`Terraria.Npc.Ai`、`Terraria.Entity.Scheduling`、真实启动（`ecs.server.ready`、stderr 0 行）均绿 |

### 2.2 本计划要修的缺陷与接缝

| # | 项 | 现象（证据） | 闭合物 |
| --- | --- | --- | --- |
| F1 | 追踪转向速率单调衰减 | AI 侧把模长归一化到**当前**速度，空间侧再逼近一次弦，每帧净缩一步；断言被写成 `6.5 < speed <= 8.05` | [DEF](../defects/2026-11-24-projectile-homing-speed-drift.md) |
| F2 | profile 数值对多 profile 内容不生效 | 行为里**已有**「profile 值为 0 就回落能力组件」的规则（`ProjectileHomingAiBehavior:51-66`），但实例来自注册工厂里闭包的那一个 `homing.default`：`AiSystem` 的工厂键是 `Type`，适配器只传 `BehaviorType`，`ProjectileAiRegistration.Bind` 无调用点（死代码）。3 条 profile 的数值全是 0，所以缺陷没显形 | [DEF](../defects/2026-11-24-projectile-homing-profile-values-not-applied.md) |
| F3 | 实体接触子步号恒为 0 | `PhysicsContactCaptureSystem` 在位移完成后整体捕获一次，拿不到子步循环变量；瓦片接触却是真实子步号 | [DEF](../defects/2026-11-24-projectile-entity-contact-substep-always-zero.md) |
| F4 | 同帧双路径耗尽的终止唯一性靠隐式守卫 | 瓦片路径显式查 `Contains`，命中路径靠 `CheckAdmission` 顺带查；`Enqueue` 不去重，原因优先级未定义 | [DEF](../defects/2026-11-24-projectile-termination-uniqueness-implicit.md) |
| F5 | 意图接缝两侧不一致 | NPC 走 `MovementIntentRequestQueue`（Movement 域安装），Projectile 的 `ProjectileAiAdapter` 直接写实体上的 `MovementIntent` | `Projectile/ProjectileAiAdapter.cs` 的 `World.Get<MovementIntent>(entity) = ...` |
| F6 | 一条平行的位移实现无人使用 | `ProjectileSimulationSystem` + `ProjectileSimulationResult` + `ProjectileSimulationStatus` + `ProjectileMotionResult` 零生产消费者，README 与 Test 注释仍称它为「唯一实现子步的代码」 | 全仓 grep 仅命中自身、README、`docs/reviews/audits/2026-11-02-*` 与 `Test/.../Program.cs:297` 的注释 |
| F7 | 射击接缝路由失败不可观测 | 定义未知 / owner 不可用只体现在 `Results` 上，宿主未接进运行期日志 | [CR §5](../cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md) 第 1 条 |

### 2.3 明确不在本计划（写入 §13，各自立项）

- 客户端输入缺口：`PlayerItemUseRequest` 没有生产者，端到端「真客户端开火」无法闭环；
- 瞄准方向未建模：协议没有可用朝向（包 13 只有位置与速度），射击方向暂由内容声明；
- 投射物网络投影：客户看不到弹体，联调前必做，但属新增能力；
- P5 内容菜单：命中附加效果、反射/吸血落账、召唤与鞭击、切草、液体响应、钓鱼钩爪。

## 3. 决策记录

### D-1 速度权威：意图持有模长，物理只限转向

**结论**：`MovementIntent` 增一枚语义枚举 `MagnitudeAuthority { Acceleration, Intent }`，默认 `Acceleration`
（保持既有 NPC / 玩家行为不变）。`Intent` 的含义是「加速度只限制转向速率，模长由意图决定」：
`SpatialSimulationSystem` 在自由速度解析时先做弦逼近，再把模长恢复为 `desired.Length()`。

**理由**：缺陷的两次逼近里，第二次发生在空间侧（`SpatialSimulationSystem.cs` 的自由速度解析），
AI 侧无论怎么补都补不回来——测试读的是位移之后的 `Velocity`。把「模长归谁」显式写成意图上的一个字段，
比让内容用 `acceleration = 0` 去编码物理语义更诚实：那样静止弹（`desired = 0`）还要另一套分支。

**被否方案**：

| 方案 | 否因 |
| --- | --- |
| AI 侧归一化到出厂速度 | 空间侧的弦照样缩短，断言读不到 8；且出厂速度存在 profile 里，会变成第二真源 |
| 内容把 `acceleration` 设 0，空间直接采纳 `desired` | 用参数编码语义；静止/悬停弹需要另一条分支；`acceleration = 0` 现有语义是「不加速」而不是「立刻采纳」 |
| 空间侧一律恢复模长 | 会改变 NPC 与玩家的加速手感，破坏既有验证断言 |

### D-2 profile 参数化：绑定期按 profile 造实例

**结论**：给 `AiEntitySystem` 补一个「带工厂的安装入口」`Attach(entity, in AiComponent, Func<AiBehavior<TIntent>>)`，
内部走已存在的 `AiSystem.Bind(component, factory)`；`ProjectileAiRegistration.CreateSystem` 只按类型注册，
`ProjectileAiRegistration.Bind` 成为唯一按 profile 造实例的路径并被适配器真正调用。
**不扩 `AiSystem` 的工厂键，也不删 profile 的数值字段。**

**理由**：`AiSystem.Bind(component, factory)` 早就存在并校验产物类型（类型不符即抛），缺的只是
`AiEntitySystem` 没有把外部 binding 装进自己字典的入口——这才是 `Bind` 变成死代码的原因。
声明目录的键仍是 `Type`（AI 声明接口计划 §12 的既定决策），把 `(Type, ProfileId)` 塞进 `AiSystem`
会造出第二套键，而 profile 维度本来就属于「构造」，不属于「登记」。

**被否方案**：

| 方案 | 否因 |
| --- | --- |
| `AiSystem` 工厂键扩为 `(Type, ProfileId)` | 与声明目录的 `Type` 键形成双真源；NPC 路径没有 profile 消费者，白付复杂度 |
| 删除 profile 的 `maximumSpeed/turnRate/range` 与 `AiBehaviorProfileId`（缺陷记录当时的倾向） | 内容侧需要按条调这三个数；删掉等于把参数化推回代码，且 `(BehaviorId, ProfileId)` 目录键与内容 JSON 已按此结构落地 |

### D-3 实体接触子步：删字段，不做子步化捕获

**结论**：删除 `PhysicsContactFact.Substep`，实体接触明确为「位移完成后整体检测」语义；
`TileContactFact.Substep` 保留并成为唯一带子步语义的接触事实。

**理由**：实体碰撞检测在架构上本来就不在子步循环里；把它搬进循环要处理「迭代查询内不得结构变更」，
成本高，而当前唯一读取该字段的是队列去重比较，不承担语义。删字段消除歧义，比造一个伪子步便宜。

**被否方案**：把 `EntityCollisionSystem.FindContacts` 搬进子步循环——与结构变更约束冲突，且没有消费方需要该精度。

### D-4 终止唯一性：队列内建幂等 + 原因优先级表

**结论**：`ProjectileTerminationQueueComponent.Enqueue` 同句柄同 tick 幂等（已存在则返回既有 `requestId`，
不追加）；`ProjectileTerminationReason` 旁增一张集中优先级表，提交阶段按它归并；新增端到端用例。
**不改命中路径的守卫**——它经 `CheckAdmission` 已经守住，缺陷记录 §10 也明确提示不要按「命中缺守卫」去修。

**理由**：「本 tick 已判定终止」这个事实只存在于队列的 `Contains` 里，权威被切成两半。把幂等放进队列，
新增第三个终止生产者时就不会静默破坏唯一性；优先级表让「原因取谁」有唯一答案。

### D-5 意图接缝：Projectile 改走请求队列

**结论**：`ProjectileAiAdapter` 不再直写 `MovementIntent`，改为入 `MovementIntentRequestQueue`，
与 NPC 侧同一条接缝；本 tick 内由 `MovementIntentRequestSystem` 安装，仍早于空间求解。

**理由**：`MovementIntent` 归 Movement 域所有，「一个域边界只留一个写者」这条在 NPC 侧已经付过代价
（见 `MovementIntentRequest` 的类注释）。两条接缝并存时，任何「谁最后写」的 bug 都要看两处。

**代价与前置**：管线顺序变化（Preparation 入队 → Simulation 安装），必须先有 P1 的意图语义才能保证
「同一 tick 的意图与求解一致」。因此 P5 排在 P1 之后。

## 4. 批次与状态

| 批次 | 状态 | 前置 | 交付目标 | 判据 |
| --- | --- | --- | --- | --- |
| P0 基线复核与红测 | 未开工 | 无 | 四条缺陷先失败，证明断言钉得住 | §5.2 的期望失败逐条出现 |
| P1 速度权威（F1） | 未开工 | P0 | 转向不掉速，单一模长权威 | §1 第 1 条 |
| P2 profile 参数化（F2） | 未开工 | P0 | 两个 profile 各自生效，错配建服期失败 | §1 第 2 条 |
| P3 子步语义（F3） | 未开工 | P0 | 子步语义只剩一种并被断言固定 | §1 第 3 条 |
| P4 终止唯一性（F4） | 未开工 | P0 | 队列幂等 + 原因优先级 + 端到端用例 | §1 第 4 条 |
| P5 接缝对齐与死代码（F5–F7） | 未开工 | P1、P2 | 意图走队列；删平行实现；路由失败可观测 | §10.3 |

依赖图：

```text
P0 ──┬──→ P1 ──┐
     ├──→ P2 ──┴──→ P5
     ├──→ P3
     └──→ P4
```
## 5. P0 基线复核与红测

### 5.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P0-1 | 记录基线：两项目构建 + 六个验证项目 + 真实启动 20 s，填进 §11（没跑不算基线） | — |
| P0-2 | 收紧 `homing-keeps-speed-within-tolerance` 到 `abs(speed - 8) < 0.05`，删掉注释里「实测 7.0」的免责话术 | `Test/.../Terraria.Projectile/Program.cs` |
| P0-3 | 新增 `termination-enqueue-idempotent`：同句柄同 tick 连续 `Enqueue` 两次，第二次返回既有 id 且 `Count` 不变 | 同上 |
| P0-4 | 新增 `entity-contact-fact-substep-semantics`：按 D-3，落地后断言类型不再带 `Substep`；落地前先断言「恒 0 且唯一读取方是去重比较」 | 同上 |
| P0-5 | 新增 `profile-values-applied`：两条 `ProfileId` 不同、`turnRate` 不同的追踪 profile，断言各自生效 | 同上 |
| P0-6 | 新增 `intent-magnitude-authority`：断言 `MovementIntent` 默认 authority 是 `Acceleration`，保护 NPC / 玩家既有行为 | 同上 |

### 5.2 期望失败（先红）

| 断言 | 期望 | 失败原因 |
| --- | --- | --- |
| `homing-keeps-speed-within-tolerance` | FAIL | 实测 `speed ≈ 7.000`，偏差 1.0 ≫ 0.05 |
| `profile-values-applied` | FAIL | 两个 profile 拿到同一个工厂实例，数值不生效 |
| `termination-enqueue-idempotent` | FAIL | `Enqueue` 无条件追加 |
| `entity-contact-fact-substep-semantics` | FAIL | 字段存在且恒 0，属「同名不同义」 |

> 任何一条没有按预期变红，说明断言没钉住缺陷：先改断言，再进下一批。这是本计划唯一允许「先改测试」的地方。

### 5.3 退出条件

- 基线六个验证项目全绿并记录在 §11；
- 四条新断言 + 一条收紧断言按 §5.2 全部失败；
- 本批只动断言文件，不动 `src/`。

## 6. P1 速度权威（F1）

### 6.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P1-1 | `MovementIntent` 增 `MagnitudeAuthority { Acceleration, Intent }`，默认 `Acceleration`，未定义值抛 | `Share/Physics/Movement/Command/MovementIntent.cs` |
| P1-2 | 自由速度解析：`authority == Intent` 且 `HasDesiredVelocity` 时，弦逼近后把模长恢复为 `desired.Length()` | `Share/Physics/SpatialSimulation/SpatialSimulationSystem.cs`（自由速度两处，约 :170 / :185） |
| P1-3 | 追踪行为只做一次方向旋转：删掉「按当前速度再归一化」那步，输出满模长方向；同时改掉 :69-70 那段把缺陷当结论的注释 | `Projectile/System/ProjectileHomingAiBehavior.cs:67-76` |
| P1-4 | 适配器提交 `MaintainVelocity` / `AdjustVelocity` 意图时置 `MagnitudeAuthority.Intent` | `Projectile/ProjectileAiAdapter.cs` |
| P1-5 | 断言：收紧后的 `homing-keeps-speed-within-tolerance` 转绿；新增 `homing-speed-invariant-over-40-ticks` | `Test/.../Terraria.Projectile/Program.cs` |

### 6.2 语义表（写进 `MovementIntent` 注释与 README）

| authority | 弦逼近 | 模长 | 使用者 |
| --- | --- | --- | --- |
| `Acceleration`（默认） | 做 | 由加速度决定，可能被弦缩短 | NPC、玩家输入（既有行为） |
| `Intent` | 只用于限转向速率 | 恢复为意图模长 | 投射物 AI（追踪、直线） |

### 6.3 验证

- `Terraria.Projectile` 全绿，含两条速率断言；
- `Terraria.Npc.Ai`、`Terraria.Entity.Scheduling` 无回归（默认值不改变既有路径）；
- 真实启动 20 s：`ecs.server.ready`、stderr 0 行；
- **反向证据**：把 P1-2 的模长恢复临时注掉，`homing-speed-invariant-over-40-ticks` 必须重新失败，证明修复是载荷。

### 6.4 退出条件

§1 第 1 条成立，且反向证据已记录。

### 6.5 风险

| 风险 | 规避 |
| --- | --- |
| 共享物理语义改动波及 NPC / 玩家 | 默认值保持 `Acceleration`；本批必须跑 `Npc.Ai` + `Entity.Scheduling` 对照 |
| 恢复模长后高度差的 Y 分量被放大 | 只恢复 `desired.Length()`，方向仍来自弦结果；直线弹的 `desired` 与当前一致，等效于不变 |

## 7. P2 profile 参数化（F2）

### 7.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P2-1 | `AiEntitySystem.Attach(entity, in AiComponent, Func<AiBehavior<TIntent>>)`：内部走 `AiSystem.Bind(component, factory)`；已存在绑定则抛 | `Share/AI/AiEntitySystem.cs` |
| P2-2 | `CreateSystem` 只按类型注册（直线、追踪），去掉单 profile 闭包；`Bind` 保留为唯一按 profile 造实例的入口 | `Projectile/System/ProjectileAiRegistration.cs` |
| P2-3 | 适配器惰性补挂时带 profile 工厂：从目录解析 `(BehaviorId, ProfileId)` → 工厂用该 profile 构造 | `Projectile/ProjectileAiAdapter.cs:81-128` |
| P2-4 | 组合根新增第二个追踪 profile（`turnRate` 非 0，证明确实生效），`homing-spark` 仍走 `homing.default`；README 固化「profile 值为 0 回落能力组件」这条既有规则 | `Program.cs:576-609` 的 `CreateProjectileBehaviorProfiles`、`Projectile/README.md` |
| P2-5 | 断言：`profile-values-applied` 转绿；新增 `profile-behavior-mismatch-rejected`（profile 声明类型与工厂产物不符 → 构造期抛） | `Test/.../Terraria.Projectile/Program.cs` |

### 7.2 验证

- 两条 profile 的 `turnRate` 不同的情况下，同一 tick 数内转向角度不同（用速度方向夹角断言，不比较内部状态）；
- 目录里查不到的 `(BehaviorId, ProfileId)` 不猜类型：现有「不挂 AI」的分支保留并加断言；
- `Terraria.Npc.Ai` 无回归（`AiEntitySystem` 只是多一个重载）。

### 7.3 退出条件

§1 第 2 条成立；`ProjectileAiRegistration.Bind` 从死代码变成唯一入口（grep 有调用点）。

### 7.4 风险

| 风险 | 规避 |
| --- | --- |
| 每个实体一份行为实例，profile 工厂被频繁调用 | 工厂只在首次补挂时调用一次（`_pendingAttachments` 路径），不做每 tick 构造 |
| 删掉 `CreateSystem` 的单 profile 闭包后，追踪行为拿到 `null` profile | 工厂参数为 `profile`，由 P2-3 从目录解析；解析失败走「不挂 AI」分支并记诊断 |

## 8. P3 子步语义（F3）

### 8.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P3-1 | 删除 `PhysicsContactFact.Substep` 与对应 ctor 校验 | `Share/Physics/ContactFact/System/PhysicsContactFact.cs` |
| P3-2 | 队列去重比较改为 `(first, second, tick)`，同 tick 同对只留一条 | `Share/Physics/ContactFact/Component/PhysicsContactQueueComponent.cs` |
| P3-3 | `TileContactFact.Substep` 注释写死为「唯一带子步语义的接触事实」 | `Share/Physics/ContactFact/System/TileContactFact.cs` |
| P3-4 | 去掉 `substep: 0` 实参，注释说明「实体接触在位移完成后整体检测，不做子步化」及理由 | `Share/Physics/ContactFact/PhysicsContactCaptureSystem.cs` |
| P3-5 | 断言：`entity-contact-fact-substep-semantics` 转绿（反射断言类型无该属性）；瓦片接触的 `substep == 1` 断言保留 | `Test/.../Terraria.Projectile/Program.cs` |

### 8.2 验证

- 编译期暴露所有读取方（删字段即报错，不允许留在代码里「读不到就跳过」）；
- 实体接触的既有伤害链路断言（命中一次、护盾吸收、免疫）无回归；
- 瓦片接触按 `ConsumeOrder` 的顺序断言无回归。

### 8.3 退出条件

§1 第 3 条成立；`docs/defects/2026-11-24-projectile-entity-contact-substep-always-zero.md` 可删除。

## 9. P4 终止唯一性（F4）

### 9.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P4-1 | `Enqueue` 幂等：同句柄同 tick 已存在 → 返回既有 `requestId`，不追加 | `Projectile/Component/ProjectileTerminationQueueComponent.cs:38-53` |
| P4-2 | 新增 `ProjectileTerminationPriority`（静态优先级表 + 比较器），`ProjectileTerminationReason` 枚举值不变 | `Projectile/System/ProjectileTerminationReason.cs` 旁 |
| P4-3 | 提交阶段按优先级归并同 tick 多条（防御性；正常已被 P4-1 吸收） | `ProjectileTerminationCommitSystem` |
| P4-4 | 端到端用例 `termination-single-per-tick`：同帧注入 `Applied` 回执 + 瓦片接触事实，断言 `Drain(tick).Count == 1`、原因 = 高优先级者、实体只销毁一次 | `Test/.../Terraria.Projectile/Program.cs` |
| P4-5 | 两处 `Contains` 守卫保留，注释指向队列幂等（不再扮演唯一性权威） | `ProjectileTileResolutionSystem` / `ProjectileHitResolutionSystem` |

### 9.2 原因优先级（草案，实施时按枚举补齐）

| 优先级 | 原因 | 依据 |
| --- | --- | --- |
| 高 | 命中耗尽、瓦片耗尽 | 玩家行为或物理接触导致，掉落与归因需要 |
| 中 | 寿命到期、owner 移除、敌意清除 | 由时间或生命周期推进导致 |
| 低 | 管理性清理 | 兜底，不携带玩法信息 |

> 优先级表是唯一权威，**不再靠枚举声明顺序**隐含高低。

### 9.3 验证

- `termination-enqueue-idempotent` 与 `termination-single-per-tick` 转绿；
- 既有 `termination-projects-reason` 类断言无回归；
- `requestId` 语义：每句柄每 tick 至多一个新 id，旧 id 稳定复用（用例断言两次取到同一个 id）。

### 9.4 退出条件

§1 第 4 条成立；`docs/defects/2026-11-24-projectile-termination-uniqueness-implicit.md` 可删除。

## 10. P5 接缝对齐与死代码（F5–F7）

### 10.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P5-1 | 投射物 AI 意图改入 `MovementIntentRequestQueue`，不再直写 `MovementIntent`；时序：适配器（Preparation）入队 → `MovementIntentRequestSystem`（Simulation）安装 → 空间求解 | `Projectile/ProjectileAiAdapter.cs`、`Program.cs`、`Registration/EcsCoordinationRules.rules`（如需顺序条目） |
| P5-2 | 删除 `ProjectileSimulationSystem` / `ProjectileSimulationResult` / `ProjectileSimulationStatus` / `ProjectileMotionResult`；删除前 grep 复核零引用 | `src/NSSLC/Projectile/` |
| P5-3 | 修正 `Projectile/README.md` 与 `Test/.../Program.cs:286-300` 中「唯一实现子步的代码」的过期陈述 | 两处 |
| P5-4 | 射击接缝路由失败进运行期日志：宿主每 tick 检查点读 spawn 结果并打 `ecs.projectile.spawn_rejected` | `Program.cs`、`Projectile/ProjectileItemUseSpawnSystem.cs` |
| P5-5 | README 更新组合模型与系统清单（含意图接缝改为请求队列、profile 参数化口径） | `Projectile/README.md` |

### 10.2 验证

- P5-1 后 `homing` 两条断言仍绿（换接缝不改语义）；
- `git grep "World.Get<MovementIntent>"` 零命中，除 Movement 域自身安装点；
- 四个死类型删除后全仓编译绿；
- 日志证据：临时把一条内容的 `shoot.projectile` 改成未知 id，启动后可看到一条 `spawn_rejected`，随后还原内容并复跑确认无残留。

### 10.3 退出条件

F5、F6、F7 三条闭合：意图只有一个写者、平行实现删除、路由失败可观测；README 与代码一致。

## 11. 验证与证据

每批必须填满本表，未执行的写 `unknown`，不填通过（[构建与验证约束](../../Context/项目约束/构建与验证约束.md)）。

| 批次 | 命令 | 退出码 | 警告 | 错误 | 产物 / 结果 |
| --- | --- | --- | --- | --- | --- |
| P0 | `dotnet build src/NSSLC/Terraria.Components.csproj` | | | | |
| P0 | `dotnet build src/NSSLC.Server/NSSLC.Server.csproj` | | | | |
| P0 | `dotnet run --project Test/Terraria.Components/Terraria.Projectile --no-build --no-restore` | | | | |
| P0 | `Terraria.Npc.Ai` / `Terraria.Entity.Scheduling` / `Terraria.Entity.Definition` | | | | |
| P0 | 真实启动 20 s（`NSSLC_WORLD_FILE=src/World/科研.wld`） | | | | |
| P1 | 同上全组（重点：两条速率断言 + NPC 回归） | | | | |
| P2 | 同上全组（重点：`profile-values-applied`） | | | | |
| P3 | 同上全组（重点：接触与伤害链路） | | | | |
| P4 | 同上全组（重点：终止唯一性用例） | | | | |
| P5 | 同上全组（重点：换接缝后速率断言 + 日志证据） | | | | |

## 12. 回滚

| 批次 | 回滚方式 | 数据影响 |
| --- | --- | --- |
| P0 | 恢复旧断言与旧注释 | 无 |
| P1 | 删除 `MagnitudeAuthority`，恢复空间解析与追踪行为的两处归一化 | 无（意图是每 tick 临时值，不落盘） |
| P2 | 恢复 `CreateSystem` 的单 profile 闭包与适配器的 `Type` 绑定，删除 `Attach` 重载 | 无 |
| P3 | 恢复 `PhysicsContactFact.Substep` 与 ctor 校验、恢复 `substep: 0` 实参 | 无；**唯一破坏性改动**是删除公开字段，回滚需同步所有读取方（当前为零） |
| P4 | 恢复 `Enqueue` 无条件追加，删除优先级表 | 无 |
| P5 | 恢复直写 `MovementIntent`、`git checkout` 四个被删类型、撤回日志检查点 | 无 |

各批独立提交、逐批回退；五批都不改变组件布局以外的协议、存档与网络载荷。

## 13. 未闭合（本计划完成后仍在）

1. **客户端输入缺口**：`PlayerItemUseRequest` 没有生产者，Communication 层未把客户端物品使用输入翻译成请求。
   闭合条件：真实客户端按住使用键时，服务端出现 `PlayerItemUseRequest` 并生成弹体。单独立项。
2. **瞄准方向未建模**：协议没有可用朝向（包 13 只有位置与速度），射击方向暂由内容声明。
   闭合条件：协议补朝向后由输入驱动，且内容不再声明方向。
3. **投射物网络投影未开工**：客户看不到弹体，联调前必做。闭合条件：`ProjectileNetworkProjectionSystem` 落地并有出站包断言。
4. **缺陷文件删除是闭合动作**：F1–F4 的 `docs/defects/` 文件只在 §1 五条口径全部满足后删除，未满足就保留并更新 §9 状态历史。
5. **P5 内容菜单**（命中附加效果、反射与吸血落账、召唤与鞭击、切草、液体响应、钓鱼钩爪）按需逐项立项。
6. **内容组合的深层改造**（拆掉融合定义与中央装配表）是另一份计划：[Projectile 内容组合分解计划](2026-11-25-projectile-content-composition-decomposition-plan.md)，本计划 P5-2 是它的先决条件。

## 14. 风险与规避

| 风险 | 触发条件 | 规避 |
| --- | --- | --- |
| 共享物理语义改动波及 NPC / 玩家 | P1 恢复模长无差别生效 | 用默认值把新语义限定在 `Intent`；每批跑 NPC 与调度验证项目 |
| profile 工厂与行为类型错配被吞掉 | 适配器解析失败静默不挂 AI | 已存在 `Bind` 的类型校验；P2-5 断言错配必须抛 |
| 队列幂等改变 `requestId` 语义 | 复用旧 id 让下游以为「同一请求」 | 语义就是「同 tick 同句柄最多一条」，用例固定两次取到同一 id |
| 删字段被外部读取方依赖 | 有人跨程序集读 `Substep` | 删字段由编译期暴露；本仓库单程序集，无跨程序集消费者 |
| 删死代码破坏验证注释 | 注释仍称其为唯一子步实现 | P5-2 与 P5-3 同批，先改名再删类型 |
| 注册顺序变化引入丢请求 | P5-1 改接缝 | 用 `EcsCoordinationRules.rules` 的键名条目固定顺序，不用下标；真实启动对照 |
| 「先红后绿」被跳过 | 直接改实现让断言顺便变绿 | §5.3 明确 P0 只改断言；P1–P4 每批的验证里要求反向证据或失败快照 |

## 15. 设计取舍记录

| 决策 | 采用 | 替代方案 | 理由 |
| --- | --- | --- | --- |
| 速度权威 | 意图带 `MagnitudeAuthority` | 内容把 `acceleration` 设 0；AI 侧补模长 | 语义显式、默认不变、静止弹不用额外分支 |
| profile 参数 | 绑定期工厂 | `AiSystem` 双键；删 profile 数值 | 无双真源、不丢参数化、声明目录键不动 |
| 实体接触子步 | 删字段 | 子步化捕获 | 结构变更约束 + 无消费方 + 消除同名不同义 |
| 终止唯一性 | 队列内建幂等 + 优先级表 | 调用方各自守卫 | 新增生产者不会静默破坏；原因取谁有唯一答案 |
| 意图接缝 | 请求队列（与 NPC 同构） | 保留直写 | 一个域边界一个写者，减少「谁最后写」的排查面 |
| 死代码 | 删除并改注释 | 保留为参考实现 | 与空间管线重复，且注释已过期，留着会误导下一个维护者 |
| 缺陷归档 | 关闭即删文件 | 转归档目录 | 仓库既定规则：缺陷只放未闭合项 |
