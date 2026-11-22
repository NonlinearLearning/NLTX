# Version4 战斗、状态与归因系统代码盘点及组件拆分报告

> 范围：`docs/Version4权威游戏模拟系统主要子系统.md` 第 11 项“战斗、状态与归因”。本报告遵循
> `public-decomposition` 的证据优先流程：先记录原始成员、读写者和生命周期，再提出 ECS
> 组件、System、Query、Command、Adapter 与 Projection 的边界。它是拆分设计和迁移前盘点，
> **不是** Version4 已实现、已编译或已通过回归的声明。

## 1. 结论与边界

`Player.Hurt`、`Player.KillMe`、`NPC.StrikeNPC`、`Projectile.Damage` 与
`NPCDamageTracker` 当前将资格判定、数值计算、状态写入、移动、UI/音效、网络、掉落、死亡、
随机数和伤害贡献混在同一调用链中。按字段数量拆开会把强耦合的“本次命中”再拆碎；合适的
分解单位是：

1. 每个受击实体的长期权威状态；
2. 一次命中的不可变候选和确定性结算结果；
3. 会修改实体或世界的结算/生命周期 System；
4. 只读的资格、减伤、效果准入与死亡资格 Query；
5. 归因、随机、网络和表现等外部边界。

本报告提出 `CombatAndStatus` 作为能力域，而不是 `PlayerCombat`、`NpcCombat` 或一个同时
保存所有战斗字段的 `CombatState`。`Player`、`NPC` 与 `Projectile` 通过组合使用该能力；
它们特有的复活、消失、AI、槽位、物品和轨迹仍留在原来的领域。

```text
Projectile / ItemUse / NPC AI / Tile hazard
        |  DamageIntent + ProvenanceSnapshot
        v
HitEligibilityQuery -> DamageResolutionQuery -> DamageResolutionSystem
        |                      |                       |
        |                      |                       +--> Health / Immunity / Status write
        |                      |                       +--> KnockbackImpulseCommand
        |                      |                       +--> DamageResolvedFact
        |                      v
        |                DeathEligibilityQuery
        v                      |
  rejected result              v
                  PlayerDeathResolutionSystem / NpcDeathResolutionSystem
                                      |
                   DeathResolvedFact + AttributionSnapshot
                         |                 |              |
                         v                 v              v
               EncounterCreditSystem   world progress   network/presentation projections
```

唯一权威写入为状态组件、显式命令和各实体生命周期 System。网络包、`CombatText`、音效、粒子、
本地客户端随机效果与旧数组槽位不是新的权威状态。

## 2. 范围、来源与证据状态

### 2.1 路径更正与只读来源

调用请求中的 `D:\TRbackup\Version4参考` 在本机不存在；同级实际存在且被既有 Version4
设计文档指定为回退源码的是 `D:\TRbackup\Version4`，因此本报告以该目录为真实 Terraria
实现来源。该目录和 `C:\Users\shan\Downloads\ECS\space-station-14-master` 均只读，本报告
没有复制其中代码。

| Source | Version / role | Query and useful evidence | Stop reason |
| --- | --- | --- | --- |
| `D:\TRbackup\Version4` | 目标的真实实现 | `Terraria/Player.cs:968-1033,1355-1383,3541-3731,4300,10847,11239,22208-22585,22572-22690`; `Terraria/NPC.cs:5897-6071,6111-6443,64571,67461-67628,76385-76480`; `Terraria/Projectile.cs:90-258,11519-12520,13228-13261`; `Terraria.GameContent/NPCDamageTracker.cs:129-310`; `Terraria.DataStructures/PlayerDeathReason.cs:6-170` | 获得字段、直接写入、调用链和已知空实现；不能由此确认尚未出现的 hook 消费者。 |
| `D:\TRbackup\tmodloader-api-docs-stable` | tModLoader `v2026.07` 本地 API 镜像；公开语义佐证 | `index.html` 标题/页眉；`class_player.html` 的 `HurtInfo`、`HurtModifiers`、`AddBuff`、`Hurt`、`KillMe` 与 `HealEffect` 实际锚点；`class_n_p_c.html` 和 `class_projectile.html` 的命中 API | 仅用于公开 API 语义；不用于推断 Version4 的私有顺序或网络行为。 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | SS14 ECS 组织模式参考 | `Content.Shared/Damage/Components/DamageableComponent.cs`; `Damage/Systems/DamageableSystem.cs`; `StatusEffectNew/Components/{StatusEffectContainerComponent,StatusEffectComponent}.cs`; `StatusEffectNew/StatusEffectsSystem.cs`; `Content.Server/Destructible/{DestructibleComponent,DestructibleSystem}.cs` | 只吸收“状态由组件所有、System 写入、事件/阈值在写后消费”的边界模式；不复用其命名、代码或游戏语义。 |

