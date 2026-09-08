# Version4 ProjectileSimulation 独立只读审查与 ECS 拆分设计

> 本报告对应固定审查任务 taskNumber: 13。所有设计类型、接口、路径和伪代码均为 status: proposed，不表示已创建、已迁移或已验证。

## 0. 报告元数据

~~~text
subsystemId: ProjectileSimulation
taskNumber: 13
layer: authoritative-simulation
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-projectile-simulation-public-decomposition.md
verificationStatus: not-run
evidenceStatus: partial
nltxStatus: partial
~~~

审查范围以 D:\TRbackup\Version4 为行为基线；D:\TRbackup\无任何删减通过编译 仅用于补充 Version4 已存在文件的成员级缺口；tModLoader 文档仅用于公开 Hook 和网络调用边界交叉验证；Space Station 14 仅用于 ECS 粒度和事件/投影访问模式参考。本轮没有启动子代理，没有运行构建、测试或其他 compile-capable 命令，也没有修改生产代码、测试或源代码。

## 1. 执行摘要

ProjectileSimulation 在 Version4 中是一个真实存在且具有明确更新阶段的权威模拟子系统，但它的核心聚合 Projectile 是高度混合责任的对象，而不是可以按字段机械切成若干文件的天然 ECS 边界。Projectile 同时持有槽位和 identity、owner、轨迹和 AI 状态、生命周期、碰撞策略、穿透和免疫、伤害载荷、网络标志、trail 缓存，以及 fishing、minion、sentry、trap 等专用分支。

主要结论如下：

1. whoAmI、数组槽位、owner-scoped identity、projUUID、网络 replicationId 和 ECS runtime entity reference 必须保持不同概念。特别是 whoAmI/槽位不能被当作持久化 ID，identity 不能和 runtime entity ID 合并。
2. ai[]、localAI[] 和 aiStyle 是当前行为策略与兼容状态的组合。aiStyle 适合作为行为分发键，不是一级子系统；在每种专用行为的读写契约未闭合前，应保留 raw compatibility state，不应宣称已经全部 typed 化。
3. 碰撞几何应通过纯 ProjectileCollisionQuery 暴露；命中结果应通过 ProjectileHitCommand 交给 CombatAndStatus。ProjectileSimulation 不应直接写 NPC/Player 的生命、死亡或状态结算。
4. Kill() 不只是把 active 改为 false。它还撤销 owner identity 反查、取消 channel、生成 child projectile、触发掉落或其他世界效果、生成声音/粒子/Dust/Gore，并可能执行即时 Damage()。因此必须把结构性销毁、子生成、掉落、网络 tombstone 和表现副作用的顺序显式化。
5. Fishing bobber 是 Projectile 的轨迹/时间载体；捕获资格、FishingAttempt、掉落/敌对生成和 Item/NPC 事务属于 FishingAndCatchSimulation。LeashedEntity 拥有独立 registry、section activation、spawn/despawn、update 和网络流，不应并入 Projectile 生命周期。
6. 当前 NLTX 已有局部 ECS 数据组件和 dome 执行链，不能写成“没有实现”；但现有链条仍未证明覆盖 Version4 的全部 AI、Kill 事务、网络 owner+identity 重协调和 fishing/leashed 边界，故整体状态为 partial。

本报告提供可实施但不落地的 proposed Component、System、Query、Command、Adapter 和 Projection 设计，并把需最终整合会话裁决的共享 owner、事务边界和顺序列为 blocking-decision。

## 2. 子系统范围与不负责内容

### 2.1 负责内容

ProjectileSimulation 的候选职责边界是：

- Projectile entity 的轨迹、位置/速度更新协调和 extraUpdates 子步；
- aiStyle 行为分发和暂存的 raw AI compatibility state；
- 投射物与空间/Tile 几何的碰撞资格查询、反射/反弹候选和移动提交；
- Projectile-local lifetime、penetration、local/static/owner hit immunity 的状态提交；
- 产生命中、生成、销毁和复制请求；
- Projectile 特有的 sentry、minion、trap、bobber、counterweight 等关系或标志，但不吞并其相邻领域的最终事务；
- 维护与网络投影所需的权威快照输入。

### 2.2 明确不负责内容

| 边界对象 | 不归属 ProjectileSimulation 的部分 | 交互方式 |
|---|---|---|
| CombatAndStatus | NPC/Player health、死亡、buff、伤害结算、反射目标最终写入 | 接收 ProjectileHitCommand；共享 owner 为 integration-review |
| SpatialSimulation | Tile/World grid 存储、section 的空间索引和实体几何权威状态 | 提供纯 geometry query 或只读视图 |
| FishingAndCatchSimulation | FishingAttempt 资格、钓鱼结果、敌对生成、Item drop 事务 | Projectile 只提交 bobber context/事件候选 |
| LeashedEntitySimulation | leashed registry、section activation、anchor lifecycle 和独立网络模块 | 只保留经整合会话确认的 relation/reference |
| SpawnLifecycleAndLoot | entity create/destroy 的结构提交、掉落和 child entity 事务 | 消费 spawn/kill request |
| NetworkSessionAndSectionStreaming | message 编码、客户端连接、section/PVS、补发和协议方向 | 消费 replication projection |
| PlayerGameplay | 发射意图、owner 的武器/资源规则 | 产生 spawn request；不直接写 Projectile 内部状态 |
| Persistence | 存档 ID、恢复快照、跨会话生命周期 | 只在最终持久化契约确认后接入 |
| Rendering/Presentation | sprite、trail 绘制、Dust/Gore/Sound/UI | 消费 projection/effect command，不成为权威模拟 |

## 3. 证据优先级、来源角色与状态语义

证据优先级固定为：

~~~text
Version4
→ 完整可编译参考源码
→ tModLoader public API documentation
→ Space Station 14 ECS reference
~~~

来源角色如下：

- Version4：决定子系统是否存在、真实字段和调用链、权威写入者、生命周期和副作用。
- 完整可编译参考：只补充 Version4 已存在路径中的删减、空实现或邻近调用链缺口；不能把独有实现回填为 Version4 事实。
- tModLoader：只说明公开扩展 Hook 的时机、调用侧和 client/server 范围；不能替代 Terraria 私有实现。
- Space Station 14：只说明组件粒度、System/Event/Relation/Projection 的结构参考；不能推断 Terraria 行为或直接复制命名/目录。

本报告使用的状态含义：

- confirmed：在指定路径和行号看到相符符号与行为。
- partial：看到了局部事实，但实现、覆盖或调用链仍不闭合。
- missing：当前允许的证据范围内没有找到所需实现。
- unresolved：存在多个合理解释，需最终整合或用户裁决。
- evidence-mismatch：任务线索与实际符号/实现不一致。
- version-drift：来源版本与基线不一致，不能互相替代。

## 4. Version4 事实证据表