### 2.2 真实代码中的入口清单

| Entry / data | 主要读者和写者 | 生命周期 / side effects | Status |
| --- | --- | --- | --- |
| `Player.statLife`, `statLifeMax*`, `statDefense`, `lifeRegen*` | 字段 `Player.cs:1355-1373`；`Hurt` 减少生命并重置再生 `:22224-22335`；`UpdateLifeRegen` 推进再生 `:10847` | 玩家存续；旧路径还同时改 UI、位移和免疫 | confirmed |
| `Player.immune`, `immuneTime`, `hurtCooldowns` | 声明 `:968-976,2475`；`Hurt` 根据 cooldown 资格拒绝并写计时器 `:22221-22225,22325-22334` | 命中窗口；按普通/命中键不同 | confirmed |
| `Player.buffType`, `buffTime`, `buffImmune` | 声明 `:1029-1033`；`AddBuff`、`DelBuff`、`ClearBuff` `:3541-3731`；`UpdateBuffs` `:4300` | 玩家存续、每 tick；数组顺序与淘汰规则可观察 | confirmed |
| `NPC.life`, `lifeMax`, `defense`, `damage`, `immortal`, `dontTakeDamage`, `knockBackResist` | 声明 `NPC.cs:6111-6443`；`StrikeNPC` 结算并更新贡献 `:67461-67628`；`checkDead` `:64571` | NPC 存续；死亡可能让槽位可重用 | confirmed |
| `NPC.buffType`, `buffTime`, `buffImmune` | 声明 `:6067-6071`；`AddBuff`/删除/网络发送 `:76385-76480` | NPC 存续；和网络发送耦合 | confirmed |
| `Projectile.damage`, `knockBack`, `owner`, `penetrate`, local/static 命中免疫 | 声明 `Projectile.cs:126-258`；`Damage` 和 `Damage_PVE` 的全 NPC 扫描、资格和 `StrikeNPC` `:11519-12520` | 投射物存续；命中会修改穿透、伤害和免疫 | confirmed |
| `PlayerDeathReason` | 工厂封装来源索引/类型 `PlayerDeathReason.cs:35-70`；死亡文本 `:73-82`；二进制编码/解码 `:84-170` | 单次死亡快照和协议 DTO；当前混用数组槽位索引 | confirmed |
| `IEntitySource` 与 `AEntitySource_OnHit` | 生成/命中来源的标记接口和攻受实体引用，`AEntitySource_OnHit.cs:3-13` | 出生、掉落、命中链传递 | confirmed |
| `NPCDamageTracker` | 把 `realLife` 归一到真实 NPC，创建/累积/最近记录 `NPCDamageTracker.cs:129-310`；`BossDamageTracker` 的活动和类型归属 `BossDamageTracker.cs:37-85` | 世界临时静态账本，按 tick 清理 | confirmed |
| `InvasionDamageTracker.IncludeDamageFor` / `CheckActive` | `InvasionDamageTracker.cs:32-35` 为空体 | 入侵贡献资格及停止条件不可从该实现确认 | partial |
| `AllowShimmerDodge` | `Player.cs:22212` 调用，但 `:22568-22571` 恒构造默认 `bool` | 会影响命中资格，语义未实现 | partial |
| NPC 死亡后的掉落、成就、复仇、进度提交 | 投射物保存 `NPCKillAttempt` 后调用 `Player.OnKillNPC`，`Projectile.cs:12510-12515`；既有设计盘点显示 `Player.OnKillNPC` 为空 | 消费链无法由当前来源闭合 | missing |

`NPCDamageTracker.AddDamage` 在目标 `life` 扣减前以 `Math.Min(damage, npc.life)` 记账
(`NPCDamageTracker.cs:178-195`)；因此迁移时贡献记录的输入必须是**已接受且已裁剪的实际伤害**，
不是初始攻击值或 UI 数字。

## 3. 成员归属表

下表是拟迁移的状态/行为归属，不表示这些 C# 类型已经存在。`confirmed` 仅表示源字段和基础
写路径有证据，不表示所有 mod hook、协议消费者或随机分支已经闭合。

| 原始成员 / 行为 | 状态性质 | Candidate | 主要读者 / 唯一写者 | 生命周期 | Evidence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `statLife`, `statLifeMax`, `statLifeMax2`; NPC `life`, `lifeMax` | 实例权威 | `HealthStateComponent` | 伤害、治疗、再生 / `DamageResolutionSystem`、`HealingSystem`、`RegenerationSystem` | entity create to death/despawn | Player `1357-1373,22224`; NPC `6341-6343,67461` | confirmed |
| `lifeRegen`, `lifeRegenCount`, `lifeRegenTime`; NPC 对应再生字段 | 实例权威、积算器 | `HealthRegenerationStateComponent` | 再生规则 / `RegenerationSystem` | entity lifetime；死亡重置由 lifecycle | Player `1369-1373,22335,10847`; NPC `6111-6115` | confirmed |
| `statDefense`, NPC `defense`, `armorPenetration`, endurance 类修正 | 派生的本 tick 战斗输入 | `DamageMitigationStateComponent` 和 `DamageOffenseSnapshot` | 装备、Buff、定义 / 属性重算 System；结算只读 | 重新计算后到 tick 结束 | Player `1351-1355,22229-22273`; NPC `6323-6327` | confirmed |
| `immune`, `immuneTime`, `hurtCooldowns`; NPC/player/projectile局部命中免疫 | 实例权威 | `HitImmunityComponent` | 命中资格 / `DamageResolutionSystem`、`ImmunityExpirySystem` | 到期/命中/销毁 | Player `968-976,2475,22221-22334`; Projectile `160-162,256-258,11579-11615` | confirmed |
| `noKnockback`, `knockBackResist`; 受击后直接写 velocity | 权威的规则输入；速度本身属 Movement | `KnockbackPolicyComponent` + `KnockbackImpulseCommand` | 命中结算 / `KnockbackApplicationSystem` | 实体存续；命令单 tick | Player `1383,22342-22347`; NPC `6359,67461+` | confirmed |
| `buffType`, `buffTime`, `buffImmune` | 实例权威；顺序有语义 | `StatusEffectSlotsComponent` + `StatusEffectImmunityComponent` | 效果资格、属性重算 / `StatusEffectSystem` | entity lifetime；每 tick 过期 | Player `1029-1033,3541-3731`; NPC `6067-6071,76385-76480` | confirmed |
| Buff 的 ID 定义、淘汰、可叠加、难度时长、宠物/近战互斥 | 内容定义 + 纯规则 | `StatusEffectDefinitionCatalog`; `StatusEffectAdmissionQuery` | Status System 只读 | catalog lifetime | Player `3545-3695`; NPC `76389-76435` | confirmed |
| `damage`, `knockBack`, `owner`, `penetrate`; `Damage()` 的碰撞候选 | 其他领域的实体状态；战斗输入快照 | `ProjectileDamageState`、`ProjectileOwnerState`、`ProjectileHitImmunityState` 留在 Projectile；`DamageIntent` 进入 Combat | Projectile System 产生命中意图；Combat 只消费快照 | projectile lifetime / one resolution | Projectile `126-258,11519-11615,12510-12520` | confirmed |
| `PlayerDeathReason` 的玩家/NPC/投射物/物品/其他/文本字段 | 不可变归因与协议投影 | `DamageAttributionSnapshot`; `PlayerDeathReasonProjection` | damage/death System 生产；网络/文本只读 | 单个 resolution/death | `PlayerDeathReason.cs:6-170` | confirmed |
| `IEntitySource`, parent/on-hit 关系 | 生成/命中因果链 | `EntityProvenanceComponent`; `ProvenanceSnapshot` | spawn/item/projectile systems 生产；Combat 读取 | entity lifetime / snapshot at command | `AEntitySource_OnHit.cs:3-13`; `EntitySource_*.cs` | confirmed |
| 玩家/世界的累计伤害、最后命中时间、最近追踪 | 临时权威账本，不是 Boss 进度 | `EncounterDamageCreditComponent` 或 session-owned `EncounterDamageCreditLedger` | credit system / only `EncounterCreditSystem` | encounter start to close/expiry | `NPCDamageTracker.cs:58-73,178-310` | confirmed |
| `dead`, `deadTime`, `respawnTimer`, `active`，掉落、墓碑、重生 | 生命周期权威 | 留在 `PlayerSpawnAndRespawn` / `NpcSpawnAiTown`；通过 `DeathResolvedFact` 接入 | 对应 lifecycle System | death to respawn/despawn | Player `1134-1136,22572-22690`; NPC `5897,64571` | confirmed |
| `CombatText`, 音效、粉尘、粒子、`NetMessage.SendData` | 外部效果 / 投影 | `CombatPresentationProjection`; `CombatReplicationProjection` | 投影 System | transient, retry policy needs definition | Player `3478-3492,22322-22323,22348+`; NPC `76436-76439` | confirmed as non-authoritative |