| ID | Version4 证据 | 读者/写者与生命周期 | 副作用与边界 | evidenceStatus |
|---|---|---|---|---|
| V4-PROJ-001 | D:\TRbackup\Version4\Terraria\Projectile.cs:32 定义 public class Projectile : Entity；90-270 集中声明 active、type、owner、ai/localAI、timeLeft、damage、penetrate、identity、network flags、trail、minion、tile collision、免疫和 fishing 缓存 | SetDefaults、NewProjectile、Update、Damage、Kill 共同读写；对象贯穿 slot 池生命周期 | 单个对象同时是运行实体、状态聚合、缓存和副作用入口 | confirmed |
| V4-PROJ-002 | Projectile.cs:444-556 的 SetDefaults(int Type) 重置 ai/localAI、player/NPC immunity、network 标志、identity、slot 相关字段、位置速度、damage、penetrate、timeLeft、flags，并按类型继续设置默认值 | 创建/复用槽位时调用；清理旧实例状态并建立新类型默认值 | 复用对象时必须避免旧类型状态泄漏；trail 数组按 definition 长度调整 | confirmed |
| V4-PROJ-003 | D:\TRbackup\Version4\Terraria\Main.cs:944-946 声明 Projectile[1001] projectile 与 int[256,1001] projectileIdentity；3480-3484 初始化槽位并赋 whoAmI | Main 持有固定数组；projectileIdentity[owner, identity] 用于反查 | slot、owner-scoped identity 和 runtime entity 不同；反查表不是持久化 ID | confirmed |
| V4-PROJ-004 | Projectile.cs:10227-10242 的 FindOldestProjectile() 扫描 1000 槽位，按 timeLeft 选择非 netImportant 的可复用槽位 | New spawn 和网络接收在无空槽时调用 | 回收策略是结构池规则，不能成为 Projectile 业务组件的隐式写入 | confirmed |
| V4-PROJ-005 | Projectile.cs:10244-10335 的 NewProjectile 设置 owner 默认值 255，查找空槽/最老槽，调用 SetDefaults，写 whoAmI、position、owner、velocity、damage、knockBack、identity=num，写 projectileIdentity[Owner,num]，应用 source/banner/minion 来源，并按 owner 随机写 AI | 发射入口写入几乎所有初始化字段；对类型和 source 有条件分支 | 同一入口混合 slot allocation、defaults、随机数、来源关系、初始轨迹和 identity；随机/UUID 应隔离到 Port/Adapter | confirmed |
| V4-PROJ-006 | Main.cs:11430 更新 Player；11511-11525 更新 NPC；11530-11549 调用 PreUpdateAllProjectiles 后顺序遍历 1000 槽位并调用 projectile[n].Update(n)；11550-11551 清理 index 并执行 post hook | 外层顺序是 Player → NPC → Projectile；Projectile 采用 slot index 顺序 | 不能依赖目录/文件顺序；未来 scheduler 必须显式声明该约束 | confirmed |
| V4-PROJ-007 | Projectile.cs:14693-14705 的 Update(int i) 检查 active、递减 immunity、设置 numUpdates=extraUpdates 并执行子步；14711-14730 处理越界/minion 回 owner；14736-14767 处理 minion/sentry 伤害和容量；18706-18716 的 AI() 按 aiStyle 分发 | 每个主 tick 可能执行多个 AI/移动子步；AI 读写 raw ai/localAI、position、velocity、damage、flags | AI dispatch 是策略选择和兼容分发键，不是独立权威状态根 | confirmed |
| V4-PROJ-008 | Projectile.cs:13654 Colliding(Rectangle, Rectangle) 支持普通矩形、flail/cone、trail/whip、lance 和特殊 type；例如 13735-13761 读 oldPos/oldRot 并执行线段几何，13885-13899 读 whip control points | Damage 路径调用；AI/位置/轨迹缓存影响几何 | 几何资格查询与状态 commit 应分开；不能让 Query 写 projectile、NPC 或 Tile | confirmed |
| V4-PROJ-009 | Projectile.cs:15513 HandleMovement(Vector2 wetVelocity)；15522-15592 读取 tileCollide、owner、pet、aiStyle、fall-through 等策略；15594-15601 继续按类型/样式改变 tile 规则；15948-15957 可能因边界/Tile 结果调用 Kill | Update 的 AI/环境阶段之后执行；读写 position、velocity、tile policy 和 kill 状态 | Tile geometry 属于 SpatialSimulation；反射、反弹、破坏 Tile、Kill 的命令顺序需显式化 | confirmed |
| V4-PROJ-010 | Projectile.cs:11480-11518 Damage_CanDealDamage 依据 type、aiStyle、ai/localAI、pet 等判断；11519-11538 的 Damage() 计算 hitbox 并调用 PVE/PVP | Update 中 Damage 在 lifetime decrement/Kill 之前执行 | damage eligibility、collision、immunity 和 target health 被挤在一个调用链；未来需拆为 Query + Command + Combat consumer | confirmed |
| V4-PROJ-011 | Projectile.cs:11554-11592 遍历 active NPC 并读取 local/static immunity；11594-11656 校验 friendly/hostile、owner hit check、trap/immortal 和 Colliding；11758-11767 可调用 NPC reflection | Damage_PVE_Inner 既读 NPC/Player，又写 projectile flags，并决定继续遍历 | 目标 eligibility 和 reflection 是跨域交互；不得在 ProjectileSimulation 内代替 Combat 写 NPC 状态 | confirmed |
| V4-PROJ-012 | Projectile.cs:11800-11803 某些 hit 会先 Kill()；12456-12520 触发命中效果并调用 StrikeNPC/TagEffect；12690 再次可能 Kill()；12698-12849 写 penetration、projectile/local/static/owner immunity 和 netUpdate | Hit sequence 可能在一次 Damage 内递减穿透、写目标免疫、创建 child 或结束 projectile | 命中提交与生命周期终止存在顺序依赖；必须使用稳定 hit sequence 和明确 commit 阶段 | confirmed |
| V4-PROJ-013 | Projectile.cs:15199-15227 更新 trail cache；15228-15240 处理 ADD2 turret、timeLeft--、timeLeft/penetrate 终止；15241-15269 处理 owner 网络更新、节流和 message 27；15271-15275 重发 skipped section 后清除 netUpdate | 同一 Update 尾部混合表现缓存、生命周期、网络节流和发送 | Projection 与 NetworkSession 不应回写模拟；trail 也不应成为一级领域根 | confirmed |
| V4-PROJ-014 | Projectile.cs:46425-46443 Kill() 先检查 active、清理 projectileIdentity[owner,identity]、把 timeLeft 置 0、按规则取消 channel；46445-46555 可停止声音、生成 spider/child projectile；大量后续分支生成 Dust/Gore/Sound/Particle，46653-46654 存在直接 active=false; return | Kill 可由 AI、碰撞、Damage、超时、网络接收触发；同一入口有结构、效果和网络可观察影响 | 不能简化为 active=false；需要 KillRequest、tombstone、child spawn、drop/effect 的顺序与幂等策略 | confirmed |
| V4-PROJ-015 | Projectile.cs:34073-34074 在 Version4 当前是空的 AI_061_FishingBobber/交付方法；47779-47786 仍有 bobber 检查和 AI_061_FishingBobber_GiveItemToPlayer 调用 | 运行时入口存在，但 fishing 行为/交付在该 Version4 文件中不闭合 | 完整参考只能补证，不能把完整实现宣布为 Version4 能力 | partial |
| V4-PROJ-016 | D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs:18-38 的 NetModule.Sync 写 full/delta、whoAmI、Type、AnchorPosition、NetSend 并按 section 广播；46-89 注册 prototypes；133-174 section Activate/Deactivate/Sync；234-306 更新、stream、remove 和虚拟生命周期 | Leashed registry/section 自己拥有 spawn/despawn/update/network 生命周期；233 Remove、290 StreamNetUpdates 当前为空 | 它是独立 boundary；不能归入 Projectile 的 registry 或 slot lifecycle | confirmed（空方法为 partial） |

## 5. 完整参考源码补证表

| ID | 完整参考来源 | 与 Version4 的匹配关系 | 补证事实 | 限制与状态 |
|---|---|---|---|---|
| REF-PROJ-001 | D:\TRbackup\无任何删减通过编译\Terraria\Projectile.cs:51236-51444，方法名 AI_061_FishingBobber | 路径、类型和方法签名匹配 Version4 Projectile.cs:34073 | 完整实现会设置 bobber timeLeft，检查 fishing pole/player 状态，处理水面/湿润状态，读 Fishing conditions，按时间和随机数触发 FishingCheck()，并写 ai/localAI/netUpdate | referenceStatus: full-reference-supplemented；Version4 对应位置为空，evidenceStatus: partial |
| REF-PROJ-002 | 同文件 :51496-51552，AI_061_FishingBobber_GiveItemToPlayer(Player,int) | 方法签名与 Version4 :34074 匹配 | 完整参考按 item type 和 fishing level 生成 stack，最后在 :51550 调用 QuickSpawnItem(new EntitySource_FishedOut(this), item) | 这是 Item 结果事务，不应移回 ProjectileSimulation；Version4 当前为空，partial |
| REF-PROJ-003 | 同文件 :19361-19521，FishingCheck、TryBuildFishingContext；:19523-19645 结果与 item/enemy roll | 同一 Projectile 文件中的完整参考邻近调用链 | FishingCheck 构建 FishingAttempt、读取池水/岩浆/蜂蜜、玩家 fishing conditions、水量、区域、luck 和 drop levels；结果阶段分出敌对生成与 item drop | 该链只用于确认 Fishing boundary；不能作为 Version4 主覆盖事实，partial |
| REF-PROJ-004 | D:\TRbackup\NLTX\docs\Version4与完整源码差异附录-2026-09-05.md 中对应 fishing 差异记录 | 作为路径/签名差异索引，与上面实际读取的完整参考代码相互印证 | 记录了 Version4 空方法和完整参考实现的成员级差异 | 附录是索引/补证材料，不提升 Version4 事实等级，confirmed（差异记录本身） |

因此 fishing 的可交接结论是：ProjectileSimulation 只拥有 bobber 的空间、时间和触发信号；FishingAndCatchSimulation 候选拥有 FishingAttempt、资格、结果、Item/NPC outcome chain，最终 shared owner 仍为 crossSubsystemOwner: integration-review。

## 6. tModLoader 公开 API 交叉验证

### 6.1 检索记录

~~~text
source: D:\TRbackup\tmodloader-api-docs-stable
version: tModLoader v2026.07
query: ModProjectile, AI, CanDamage, CanHitNPC, CanHitPlayer, CanHitPvp, Colliding, ModifyDamageHitbox, ModifyHitNPC, ModifyHitPlayer, OnHitNPC, OnKill, OnSpawn
hits: D:\TRbackup\tmodloader-api-docs-stable\class_mod_projectile.html:8, 29, 96-100, 105-134, 186-218
evidence: 公开 projectile Hook、命中几何、伤害修改、OnHit/OnKill/OnSpawn 的调用侧和生命周期说明
gaps: 文档不包含 Version4 私有 Update、slot allocation、identity table、Kill 分支或 Terraria 全部 AI style 算法
stop_reason: 已取得当前任务所需的公开 Hook/网络边界；不以公开 API 替代私有源码深读
~~~

### 6.2 API 表

| ID | 文档证据 | 可交叉验证的边界 | 不能推出的结论 | evidenceStatus |
|---|---|---|---|---|
| TML-PROJ-001 | class_mod_projectile.html:8 页面标题为 ModProjectile Class Reference；:29 为 tModLoader v2026.07；:96-100 说明每种 projectile 的 properties/hooks 注册方式 | 公开扩展层可以把 Projectile 行为 Hook 化；这支持将策略接在行为边界，而不是把每种 type 做成子系统 | 不能推出 Version4 Projectile 已经按 ModProjectile 分层，也不能替代 aiStyle 私有 dispatch | confirmed |
| TML-PROJ-002 | :105-106 AI()：只有 PreAI 允许时调用，并可在 local/server/remote client 调用 | AI 的公开扩展时机和多侧调用事实；需要注意预测/投影与权威写入隔离 | 不能据此断定 Version4 所有 AI 在三侧等价，或网络字段自动同步 | confirmed |
| TML-PROJ-003 | :117-127 CanDamage、CanHitNPC、CanHitPlayer、CanHitPvp | 伤害资格和目标类别可以是独立 Hook/规则边界；Player 命中公开说明 server/client 侧不同 | 不能用公开 Hook 代替 Version4 Damage_CanDealDamage 和 immunity/owner hit check | confirmed |
| TML-PROJ-004 | :133-134 Colliding(Rectangle, Rectangle) | 自定义 laser/trail/特殊几何应进入碰撞边界，而非塞进普通矩形组件 | 不能推出 Version4 的所有特殊 type 或 oldPos 算法 | confirmed |
| TML-PROJ-005 | :186-196 ModifyDamageHitbox、ModifyHitNPC、ModifyHitPlayer | hitbox 与伤害修饰可作为命中前阶段；Player 命中端侧应单独标记 | 不能推出 CombatAndStatus 的最终 ownership，也不能证明行为等价 | confirmed |
| TML-PROJ-006 | :202-218 OnHitNPC、OnHitPlayer、OnKill、OnSpawn | 命中后/销毁后/生成后的效果适合事件或 Adapter；OnKill 可有生成物和表现效果 | 文档不证明 Version4 Kill() 的具体分支顺序、tombstone 或 child spawn 事务 | confirmed |

## 7. Space Station 14 ECS 参考表

Space Station 14 仅作为结构参考，禁止复制其代码、命名、目录结构或领域语义。

| ID | 实际读取文件和类型/方法 | 参考用途 | 对 Terraria 的限制 | evidenceStatus |
|---|---|---|---|---|
| SS14-PROJ-001 | C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Projectiles\ProjectileComponent.cs:9-100，ProjectileComponent | 观察基础 projectile component 将 shooter/weapon、damage、delete-on-collide、spent、penetration threshold/amount 分开表达 | 只能借鉴粒度；Terraria 的 owner、identity、penetration 和 immunity 语义必须由 Version4 决定 | confirmed |
| SS14-PROJ-002 | ...\Content.Shared\Projectiles\SharedProjectileSystem.cs:21-45，SharedProjectileSystem.Initialize | 观察 System 通过显式 event subscription 建立碰撞前、命中、嵌入和 shutdown 边界 | 不表示 Terraria 必须使用相同 event API；本报告只采用“边界可见、写者可追踪”的原则 | confirmed |
| SS14-PROJ-003 | 同文件 :91-187，OnEmbedProjectileHit、EmbedAttach、EmbedDetach | 观察实体关系、parent、detach、delete/recover 的生命周期可以独立于基础 projectile payload | Terraria 是否有对应嵌入语义须由 Version4 逐类型确认，不能直接引入 Embedded 组件 | confirmed |
| SS14-PROJ-004 | 同文件 :194-203、:216-231、:240-272，复杂伤害、PreventCollision、Impact/Hit/BeforeHit events | 观察目标相关 damage policy、self-collision 排除、命中前后事件和投影效果可以隔离 | CombatAndStatus、SpatialSimulation 和 NetworkSession 的最终 owner 仍由 NLTX/Version4 整合裁决 | confirmed |
| SS14-PROJ-005 | ...\Content.Client\Projectiles\ProjectileSystem.cs:9-57，ImpactEffectEvent 接收并生成 timed visual | 观察客户端表现只消费 effect event，不反写 shared simulation state | 不表示 Terraria 的 Dust/Sound 必须同样实现；只验证 Projection 不写 authority 的原则 | confirmed |
| SS14-PROJ-006 | ...\Content.Shared\Projectiles\EmbeddableProjectileComponent.cs:10-54 与 ComplexProjectileDamageComponent.cs:15-51 | 观察可选 relation 和复杂 damage policy 不必膨胀基础 projectile component | 不能把 SS14 的数据字段当作 Terraria 组件字段或行为 | confirmed |

若需要 SS14 对 Terraria 专有 fishing、leashed registry、owner-scoped projectile identity 的直接对应证据：Space Station 14 无直接对应证据；以下边界仅由 Version4 真实代码和 NLTX 项目约束决定。

## 8. 成员、字段、方法、读者/写者与生命周期盘点

| 责任组 | Version4 成员/字段 | 主要读者 | 主要写者 | 生命周期/副作用 | 归类建议 |
|---|---|---|---|---|---|
| 身份与槽位 | active :90、whoAmI、identity :168、owner :126、projUUID :248、Main.projectileIdentity | New、Update、Kill、NetMessage、MessageBuffer | SetDefaults、NewProjectile、网络 receive、Kill | active 决定存活；identity 反查；slot 可复用；网络 receive 需 reconcile | ProjectileIdentity + 外部 lifecycle/registry；ID shared owner integration-review |
| 类型与行为 | type :118、aiStyle :136、ai[] :128、localAI[] :130、numUpdates :200、extraUpdates :196 | AI、Damage eligibility、Colliding、movement、network | SetDefaults、NewProjectile、各专用 AI、网络 receive | 每个主 tick/子步更新；兼容数据可能由网络或 type-specific branch 写入 | Behavior strategy key + raw compatibility state |
| 轨迹/空间 | position/velocity（继承 Entity）、rotation :116、spriteDirection :146、oldPos/oldRot :180-184、stepSpeed :134 | AI、movement、collision、Damage、render | AI、HandleMovement、Update、特殊 Kill | 轨迹是权威模拟；trail 是派生/缓存；不复制通用 Location/Velocity | 读取 SpatialSimulation 提供的通用位置/速度组件；trail 单独投影或缓存 |
| 生命周期 | timeLeft :138、active、Kill() :46425 | Update、AI、Damage、Net receive | SetDefaults、AI、Update、Kill、特殊类型 | timeLeft decrement 在 Damage 后；Kill 可能是递归/结构变化入口 | Lifetime state + Kill request/commit |
| 伤害载荷 | damage :142、originalDamage :144、knockBack :152、armorPenetration :266、bonusCritChance :268、tagEffectType/bonusTagDamage :262-264、melee/ranged/magic/cold flags | Damage、Player/NPC 规则、NetMessage | SetDefaults、New、minion scaling、hit special branches、network receive | payload 是攻击来源描述；不能拥有 target health；部分字段在命中中变化 | ProjectileDamage payload |
| 穿透与免疫 | penetrate :156、maxPenetrate :166、localNPCImmunity :158、playerImmune :220、static/local cooldown :256-258、restrikeDelay :192 | Damage、Update、SetDefaults | Damage、Update decrement、SetDefaults、网络/行为特殊分支 | 需要按 hit sequence 和 owner/target/type scope 隔离；不能塞入 lifetime | Penetration state + hit immunity state |
| 碰撞策略 | tileCollide :194、ignoreWater :202、correctSlopeCollision :250、decidesManualFallThrough/shouldFallThrough :252-254、reflected :150 | HandleMovement、Colliding、AI、Damage | SetDefaults、AI、movement、reflection | 读取 world geometry；可能产生 reflect/bounce/Kill/Tile command | Collision policy/state；Tile storage 外置 |
| 网络 | netUpdate/netUpdate2/netSpam :172-178、netSyncSkippedForPlayer :178 | Update、NetMessage、section resend | Update、Damage、AI、Kill、Net receive | packet 27/29、section visibility、节流、补发；不应是核心模拟状态唯一来源 | Network projection input + NetworkSession adapter |
| 表现/派生 | oldPos、oldRot、oldSpriteDirection、light、alpha、frame、frameCounter、soundDelay、WhipPointsForCollision、static hitbox/list caches :282-294 | Render、Colliding、AI、Kill、查询临时填充 | Update、AI、Kill、查询 | 有的为 collision cache，有的为 presentation；不能把所有数组做 authority | Trail/geometry cache/presentation projection |
| 专用关系 | minion/minionSlots/minionPos、sentry、trap、bobber、counterweight、MinionSpawnInfo、bannerIdToRespondTo | Player、AI、Damage、spawn source、Fishing、network | SetDefaults、New、AI、hit branches | 这些是可选能力/关系；相邻领域仍拥有各自事务 | 可选 capability components；跨域 owner integration-review |
| fishing 缓存 | _context :294、_availableFishTypesToShow :292 及 fishing helper | fishing display/check | Projectile fishing methods和完整参考 | 当前 Version4 目标方法为空；静态缓存不是实体权威状态 | 交给 FishingAndCatchSimulation 重新裁决 |