## 4. 拆分后的模块、接口和 seam

建议目录以领域优先组织为 `dome/src/Terraria.Dome.Simulation/Combat/`。初始迁移只在该领域
平铺核心类型；当 Effects、Attribution 或 Projection 各自形成稳定的独立测试/迁移单元后，再
按实际规模建立对应子目录。一个公共类型一个同名 PascalCase 文件，且不以文件顺序安排运行。

| Module | Interface / minimal contract | Implementation / ownership | Seam, depth and leverage |
| --- | --- | --- | --- |
| `HealthStateComponent` | `Current`, `Maximum`; 不公开可变集合 | `Combat` 域中的玩家/NPC 共用生命权威状态 | `DamageResolution` 与 `HealingCommand` 是唯一的生命突变 seam；可替换计算而不碰实体分类。 |
| `HealthRegenerationStateComponent` | `Accumulated`, `Delay`, `ExpectedLossPerSecond` | 生命再生积分；不混入魔力或 Buff 槽位 | `RegenerationRuleQuery(input) -> HealthDelta` 是纯函数；处理不同更新频率。 |
| `DamageMitigationStateComponent` | `Defense`, `Endurance`, `DamageReductionRulesVersion` | 本 tick 由装备、状态和 NPC 定义重建的读取快照 | `DamageResolutionQuery` 只接收快照，防止结算读取全局 `Main`。 |
| `HitImmunityComponent` | `CanAccept(HitKey, Tick)`, `RegisterAccepted(HitKey, UntilTick)` | 通用/按攻击者/按投射物类型的命中窗口 | 减伤和免疫不同：前者改变伤害，后者决定是否接受；独立 seam 允许聚焦测试槽位复用。 |
| `KnockbackPolicyComponent` | `CanReceive`, `Resistance` | 受击资格与抗性；不存位置或速度 | `KnockbackImpulseCommand` 跨到 Movement；Combat 不直接写 `VelocityComponent`。 |
| `StatusEffectSlotsComponent` | 有序 `StatusEffectSlot` 快照；`Revision` | 保留原始有序槽位、空槽、替换和压缩语义 | 不能把每条效果都机械变成实体，除非需求要求独立网络身份或跨实体附着；数组顺序有淘汰语义。 |
| `StatusEffectImmunityComponent` | `IsImmune(StatusEffectId)` | Buff 免疫能力，与命中免疫独立 | `StatusEffectAdmissionQuery` 纯读；消除当前 `AddBuff` 中的直接网络发送。 |
| `DamageIntent` | `Target`, `Payload`, `HitKey`, `Knockback`, `Attribution`, `SourceTick`, `IntentId` | 不可变命令，来自 Projectile/Item/NPC/环境 Adapter | 跨领域输入统一在这里；`IntentId` 为重放/去重准备，但去重策略需协议设计确认。 |
| `DamageResolutionQuery` | `Resolve(intent, target state, rules) -> DamageResolution` | 纯计算：资格、暴击、穿透、防御、减伤、实际伤害、免疫写入建议 | 深模块：输入明确、输出不泄漏实体内部；使用固定 `CombatRandomSnapshot`，不读 `Main.rand`。 |
| `DamageResolutionSystem` | `Apply(DamageIntent) -> DamageResolvedFact / DamageRejectedFact` | 依序写 Health、HitImmunity、状态后果，发出击退命令和事实 | 唯一普通伤害写边界；效果排序显式化。 |
| `StatusEffectSystem` | `Apply(StatusEffectApplyCommand)`, `Expire(Tick)` | 写效果槽位、发出属性重算请求 | 状态定义目录、资格 Query 与状态写入分离。 |
| `RegenerationSystem` | `Tick(HealthRegenerationStateComponent)` | 只应用已计算的生命变化 | 不触及命中免疫、Buff、死亡或 UI。 |
| `PlayerDeathResolutionSystem` | `Resolve(PlayerDeathCandidate)` | 生成 respawn、库存掉落/墓碑、玩家死亡记录等命令；不做随机表现 | 与 NPC 死亡系统分开，防止“通用死亡组件”吞没复活/掉落差异。 |
| `NpcDeathResolutionSystem` | `Resolve(NpcDeathCandidate)` | 提交 despawn、掉落/进度/成就请求及可归因死亡事实 | 当前 `OnKillNPC` 消费链为空，具体后续命令必须先补证据。 |
| `EntityProvenanceComponent` | `RootCause`, `ParentEntity`, `SpawnCause`, `DefinitionRef` | 实体创建时写入、创建后只读或只追加可审计版本 | 实体 ID、网络 ID、持久 ID 与外部 ID 分列；不可用一个字符串或槽位替代。 |
| `DamageAttributionSnapshot` | `Attacker`, `DirectSource`, `RootCause`, `Item/ProjectileDefinition`, `EnvironmentCause` | 在 intent 建立时冻结，death 时再复制到结果 | 槽位重用前冻结，避免 `PlayerDeathReason.ByProjectile` 从可变数组回读错误对象。 |
| `EncounterCreditSystem` | `Record(DamageResolvedFact)`, `Close(EncounterClosedFact)` | 对 boss 根实体或会话中的遭遇账本写入实际伤害与参与者 | 账本不直接写世界 `downedBoss`/入侵旗标；只发已结算贡献事实。 |
| `ICombatRandomSource` | `Next(CombatRandomRequest) -> CombatRandomOutcome` | 调度器注入，支持录制/重放 | 隔离 `Main.rand`；随机结果写入 intent/result 以便服务器重演。 |
| `CombatReplicationProjection` | `Project(DamageResolvedFact / StatusEffectChangedFact / DeathResolvedFact)` | network adapter，编码明确 DTO | 网络发送失败不可回滚游戏状态；确认投递/重试语义后才能实现。 |
| `CombatPresentationProjection` | `Project(...) -> presentation events` | client/UI/audio/particle adapter | `CombatText`、音效、粉尘只在状态成功提交后消费事实，绝不反写生命或 Buff。 |