## 9. 权威状态所有权

| 状态 | Version4 当前事实上的写入根 | proposed owner | 读者/消费者 | 备注 |
|---|---|---|---|---|
| Projectile 存活与剩余时间 | SetDefaults、Update、Kill | ProjectileLifetimeComponent，status: proposed；结构提交由 SpawnLifecycleAndLoot 负责，status: proposed | Behavior、Collision、Hit、Replication | active 的 runtime entity 生命周期不能与网络 tombstone 混为一谈 |
| owner/identity/slot/UUID | New、MessageBuffer、Kill 和 Main 反查表 | ProjectileIdentityComponent，status: proposed；slot allocator/registry，status: proposed 且 crossSubsystemOwner: integration-review | Damage、Spawn、Replication、Network | owner index、runtime entity、identity、UUID、replicationId 分开建模 |
| trajectory/AI | AI、HandleMovement、Update、Net receive | TrajectoryStateComponent，status: proposed；behavior strategy registry，status: proposed | Collision、Damage、Projection | raw AI 兼容字段暂缓 typed 化 |
| Projectile damage payload | SetDefaults、New、minion scaling、hit modifiers | ProjectileDamageComponent，status: proposed | Combat、Replication、hit rules | 只描述 projectile attack，不写目标 life/health |
| penetration | Damage_PVE_Inner、特殊 AI/hit branch | ProjectilePenetrationComponent，status: proposed | Hit commit、Lifetime、Replication | immunity 另设作用域，不嵌进 penetration component |
| collision policy | SetDefaults、AI、HandleMovement | ProjectileCollisionPolicyComponent，status: proposed | Collision Query、Movement、Environment contact | world tile/section geometry 仍由 SpatialSimulation 提供 |
| target health、NPC/Player immune/buff/death | Version4 Damage_PVE_Inner 直接调用 target/owner 方法 | CombatAndStatus，crossSubsystemOwner: integration-review | Projectile 只提交 hit command | 当前 Version4 混合写法是迁移风险，不是 proposed ownership 的依据 |
| child projectile/drop/structural despawn | Kill、Damage、special AI | SpawnLifecycleAndLoot，crossSubsystemOwner: integration-review | Spawn、Loot、Network tombstone、Presentation | 最终事务边界须整合会话决定 |
| network snapshot/message | NetMessage/MessageBuffer | ProjectileReplicationProjection 与 NetworkSessionAndSectionStreaming，均 status: proposed；共享 snapshot owner integration-review | Client projection、protocol adapter | Projection 只读；receive 不能成为 server authority |
| fishing outcome | 完整参考 FishingCheck/delivery；Version4 当前目标方法为空 | FishingAndCatchSimulation，crossSubsystemOwner: integration-review | Item/NPC outcome、UI | Projectile 只维护 bobber carrier/trigger |
| leashed registry/section | LeashedEntity.Registry、SectionEntityList | LeashedEntitySimulation，crossSubsystemOwner: integration-review | Leashed network/update | 不进入 Projectile entity lifecycle |

## 10. proposed Component 拆分

下表所有路径和类型均为设计草案，未创建文件。按 Projectile 能力/责任组织，不引入通用 Shared/Components catch-all 目录。

| proposed 类型与路径 | 状态字段/职责 | 不拥有的内容 | 共同读写者 |
|---|---|---|---|
| D:\TRbackup\NLTX\src\Projectile\Identity\ProjectileIdentityComponent.cs，status: proposed | runtime entity reference、owner reference、owner-scoped identity、slot token、可选 projUUID 的分离值 | 不拥有 persistent ID、protocol packet、Player/NPC health | Spawn、Replication、Network reconcile |
| D:\TRbackup\NLTX\src\Projectile\Trajectory\TrajectoryStateComponent.cs，status: proposed | behavior strategy ID、raw ai[3]、raw localAI[3]、extra updates、substep index、必要的 direction policy | 不复制通用 Location/Velocity；不把每个 type 变成组件 | Behavior、Movement、Collision、Replication |
| D:\TRbackup\NLTX\src\Projectile\Lifecycle\ProjectileLifetimeComponent.cs，status: proposed | remaining ticks、terminal reason、expiration policy、pending kill sequence | 不拥有 entity destruction、loot、network tombstone | Lifetime、Kill commit、Replication |
| D:\TRbackup\NLTX\src\Projectile\Impact\ProjectilePenetrationComponent.cs，status: proposed | maximum/remaining penetration、hit count、stops-dealing-damage-after-depletion | 不拥有 NPC/Player immunity map | Hit resolution、Combat、Lifetime |
| D:\TRbackup\NLTX\src\Projectile\Impact\ProjectileHitImmunityStateComponent.cs，status: proposed | projectile-local NPC immunity、player immunity、restrike delay；scope key 明确为 projectile/target | 不拥有 Combat 全局 immunity policy 或 target health | Hit resolution、Combat、Replication |
| D:\TRbackup\NLTX\src\Projectile\Impact\ProjectileDamageComponent.cs，status: proposed | current/original damage、knockback、armor penetration、crit/tag metadata、damage class | 不计算最终 target health；不直接调用 NPC/Player | Hit command builder、Combat、Replication |
| D:\TRbackup\NLTX\src\Projectile\Collision\ProjectileCollisionPolicyComponent.cs，status: proposed | tile collision、fall-through、liquid policy、reflect/bounce、slope policy、owner self-hit policy | 不持有 Tile/World grid 或 entity broadphase | Collision Query、Movement、Environment contact |
| D:\TRbackup\NLTX\src\Projectile\Capability\ProjectileCapabilityComponents.cs，status: proposed | 设计上应拆成同名核心类型的 sentry/minion/trap/bobber/counterweight capability 文件；每个类型一项稳定能力 | 不拥有相邻领域的 minion accounting、Fishing result、trap world ownership | PlayerGameplay、Fishing、SpawnLifecycle、Replication |
| D:\TRbackup\NLTX\src\Projectile\Presentation\ProjectileTrailState.cs，status: proposed | old position/rotation/direction 的派生缓存 | 不拥有碰撞权威位置，不拥有 renderer | Collision special geometry、Presentation |

对当前 NLTX 已存在 ProjectileBehaviorComponent、ProjectileDefinitionComponent、ProjectileDamageComponent 和 ProjectileLifetimeComponent 的处理建议是“先核对读写者，再增量收敛”，不是另造 _v2 组件。当前 ProjectileBehaviorComponent 的 State0..State3/LocalState0..LocalState1 应在 typed behavior 契约闭合前继续作为 compatibility state；当前 ProjectileDirectionComponent 的 Trajectory 不能在没有迁移依据时直接替换为通用 Location/Velocity。

## 11. System、Query、Command、Adapter、Projection 契约

### 11.1 proposed Systems