### 4.1 不变量

1. `HealthStateComponent.Current` 只能由治疗、再生和伤害的专用 System 改写；任何网络或 UI
   Adapter 只能发命令或读投影。
2. 被拒绝的 `DamageIntent` 不写生命、击退、伤害贡献或死亡事实。可选择记录
   `DamageRejectedFact` 供诊断，但不产生伤害数字。
3. `DamageResolvedFact.AppliedDamage` 是唯一可以进入 `EncounterCreditSystem` 的数值，且必须
   已裁剪到结算前剩余生命，保持现有 `NPCDamageTracker` 语义。
4. 每次实体死亡只有一个 `DeathResolvedFact`；必须以稳定实体身份和生命周期 revision 去重，
   不能用 `Main.npc[index]` 槽位作为长期主键。
5. `StatusEffectSlotsComponent` 中的排序、互斥、刷新、淘汰和压缩在迁移初期属于兼容语义；
   不因换成 ECS 而改变 `DelBuff` 的可观察槽位行为。
6. `DamageAttributionSnapshot` 在命中候选创建时冻结，读取其内部字段不接触可变实体数组。

## 5. System 顺序和副作用登记

```text
1. Projectile / Item / NPC / environment systems create DamageIntent
2. HitEligibilityQuery rejects invalid geometry, relation, immunity or invulnerability
3. DamageResolutionQuery computes the deterministic result from explicit snapshots
4. DamageResolutionSystem commits Health, immunity, status secondary commands
5. KnockbackApplicationSystem commits Movement impulse
6. RegenerationSystem and StatusEffectSystem run at their declared tick points
7. DeathEligibilityQuery emits a candidate after health commit
8. PlayerDeathResolutionSystem or NpcDeathResolutionSystem commits lifecycle commands
9. EncounterCreditSystem consumes applied-damage and death facts
10. replication and presentation projections consume committed facts only
```