| 类型/路径 | 契约 | 读取 | 写入/输出 | 副作用、失败与重试 |
|---|---|---|---|---|
| ProjectileBehaviorSystem，status: proposed，路径 ...\src\Projectile\Systems\ProjectileBehaviorSystem.cs | 每个 projectile 的每个 substep 根据 strategy ID 计算 trajectory state transition | Identity、Trajectory、definition、显式 world/player read view | Trajectory、Velocity/Location transition、behavior events | 不直接随机、日志或网络；未知 style 产生 UnsupportedBehavior evidence，不静默删除 |
| ProjectileEnvironmentContactSystem，status: proposed | 处理 wet/liquid/edge contact 和环境候选 | Collision policy、Location、Spatial query result | movement/reflect/kill commands | Tile change 由 Spatial/World command adapter 提交；失败可记录 command rejection，不重复应用同一 sequence |
| ProjectileMovementSystem，status: proposed | 按行为结果执行位置速度提交，支持 extraUpdates | Trajectory、Velocity、Collision result | Location、Velocity、trail cache | 纯状态 commit 与外部 I/O 分离；substep sequence 必须稳定 |
| ProjectileHitGenerationSystem，status: proposed | 对候选目标调用纯 Query，产生 hit command，不结算目标 | Projectile payload、geometry、target read view、immunity read view | ProjectileHitCommand，status: proposed | 不能直接写 Player/NPC health；command 必须可排序、去重和审计 |
| ProjectilePenetrationSystem，status: proposed | 在 Combat 接受命中结果后提交 projectile-local penetration/immunity 变化 | accepted hit result、Penetration、HitImmunity | penetration/immunity state、KillRequest | 必须维持 Version4 的 hit → hit effects/target strike → penetration/immunity → Kill 语义，具体事务 owner 留 integration-review |
| ProjectileLifetimeSystem，status: proposed | 递减 timeLeft 并产生 terminal request | Lifetime、special lifetime policy | Lifetime、ProjectileKillRequest | request 可重试但 commit 必须幂等；不能仅以 entity delete 代替 tombstone |
| ProjectileReplicationProjection，status: proposed | 只读 authority components 生成 snapshot | Identity、Trajectory、Damage、Lifetime、Penetration、section view | immutable replication snapshot | 不写 simulation；发送失败交给 NetworkSession 以 sequence/retry 处理 |

### 11.2 proposed Query

~~~text
type ProjectileCollisionQuery, status: proposed
input:
  projectileGeometry: explicit hitbox/trail/cone descriptor
  targetGeometry: explicit target shape
  worldGeometry: SpatialSimulation read view
  collisionPolicy: ProjectileCollisionPolicyComponent
  trajectoryContext: current/previous position and substep
output:
  CollisionQueryResult, status: proposed
  { isHit, contactPoint, normal, targetCandidate, reflectionAxis, reason }
rules:
  - pure calculation; no authority write
  - no implicit Main.tile/Main.npc/Main.player access
  - no Kill, Damage, network send, logging or UI
~~~

Colliding 的普通矩形、cone、trail、whip、lance 等几何可以逐类适配到该 Query，但不能在报告阶段声称已完成算法迁移。Query 的输出只表示资格/几何事实；movement、reflection、penetration 和 target health 必须由后续 System/Command 提交。

### 11.3 proposed Commands

~~~text
ProjectileSpawnRequest, status: proposed
  source, ownerReference, projectileType, position, velocity,
  initialDamage, knockback, ai[3], requestedCapabilities, spawnSequence

ProjectileHitCommand, status: proposed
  projectileEntityReference, targetEntityReference,
  projectileIdentity, ownerReference, hitSequence,
  damagePayload, collisionResult, penetrationSnapshot,
  reflectionCandidate

ProjectileKillRequest, status: proposed
  projectileEntityReference, projectileIdentity, ownerReference,
  reason, killSequence, spawnChildrenPolicy, dropPolicy,
  networkTombstonePolicy
~~~

命令方向：

~~~text
PlayerGameplay
  -> ProjectileSpawnRequest
ProjectileSimulation
  -> ProjectileHitCommand
  -> CombatAndStatus
ProjectileSimulation
  -> ProjectileSpawnRequest(child)
  -> SpawnLifecycleAndLoot / RuntimeComposition
ProjectileSimulation
  -> ProjectileKillRequest
  -> SpawnLifecycleAndLoot
ProjectileReplicationProjection
  -> ReplicationSnapshot
  -> NetworkSessionAndSectionStreaming
~~~

ProjectileHitCommand 的 producer 候选是 ProjectileSimulation，consumer 是 CombatAndStatus；该命令类型的最终共享 owner 标记为 crossSubsystemOwner: integration-review。Combat 的结果应返回可排序的 accepted/rejected/reflected outcome，Projectile 再提交 local penetration/immunity 变化。

### 11.4 proposed Adapter/Port

| 类型 | 设计契约 | 隔离的副作用 |
|---|---|---|
| IProjectileSpawnPort，status: proposed | Submit(ProjectileSpawnRequest)；分配 runtime entity/slot/identity，并返回可审计 result | entity create、slot allocator、UUID/random、source bookkeeping |
| IProjectileKillPort，status: proposed | Submit(ProjectileKillRequest)；按 sequence 幂等处理 tombstone、entity removal、child/drop requests | 结构变化、loot、child spawn、network tombstone |
| IProjectileReplicationPort，status: proposed | Publish(ProjectileReplicationSnapshot)；不接受写 simulation 的反向调用 | protocol serialization、section/PVS send、retry/skip resend |
| IProjectileRandomSource，status: proposed | 显式接收 seed/stream 并返回随机值 | Version4 Main.rand、NewProjectile 初始化随机和 Kill visual 随机 |
| IProjectileClock，status: proposed | 提供 tick/update sequence | Main.GameUpdateCount 和 time-based retry/retention |

这些接口的具体 owner、命名和公共程序集不能在单个子系统报告中最终决定；跨域接口/值对象使用 crossSubsystemOwner: integration-review。

## 12. 调用方向与 System 顺序

Version4 的确认顺序是：Player 更新在 Main.cs:11430，NPC 更新在 :11511-11525，Projectile 前置/1000 槽位更新在 :11530-11551；单个 Projectile.Update 内，AI() 和环境/移动/碰撞后，Damage() 发生在 timeLeft-- 与 Kill() 之前，网络发送紧随终止判断之后。

建议的显式 proposed scheduler 约束如下：

~~~text
PlayerGameplay
  -> NpcAndTownSimulation
  -> ProjectileBehaviorSystem
  -> ProjectileEnvironmentContactSystem
  -> SpatialSimulation / ProjectileMovementSystem
  -> ProjectileCollisionQuery
  -> ProjectileHitGenerationSystem
  -> CombatAndStatus
  -> ProjectilePenetrationSystem
  -> ProjectileLifetimeSystem
  -> ProjectileKillCommit
  -> SpawnLifecycleAndLoot
  -> ProjectileReplicationProjection
  -> NetworkSessionAndSectionStreaming
~~~

必须保持的局部约束：

1. 先执行 AI/行为子步，再根据显式前后位置执行环境和实体碰撞 Query。
2. ProjectileCollisionQuery 不得写 authority；Query 结果先形成 hit/movement/reflect command。
3. CombatAndStatus 的 target health/buff/death 结算不由 ProjectileSimulation 代办。
4. accepted hit 的 Projectile-local penetration/immunity commit 必须发生在可观察命中结果之后；不能把所有状态推迟到 Kill 之后。
5. Version4 timeLeft-- 与 penetrate == 0 终止检查位于 Damage 后，故 lifetime/kill phase 不能提前到命中之前。
6. Kill commit 后才能清理 entity/slot；但 identity tombstone 的可见时间窗和 child spawn/drop 顺序需要整合决定。
7. Projection 只能读取 commit 后的权威状态，NetworkSession 负责 section visibility、skip/resend 和 packet direction。

## 13. 持久化、网络与客户端投影边界

### 13.1 ID/关系分离

建议在最终整合时逐一验证以下概念，不能用一个 EntityId 代替：

| 概念 | Version4 证据/当前含义 | proposed 表达 | owner 状态 |
|---|---|---|---|
| slot/whoAmI | Main 的 projectile[1001] 槽位；NewProjectile 写 whoAmI=num | runtime pool/slot token | crossSubsystemOwner: integration-review |
| owner index | owner 默认 255，网络 message 27 写 byte | owner reference/value | crossSubsystemOwner: integration-review |
| owner-scoped identity | identity 与 projectileIdentity[owner,identity] 反查 | ProjectileIdentityComponent 的 network identity | crossSubsystemOwner: integration-review |
| projUUID | 仅部分 type/sets 需要；message 27 可选传输 | optional external/projectile UUID | crossSubsystemOwner: integration-review |
| ECS runtime entity | 当前 dome Arch entity | runtime entity reference | crossSubsystemOwner: integration-review |
| network replication ID | 当前 dome ProjectileStore 的 replicationId | protocol/session replication key | crossSubsystemOwner: integration-review |
| persistent ID | 本轮未找到 Projectile 持久化完整证据 | persistence ID/value object | unresolved，不得复用 identity |
| section/PVS | NetMessage message 27 按 client section active 发送 | WorldSectionId/visibility view | crossSubsystemOwner: integration-review |

### 13.2 Version4 网络边界

- D:\TRbackup\Version4\Terraria\NetMessage.cs:769-843 的 message type 27 序列化 identity、position、velocity、owner、type、AI flags、ai[0..2]、banner、damage、knockBack、originalDamage 和可选 projUUID。
- NetMessage.cs:852-855 的 type 29 发送 projectile identity 和 owner，用于 Kill。
- NetMessage.cs:1760-1785 按 client section visibility 发送 type 27；不可见时写 netSyncSkippedForPlayer，:1807-1809 发送后清除 skipped 标志。
- NetMessage.cs:2687-2693 玩家加入时遍历 active、owner 匹配的 projectile 发送 type 27。
- D:\TRbackup\Version4\Terraria\MessageBuffer.cs:1320-1404 接收 type 27，先按 owner+identity 找 active projectile，再找空槽，必要时回收最老槽，设置 defaults/identity/position/velocity/owner/AI/UUID，并转发；:1439-1453 接收 type 29 后按 owner+identity 调用 Kill() 并广播。

这表明 ReplicationProjection 只能输出快照；section/PVS、重传/补发、protocol encode/decode 归 NetworkSessionAndSectionStreaming adapter。客户端接收的投影状态不得反向成为 server authority。

### 13.3 持久化边界

本轮读取到的核心 Version4 证据覆盖 runtime slot、network identity 和 UUID，但没有闭合 Projectile 的跨世界/存档持久化契约。因此：

- identity、slot、UUID、replicationId 暂不能宣布任一为持久化 ID；
- persistence adapter 不应直接序列化 ProjectileReplicationSnapshot 作为存档格式；
- 若未来 Projectile 允许跨存档恢复，必须单独定义 PersistentProjectileState，status: proposed，并由最终整合会话确定 owner；
- projUUID 的长期映射和恢复策略是 blocking-decision，不能依据当前 packet 字段推断。

## 14. 当前 NLTX 映射

### 14.1 已观察到的局部实现

| 当前路径 | 实际内容 | 对 Version4 的覆盖判断 |
|---|---|---|
| D:\TRbackup\NLTX\src\Projectile\ProjectileBehaviorComponent.cs:3-30 | 已有 style、State0..State3、LocalState0..LocalState1 数据 | 只覆盖 raw behavior data；未证明全部 AI style typed/执行闭合，partial |
| ...\src\Projectile\ProjectileDamageComponent.cs:3-23 | 已有 current/original/knockback/armor/crit 数据 | payload 模型局部存在；未证明 Combat commit/命中效果完整，partial |
| ...\src\Projectile\ProjectileDefinitionComponent.cs:3-23 | 已有 type、friendly/hostile、extraUpdates、catalog revision | definition 与 runtime state 仍需读写边界核对，partial |
| ...\src\Projectile\ProjectileDirectionComponent.cs:5-12 | 已有 Vector2 Trajectory | 不能证明等价于 Version4 position/velocity/rotation/direction，partial |
| ...\src\Projectile\ProjectileLifetimeComponent.cs:3-12 | 已有 RemainingTicks/EndReason | 仅为数据模型，不含 Kill transaction/tombstone，partial |
| ...\src\Projectile\ProjectileOwnerComponent.cs:5-12 | 已有 EntityReference owner | 当前 Version4 owner index/255 与 dome PlayerHandle 的适配仍是 unresolved |
| ...\src\Projectile\ProjectilePenetrationComponent.cs:3-17 | 已有 remaining/maximum/stops-dealing-damage | 不能证明 local/static/player/owner immunity 的作用域和顺序完整，partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Commands\SpawnProjectileCommand.cs:5-37 | Spawn command 使用 PlayerHandle owner，携带 type/behavior/damage/lifetime/AI/capability flags | 已有 spawn 输入，但 owner 类型和 Version4 trap/NPC/world owner 不闭合，partial |
| ...\Projectile\Systems\ProjectileSpawnSystem.cs:18-151 | 创建 Arch entity，添加 definitions、behavior、network identity、penetration、Location/Velocity、damage/lifetime 等组件；:133 使用 Guid.NewGuid()；:195-212 添加 bobber/counterweight/fall-through | 局部 Spawn 链存在；未证明 slot allocation、Version4 identity table、source side effects 和全部 defaults 等价，partial |
| ...\Projectile\Systems\ProjectileBehaviorSystem.cs:11-79 | registry 注册约 21 个行为；:76-78 修改 Location/Velocity/Behavior | 已有策略执行边界，但远未闭合 Version4 全部 specialized AI，partial |
| ...\Projectile\Systems\ProjectileCollisionSystem.cs:9-257 | 提供 tile/path/slope/liquid query，且通过显式 WorldGrid/组件输入 | 方向符合 Query 边界；未证明覆盖 Version4 entity geometry、whip/cone 等全部碰撞，partial |
| ...\Projectile\Systems\ProjectileDamageSystem.cs:19-205 | 对 candidates 排序，检查 immunity/eligibility，递减 penetration，应用 local/static/owner/player immunity并登记 hit | 有局部 hit/immunity 实现，但当前返回 accepted events，仍未闭合 Version4 target policy、reflection、Combat command、Kill 顺序，partial |
| ...\Projectile\Systems\ProjectileLifetimeSystem.cs:8-27 | 只递减 RemainingTicks，到期返回 bool | 没有展示完整 Kill、child spawn、drop、tombstone，partial |
| ...\Projectile\Systems\ProjectileReplicationSystem.cs:11-153 | 只读多组件生成 snapshot，含 identity/UUID/AI/damage/lifetime/section/network flags | 是 Projection 候选；字段数量过大，有成为第二个巨型权威模型的风险，partial |
| ...\Snapshots\ProjectileReplicationSnapshot.cs:7-95 | 快照同时含 owner、identity、UUID、AI、damage、lifetime、section、friendly/hostile、penetration、fishing/minion/network 字段 | 应明确为 immutable projection DTO，不可反向写模拟；当前 scope 偏宽，partial |
| ...\Projectile\ProjectileStore.cs:7-49 | 仅保存 Arch Entity → replicationId，反查按字典线性扫描 | 不是 Version4 owner+identity 反查表的等价实现，partial |
| D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Isolation\NetworkProjectileSlice.cs:5-43 | 从 snapshot 投影少量 network fields | 协议隔离方向正确，但尚未覆盖 Version4 message 27/29、section skip/resend 和 tombstone，partial |

### 14.2 当前结论

当前 NLTX 是“已有局部数据模型和部分执行链，但行为闭合度不足”的 partial，不是 missing，也不是 independently-verified。特别需要避免以下误读：

- 有 ProjectileSpawnSystem 不等于 Version4 NewProjectile 的 slot/identity/source/random/definition 语义已覆盖；
- 有 ProjectileDamageSystem 不等于 CombatAndStatus 的 health/death/buff 事务已正确分界；
- 有 replication snapshot 不等于网络 protocol 与 section/PVS/tombstone 已闭合；
- 有 fishing/bobber component 不等于 FishingAndCatchSimulation 的资格和 outcome 已实现；
- 有 verifier 源文件不等于本轮或当前 checkout 的验证结果已执行。

## 15. focused verifier 设计

以下是建议的独立 verifier 场景，均为设计计划，verificationStatus: not-run。每个 verifier 应使用纯输入/输出 fixture 或受控 fake port，记录 input、ordered events、authority diff、command sequence 和 rejection reason。