步骤 4 必须早于步骤 7；步骤 8 必须早于会依赖“实体仍存在”的掉落、成就或世界进度提交；步骤
9 的贡献写入必须使用步骤 4 的结果。步骤 10 可在同一 tick 末尾执行，但其失败、重复或延迟不能
影响步骤 1--9。默认不并行执行共享目标相同的命中结算；只有 Query 输入和写集证明无交集后，
调度器才可并行。

| Boundary | Reads | Writes / effects | Failure and duplicate policy to define |
| --- | --- | --- | --- |
| `DamageResolutionSystem` | 明确状态快照、规则、随机结果 | Health、Immunity、状态命令、事实 | 同一 `IntentId` 至多一次；非法或过期 revision 拒绝。 |
| `DeathResolutionSystem` | death candidate、lifecycle revision、归因快照 | 实体生命周期与下游命令 | 每个 `(EntityId, LifecycleRevision)` 至多一次；下游失败须保留可重放事实。 |
| `EncounterCreditSystem` | `DamageResolvedFact`、遭遇资格 | 临时 credit ledger | 去重使用 intent/result ID；关闭后不再接受伤害。 |
| replication adapter | 已提交事实 | 网络包 | 不确定投递不回写权威状态；协议 sequence/ack 另行设计。 |
| presentation adapter | 已提交事实 | UI、音效、粒子 | 至少一次或允许丢失均不得改变模拟；客户端自行节流。 |

## 6. SS14 参考带来的组织结论

SS14 的 `DamageableComponent` 把可承受伤害的持久状态放在组件，`DamageableSystem` 集中改变它；
`DestructibleSystem` 订阅伤害已改变事件后才检查阈值并执行破坏，表明“伤害写入”和“死亡/破坏
后果”可以有清晰的先后边界。其 `StatusEffectContainerComponent` 与单个
`StatusEffectComponent` 也将容器关系、效果状态和生命周期 System 分开。

这些是可迁移的架构证据，而不是 API 模板：Terraria 的 Buff 固定槽位、互斥/淘汰、玩家复活、
NPC 槽位重用和服务器权威网络协议均不同。因此 Version4 初期保留 `StatusEffectSlotsComponent`
的有序槽位模型，不将每个 Buff 强制实体化；玩家和 NPC 的死亡也不采用一个通用的破坏阈值组件。

## 7. 明确不拆分、兼容与风险

| Item | Decision | Reason / compatibility policy |
| --- | --- | --- |
| `ProjectileDamageState`、`ProjectileOwnerState`、`ProjectileHitImmunityState` | 不迁入 Combat | 它们随投射物生成/穿透/销毁，且 Movement/Projectile 是主要写者；Combat 接收不可变 `DamageIntent`。 |
| 魔力、魔力再生和库存 | 不归入 CombatAndStatus | `Player.Hurt` 会影响魔力等资源，但资源所有权仍在 PlayerGameplay；通过 `ResourceChangeCommand` 协作。 |
| 位置、速度、mount、位移 | 不归入 Combat | Combat 只发击退 impulse；MovementPhysics 处理实际速度、碰撞和坐骑不变量。 |
| 世界 Boss/入侵进度 | 不归入 credit ledger | 归因层产生事实，WorldSession 决定进度；禁止直接写 `downedBoss` 或 invasion 字段。 |
| `CombatText`、音效、尘埃、粒子 | 不作为 component | 它们是表现效果，必须消费事实且可丢失；不在权威实体上保存历史。 |
| `PlayerDeathReason` 的现有二进制布局 | 先保持 Projection 兼容 | 将旧字段填充移至 `PlayerDeathReasonProjection`；领域模型以稳定实体/定义引用保存，避免槽位重用。 |
| 旧 `buffType[]/buffTime[]` | 迁移期间只读适配或单向写入 | 不允许新旧 Buff 权威双写；以 revision、镜像检查和一个提交方向逐步切换。 |

主要风险是行为纠缠而非类型数量：`Player.Hurt` 目前直接处理减伤、Buff 删除、资源回复、分摊伤害、
位移、声音、粒子和生命写入；`Projectile.Damage` 含大量类型特例和随机分支。第一批迁移应先
覆盖无特例的伤害资格/免疫/生命提交与确定性结果，遇到依赖 `Main.rand`、客户端本地玩家、全局
数组或空实现的分支必须留在 Adapter/compatibility path 并标记 `partial`，不能猜测替代语义。

## 8. focused verifier 计划与实际结果

本任务只进行只读源码盘点和文档设计，没有新增/移动 C# 或 ECS 文件，因此没有运行 `dotnet`
构建或测试。下面是实施每一迁移批次前应先落地的 focused verifier；当前结果均为**未验证**。

| Verifier | Arrange / act | Required assertions | Current result |
| --- | --- | --- | --- |
| `DamageResolutionEligibilityTests` | 普通/按键命中免疫、无敌、PVP、无效关系与槽位重用 | 被拒绝命中不改生命、击退、贡献和死亡事实 | 未验证 |
| `DamageMitigationTests` | 防御、耐久、暴击、最小伤害、护盾分摊 | 结果与冻结输入一致，实际伤害是唯一 commit 值 | 未验证 |
| `StatusEffectSlotCompatibilityTests` | 已存在、满槽、debuff、宠物/近战互斥、删除压缩、时间刷新 | 槽位排列、淘汰与 revision 符合兼容规则 | 未验证 |
| `ProjectileHitAttributionTests` | Owner、投射物、实体来源、穿透、local/static immunity | 每次已接受命中只产生一个 attribution snapshot；不读取已重用槽位 | 未验证 |
| `DeathIdempotencyTests` | 生命归零、重复 intent、死亡后延迟包、NPC 槽位重用 | 每个 lifecycle revision 恰好一个 death fact；掉落/进度请求不重复 | 未验证 |
| `EncounterCreditTests` | 玩家、世界伤害、boss 多节、生命周期结束 | 信用账本使用裁剪后实际伤害；不直接改世界进度 | 未验证 |
| `CombatProjectionIsolationTests` | 网络发送失败、UI/音效丢失和重复消费 | 权威 Health/Status/Death 不回滚、不重算、不被客户端写入 | 未验证 |
| `CombatRandomReplayTests` | 记录随机请求与结果并重放 | 同输入和随机记录产生相同 resolution 与后续命令 | 未验证 |

实施后，受影响项目应从仓库根通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，并在构建后以
`--no-build --no-restore` 运行上述对应项目的验证。报告成功时必须另行记录精确命令、项目、退出码、
警告/错误计数和 `Build/bin/` 产物路径；本报告不提前作出这些声明。

## 9. 最小迁移批次

1. 先引入纯 DTO：`DamageIntent`、`DamageResolution`、`DamageResolvedFact`、
   `DamageRejectedFact`、`DamageAttributionSnapshot`，并以 fixtures 建立资格和减伤差分测试。
2. 将 `HealthStateComponent`、`HitImmunityComponent` 和纯 `DamageResolutionQuery` 接入一个
   无投射物特例的 NPC/Player 命中路径；旧路径只作为单向 Adapter，禁止双写。
3. 分离 `KnockbackImpulseCommand`，由 Movement 处理速度写入；再迁移有序 Buff 槽位和每 tick
   过期/属性重算。
4. 在稳定实体 ID 与 lifecycle revision 就位后，迁移 `DamageAttributionSnapshot` 和
   `EncounterDamageCreditComponent`；先覆盖 Boss，入侵贡献保持 `partial` 直到其资格逻辑有证据。
5. 最后迁移 `PlayerDeathResolutionSystem` 与 `NpcDeathResolutionSystem`；在死亡后的掉落、成就、
   复仇、世界进度消费者补齐前，不删除旧 compatibility path。

每批都要记录旧字段源路径、目标文件、调用方变化、网络/存档影响和回滚方向。没有读写者闭合、
focused verifier 和串行构建证据时，不得把该批报告为完成。