| ID | 场景 | 断言 | 主要证据/缺口 |
|---|---|---|---|
| VER-PROJ-001 | spawn identity separation | slot/whoAmI、owner+identity、UUID、runtime entity、replicationId 各自可追踪，slot reuse 不改变 owner-scoped identity 规则 | Version4 NewProjectile/MessageBuffer；当前 ProjectileStore 反查不等价 |
| VER-PROJ-002 | defaults reset | 复用最老 slot 后旧 ai/localAI、immune、net flags、trail、minion/sentry/bobber flags 不泄漏 | Version4 SetDefaults:444-556 |
| VER-PROJ-003 | extra updates | extraUpdates=n 产生 n+1 子步，AI/movement/collision/hit/lifetime 的顺序与主 tick contract 一致 | Version4 Update:14693-14705 |
| VER-PROJ-004 | behavior compatibility | raw AI/localAI 在未知/未 typed style 时保持可读写；已注册 strategy 与 raw state 同步，不静默丢弃 | Version4 AI dispatch；当前 behavior registry 仅局部 |
| VER-PROJ-005 | pure collision | 相同显式 geometry/world view 得到相同 result；Query 不改变位置、速度、penetration、Tile 或 target health | Version4 Colliding、NLTX CollisionSystem |
| VER-PROJ-006 | collision geometry matrix | 普通矩形、cone、trail/whip、lance、slope、fall-through、liquid 逐项验证，并标出尚未覆盖的 type | Version4 Colliding:13654+、HandleMovement:15513+ |
| VER-PROJ-007 | immunity/penetration order | accepted hit 的 penetration、local/static/owner/player immunity 按 hit sequence 递减；depleted hit 触发正确 damage eligibility | Version4 Damage_PVE_Inner:11612-12849、当前 DamageSystem |
| VER-PROJ-008 | hit command boundary | Projectile 只产生 ProjectileHitCommand；Combat fake consumer 才写 target health/status；reflection 结果可回到 projectile | Version4 Damage 直接 target write 是迁移风险 |
| VER-PROJ-009 | lifetime/kill | timeLeft decrement 在 Damage 之后；penetrate=0 或 timeLeft<=0 才产生 kill request；Kill 幂等并保留 tombstone | Version4 :15228-15275、:46425+ |
| VER-PROJ-010 | Kill effect ordering | identity cleanup、channel cancel、child spawn、drop、visual effect、active removal、tombstone 的 sequence 可审计 | Version4 Kill 分支复杂且有直接 active=false return |
| VER-PROJ-011 | network reconcile | message 27 的 owner+identity 查找优先于空槽；message 29 只结束匹配的 owner+identity；未知 UUID 不覆盖错误 entity | Version4 NetMessage/MessageBuffer |
| VER-PROJ-012 | section skip/resend | inactive section 不丢 snapshot；进入 active section 后补发一次且不重复提交 | Version4 netSyncSkippedForPlayer |
| VER-PROJ-013 | projection purity | snapshot/protocol slice 生成不写 authority；tombstone 在 retention window 内可投影；客户端 projection 不反向写 server state | 当前 snapshot/slice 覆盖有限 |
| VER-PROJ-014 | fishing boundary | bobber carrier 可发 trigger，但 Fishing qualification/drop/item/NPC outcome 由 fishing consumer 处理；Projectile 不直接 QuickSpawnItem | 完整参考 fishing 与 Version4 空实现差异 |
| VER-PROJ-015 | leashed boundary | Leashed registry/section activation/deactivate/update 不进入 Projectile spawn/kill；anchor relation 只通过整合确认的 reference | Version4 LeashedEntity:18-306 |
| VER-PROJ-016 | owner domain matrix | Player、NPC、trap、world、server owner 输入都不会被强制压成 PlayerHandle；owner invalid 时失败原因可见 | 当前 SpawnCommand 只用 PlayerHandle，是 blocking decision |

未来执行这些 verifier 时，compile-capable 命令必须从仓库根目录通过 Build/Tools/Invoke-SerialDotnet.ps1，只构建受影响项目，使用 -p:UseSharedCompilation=false、-m:1、-nr:false，并在 build 后以 --no-build --no-restore 运行 verifier。本轮不执行任何此类命令。

## 16. 本次验证状态

~~~text
verificationStatus: not-run
~~~

本轮实际执行的命令仅为非编译型源码读取、行号定位和工作树/目标文件存在性检查；没有执行 dotnet restore/build/test/run/publish/pack/msbuild，没有启动测试进程，也没有把已有 verifier 源码的存在当作测试结果。故本报告不提供 warning/error count、Build/bin artifact 或行为通过声明。

已有 NLTX verifier 源文件（例如 dome\Test\Terraria.Dome.Combat.Verification\Program.cs、Projectile behavior/hook/hostile/protocol 相关 verifier）只能说明存在局部验证入口；由于本轮未运行，均不提升本报告验证状态。

## 17. 不拆分项

| 对象 | 不作为一级子系统/独立 authority 的理由 |
|---|---|
| 单个 aiStyle | 它是行为策略/兼容 dispatch key；多个 style 共享 Projectile lifecycle、碰撞、伤害和网络边界 |
| 单个 projectile type | 它是 definition/实例分类；type-specific branch 应由 strategy/definition registry 服务，不能产生数百个 subsystem |
| 单个 damage 字段 | current/original/knockback/crit/tag 是共同 payload 的字段，变更原因和读者高度相关 |
| 单个 spawn packet | packet 是 protocol representation；spawn allocation、identity、definition application 和 network serialization 应分别隔离 |
| 单个 render trail | oldPos/oldRot 可能同时支持 collision geometry，但仍是缓存/投影，不是权威模拟根；按使用者拆 cache/query 边界 |
| fishing bobber 本身 | bobber 是一种 Projectile carrier/capability；qualification、drop、Item/NPC outcome 属于 Fishing boundary |
| leashed entity prototype | registry/section activation/update 是独立 lifecycle；Projectile 不能借 type 或 owner 关系吞并它 |
| network message 27/29 | 它们是协议消息；不能反过来定义模拟状态 owner 或客户端权威 |

## 18. 兼容策略与行为保持风险

### 18.1 兼容策略

1. 先建立字段读写矩阵和 event/command sequence，再逐个行为策略迁移；保留 raw ai[]/localAI[] 作为过渡兼容载体。
2. 用 owner+identity 建立稳定 reconcile contract；不要让 slot reuse、runtime ECS entity 或 protocol replicationId 改写 identity 语义。
3. 将 Location/Velocity、Tile/World grid、Player/NPC authority 作为外部显式输入/端口，不复制为 Projectile 巨型 component。
4. 先把 Damage 改为“生成命中候选/命令”的可审计边界，再迁移 Combat target write；不要把 target health 逻辑临时藏进 ProjectileDamageSystem。
5. Kill 迁移采用 sequence/幂等 request，先定义 identity tombstone retention、child spawn/drop 顺序和可重试失败行为。
6. Fishing/LeashedEntity 只通过明确定义的 boundary event/reference 接入；不把当前 Version4 完整参考实现直接复制到 Projectile。
7. Projection 快照按网络消费者裁剪；不能因为当前 ProjectileReplicationSnapshot 字段很多就把它变成第二个权威聚合。

### 18.2 主要风险

| 风险 ID | 风险 | 可能后果 | 缓解/状态 |
|---|---|---|---|
| RISK-PROJ-001 | whoAmI、identity、UUID、replicationId 混用 | 网络 receive 覆盖错误 entity、slot reuse 后 ghost projectile | 先完成 ID matrix verifier；blocking-decision |
| RISK-PROJ-002 | aiStyle raw state 过早 typed 化 | 专用 AI 未覆盖时行为丢失或默认分支错误 | raw compatibility state 保留；partial |
| RISK-PROJ-003 | Damage 直接写 NPC/Player 的旧顺序被打散 | immunity、penetration、death、reflection 顺序变化 | hit sequence + Combat result contract；blocking-decision |
| RISK-PROJ-004 | Kill 被简化为 entity destroy | child spawn、drop、channel cancel、effects、network tombstone 丢失 | KillRequest/Port/sequence；partial |
| RISK-PROJ-005 | 当前 dome Guid.NewGuid() 直接在 spawn system 中生成 UUID | 随机/外部标识副作用难以复现，identity 与 UUID 可能错误绑定 | IProjectileRandomSource/identity allocator；integration-review |
| RISK-PROJ-006 | ProjectileReplicationSnapshot 继续膨胀 | projection 反向成为第二权威模型，协议字段渗入核心 | immutable projection、按 consumer 分片；partial |
| RISK-PROJ-007 | owner 只支持 PlayerHandle | NPC/trap/world/server owner 无法保持 Version4 语义 | owner domain matrix；blocking-decision |
| RISK-PROJ-008 | Fishing 完整参考被误当 Version4 | 产生未由 Version4 证明的能力声明 | 标记 full-reference-supplemented/partial |
| RISK-PROJ-009 | leashed anchor/section 被放进 Projectile | 两套 registry、section 和网络生命周期冲突 | 明确 LeashedEntity boundary；confirmed/empty methods partial |
| RISK-PROJ-010 | 文件顺序代替 scheduler | Player/NPC/Projectile 或 Damage/Lifetime 顺序漂移 | scheduler graph + focused verifier；blocking-decision |

## 19. evidence-gap 与 blocking-decision

### 19.1 普通 evidence-gap

- Version4 Projectile.cs 中大量 specialized AI 的完整读写图尚未在本报告逐个闭合；当前只确认 dispatch、代表性行为和 raw state 的混合性质。
- Version4 Kill() 后段包含很多 type-specific effect 分支，本报告确认其存在和若干 child/visual 分支，但尚未为每种 type 建立完整事务表。
- Version4 HandleMovement 的全部 tile/slope/反射分支尚未逐条映射为 collision result；当前 NLTX CollisionSystem 的覆盖仍为 partial。
- Version4 Projectile 的持久化/存档恢复路径未在本次最小证据中闭合，不能定义 persistent ID。
- Version4 LeashedEntity 的 Remove 与 StreamNetUpdates 为空；独立 boundary 已确认，但完整移除/网络流行为为 partial。
- 当前 NLTX 的局部 verifier、完整 behavior registry 和 network protocol slice 未运行，无法给出 independently-verified 结论。
- tModLoader 文档为 v2026.07 的公开 API 参考，不是 Version4 私有实现；该版本角色是 version-drift/公开边界交叉证据，不参与行为覆盖计数。

### 19.2 blocking-decision

以下事项会改变 authority、事务边界或跨子系统 owner，必须由用户或最终整合会话裁决，本报告不自行选择：

1. owner 类型：
   - 方案 A：统一为 PlayerEntityId，实现简单但不能覆盖 NPC/trap/world/server owner；
   - 方案 B：使用通用 EntityReference，覆盖面更接近 Version4，但需要确定跨域 reference 生命周期；
   - 方案 C：保留 owner-kind + typed reference，协议和查询更复杂但可表达完整来源域。
2. projUUID 长期映射：
   - 方案 A：仅作为可选 network compatibility field；
   - 方案 B：作为 owner-scoped external ID；
   - 方案 C：建立独立 persistent/projectile correlation ID；三者影响存档、网络重连和 child relation。
3. raw AI 的期限：
   - 方案 A：长期保留 compatibility state；
   - 方案 B：按 behavior strategy 分阶段 typed 化并保留 fallback；
   - 方案 C：以完整行为覆盖为门槛后删除 raw state；会延后迁移但减少长期兼容复杂度。
4. ProjectileHitCommand owner：
   - 方案 A：ProjectileSimulation 产生、CombatAndStatus 消费；边界清晰；
   - 方案 B：CombatAndStatus 统筹碰撞/命中；可能扩大 Combat 对空间/轨迹的依赖；
   - 方案 C：Integration 层拥有命中事务，两个子系统只提供候选/结算端口。
5. Kill 事务：
   - 方案 A：ProjectileSimulation 产生 KillRequest，SpawnLifecycleAndLoot commit；
   - 方案 B：统一 EntityLifecycle owner commit，并由 Projectile 提供 effects/child commands；
   - 方案 C：按 server/client 分别提交 authority kill 和 presentation kill；需定义 tombstone 与重放语义。
6. Fishing commit boundary：
   - 方案 A：bobber 仅触发 Fishing command，结果全由 FishingAndCatchSimulation 结算；
   - 方案 B：Projectile 先写兼容的 fishing marker，再由 Fishing 消费；
   - 方案 C：短期 adapter 保持 legacy call，长期迁移 outcome；需防止 marker 变成巨型组件。
7. Leashed anchor/reference：
   - 方案 A：LeashedEntitySimulation 独立拥有 anchor/section，Projectile 仅保存关系引用；
   - 方案 B：共享 relation service；
   - 方案 C：按实体类型分别适配；最终 shared EntityReference/NetworkId owner 仍为 integration-review。

## 20. Integration Handoff

~~~text
subsystemId: ProjectileSimulation
taskNumber: 13
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-projectile-simulation-public-decomposition.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 Projectile 的轨迹、AI dispatch、碰撞资格、Projectile-local lifetime、penetration/immunity、damage payload 和 network dirty state 属于本子系统的候选权威范围
- Version4 NewProjectile 的 slot allocation/default application/owner-scoped identity 初始化链属于 Projectile spawn boundary，但结构 commit owner 仍需整合
- Version4 LeashedEntity registry/section lifecycle 不属于 ProjectileSimulation

proposedTypes:
- ProjectileIdentityComponent, status: proposed
- TrajectoryStateComponent, status: proposed
- ProjectileLifetimeComponent, status: proposed
- ProjectilePenetrationComponent, status: proposed
- ProjectileCollisionQuery, status: proposed
- ProjectileBehaviorSystem, status: proposed
- ProjectileHitCommand, status: proposed
- IProjectileSpawnPort, status: proposed
- IProjectileKillPort, status: proposed
- ProjectileReplicationProjection, status: proposed

sharedTypesForIntegrationReview:
- runtime EntityReference/EntityId
- PlayerEntityId、NpcEntityId 和 owner-kind reference
- slot token/whoAmI、owner-scoped NetworkId、projUUID、replicationId
- WorldSectionId/PVS、TileCoordinate、WorldTime
- ProjectileHitCommand、Kill/Spawn request、replication snapshot 和 tombstone

crossSubsystemReaders:
- CombatAndStatus 读取 Projectile damage/collision hit candidate 和 penetration outcome
- SpatialSimulation/Movement 读取 Projectile collision policy 和轨迹输入
- PlayerGameplay 读取 owner/minion/sentry 能力结果
- FishingAndCatchSimulation 读取 bobber trigger/carrier context
- NetworkSessionAndSectionStreaming 读取 replication projection
- SpawnLifecycleAndLoot 读取 spawn/kill requests

crossSubsystemWriters:
- PlayerGameplay 产生初始 spawn request 和 owner/source input
- CombatAndStatus 返回 hit/reflection/accepted outcome，影响 projectile-local penetration/lifetime
- SpatialSimulation 提供 geometry/contact result，不直接写 Projectile authority
- FishingAndCatchSimulation 消费 bobber event，不直接拥有 Projectile slot
- NetworkSessionAndSectionStreaming 只能通过受控 receive/reconciliation adapter 提交网络输入，不能成为 server authority
- SpawnLifecycleAndLoot 提交 entity/slot/tombstone/child structure result

orderingConstraints:
- PlayerGameplay -> NpcAndTownSimulation -> ProjectileSimulation
- Behavior/AI substep -> environment/movement -> collision query -> hit command -> CombatAndStatus
- accepted hit -> projectile-local penetration/immunity commit -> timeLeft/penetrate terminal check -> Kill request
- Kill commit -> child/drop/effect boundary -> replication tombstone/projection -> network section send
- 不得以文件或目录顺序替代上述 scheduler contract

boundaryChallenges:
- owner 是否只允许 Player，或需覆盖 NPC/trap/world/server
- Fishing bobber 的 carrier/trigger 与 Fishing outcome 的提交边界
- LeashedEntity anchor/section relation 是否共享 EntityReference/NetworkId
- Kill 的 child spawn/drop/effect/tombstone 顺序和最终结构 owner
- 当前 replication snapshot 字段过宽，需避免成为第二个 authority model

evidenceGaps:
- 全部 specialized AI style 的读写与行为覆盖未闭合
- Version4 Kill type-specific 后段事务表未闭合
- 完整 Projectile persistence contract 未找到
- LeashedEntity Remove/StreamNetUpdates 当前为空
- 当前 NLTX network 27/29、section resend、tombstone 和 owner+identity reconcile 未闭合
- 本轮 verifier 未执行

blockingDecisions:
- owner reference model
- projUUID 长期 identity/persistence mapping
- raw AI compatibility state 的保留期限
- ProjectileHitCommand 与 CombatAndStatus 的最终 owner
- Kill/child/drop/network tombstone 事务 owner 和顺序
- FishingAndCatchSimulation 与 bobber carrier 的 commit boundary
- LeashedEntity relation 与共享 ID 类型 owner

notImplemented:
- 本报告提出的所有 proposed Component/System/Query/Command/Adapter/Projection
- Version4 全部 AI style 到 NLTX typed behavior 的闭合迁移
- Version4 Kill 的完整结构/掉落/child/tombstone transaction
- owner+identity/UUID/slot/network replication 的端到端等价 reconcile
- Fishing outcome 和 LeashedEntity lifecycle 的 Projectile 内整合

verifierPlan:
- VER-PROJ-001 至 VER-PROJ-016，覆盖 identity separation、defaults、extraUpdates、collision purity、hit/penetration、Kill ordering、network reconcile、section resend、projection purity、fishing/leashed boundary 和 owner matrix
- 未来执行时遵守仓库 BUILD-CONCURRENCY-1 和 serial dotnet wrapper；本轮不运行
~~~

## 21. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本报告唯一写入目标为：

D:\TRbackup\NLTX\docs\第一轮审查\2026-09-05-version4-projectile-simulation-public-decomposition.md
