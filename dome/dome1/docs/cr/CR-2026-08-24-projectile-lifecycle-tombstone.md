# CR-2026-08-24 Projectile 生命周期 Tombstone 原因

## 原始需求

“`/D:/TRbackup/NLTX/docs/research/2026-08-24-projectile-lifecycle-gap-and-implementation-design.md`，使用flowstate管理上下文和流程,写代码不是写很多上下文只写重要节点减少到10%的上下文和管理,测试量减少到30% $pua:pua”

## 分级与排期

- 分级：moderate。原因：新增 simulation 状态字段并改变 projectile despawn command，但不改变协议字节。
- 策略：`spec`，首批只闭环 Tombstone 原因，不扩展反射、液体或 PVP。
- 批次：B1 projectile-tombstone-reason，已完成；B2 projectile-identity-allocator，已完成；
  B3 projectile-tombstone-retention，当前迭代执行。

## 影响评估

- 数据库：无。
- 网络协议：无 wire change；KillProjectile 继续由 inactive authoritative snapshot 投影。
- 代码写集：`ProjectileReplicationSnapshot`、`DespawnEntityCommand`、`ProjectileIdentityAllocator`、
  `DomeSimulation`、combat verifier。
- 风险：未标注原因的旧调用点必须回退为 `Administrative`，避免产生空原因。

## 验收证据

- `Expired`、`TileHit`、`Penetrated` 三路径断言位于
  `Test/Terraria.Dome.Combat.Verification/Program.cs`。
- 定向命令：`dotnet run --project Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj -p:UseSharedCompilation=false`
- 结果：exit 0；输出包含 `PASS: player vitals and damage events are bounded and ordered` 和
  `PASS: authoritative combat records, contact damage, collision, cooldown and loot`。
- 新鲜证据：`Build/diagnostics/projectile/lifecycle-tombstone/20260824-160000/`，其中
  `evidence.json` 记录 build/verifier 均为 exit code 0。
- B2 验收：`ProjectileIdentityAllocator` 证明从 1 单调分配到 `int.MaxValue - 1`，耗尽后拒绝，
  不回收已分配 identity；原有 combat verifier 仍通过。
- B3 验收：`ProjectileTombstonePolicy` 固定 30 tick retention，验证 retained/expired exclusive
  boundary 和 `long.MaxValue` overflow clamp；inactive snapshot 保存截止 tick。
- B4 代码节点：`DomeSimulationSnapshot.NextProjectileIdentity` 已加入，内存 snapshot restore 会
  恢复 allocator 游标；磁盘 persistence 暂不写入，避免与工作树已有 v32 sign-tombstone 尾部冲突。
- B4 验收：combat verifier 使用 `nextProjectileIdentity: 17` 恢复 simulation，并确认下一枚
  projectile 获得 identity `17`；Release build 和 verifier 均 exit 0。
- B5 代码节点：客户端 `SyncProjectile` 通过 owner 校验后返回
  `ClientProjectileSyncIgnored`，不进入 authoritative synchronization；protocol verifier exit 0。
- B6 代码节点：Projectile damage 使用 `(projectile identity, target identity)` 局部免疫；同一
  projectile 重复命中被阻止，不同 projectile 对同一 target 可分别命中；combat verifier exit 0。
- B7 代码节点：增加 hostile-only definition type `3`、`CanDamagePlayer` 和 player candidate/
  `DamagePlayerCommand` 路由；当前只验收 eligibility，完整 player integration 保持下一批。
- B7 验收补充：hostile type `3` 命中非 owner player 后，authoritative health `100 -> 90`，owner
  保持 `100`；combat verifier exit 0。
- B8 代码节点：客户端 `KillProjectile` 校验后返回 `ClientProjectileTerminationIgnored`，不再
  报告为 authoritative termination；protocol verifier exit 0。
- B9 代码节点：`CombatReplicationAssembler` 接收可选 authoritative tick；带 retention 截止点的
  inactive projectile 在 `currentTick >= TombstoneRetainedUntilTick` 时不再复制，保留期内仍按
  已发送活动实体的既有 cursor 语义投影 `KillProjectile`。旧快照截止点为 0 时保持兼容。
- B9 验收：协议 verifier 覆盖 retained/expired 两个边界；Release build 0 warnings/0 errors，
  verifier exit 0。
- B10 代码节点：Dome state persistence 在现有 sign tombstone 尾部追加严格校验的
  `NextProjectileIdentity`；读取没有该尾字段的历史状态时安全回退为 `1`，不改 wire protocol。
- B10 验收：persistence verifier 覆盖当前格式 round-trip 和去除 cursor 的 legacy default，
  Release build 0 warnings/0 errors，verifier exit 0。
- B11 代码节点：增加 server-authoritative `BounceProjectileCommand`、bounce component 和
  type `4` 的单次 bounce definition。solid-tile collision 只产生 command；commit 阶段按确定轴
  反射速度、回退到安全位置并递减次数，耗尽后仍产生 `TileHit` tombstone。
- B11 验收：combat verifier 覆盖活动状态、水平速度反射和反射后继续移动；protocol/persistence
  focused verifier 仍通过。
- B12 代码节点：增加显式 `ProjectileLiquidPolicy`。默认 projectile 继续 `Pass`；新增 type `5`
  使用 `Destroy`，液体命中通过 collider 检测产生 `LiquidHit` tombstone，仍走 despawn command
  和 deterministic commit，不修改 liquid world state。
- B12 验收：combat、protocol、persistence 均使用串行 Release build；各 build 为 0 warnings/
  0 errors，liquid tombstone integration、PVS protocol 和 persistence round-trip verifier 均 exit 0。
- B13 代码节点：projectile solid impact 读取既有 slope 编码；slope `1/2` 选择垂直反射轴，
  slope `3/4` 选择水平反射轴，half-brick 选择垂直轴。普通 full tile 的原有运动轴判定不变。
- B13 验收：combat verifier 覆盖 top-slope impact axis；combat、protocol、persistence 串行
  Release build 均为 0 warnings/0 errors，verifier 均 exit 0。
- B14 验收节点：slope verifier 扩展覆盖 top-slope、bottom-slope 和 half-brick 三种轴边界。
  同时修复工作树 `NpcProjectileSpawnSystem.cs` 的 `Arch.Core.World` 命名冲突与错误
  `SimulationVector` 命名空间，使 protocol/persistence 宽回归恢复可构建。
- B14 验收：combat、protocol、persistence 串行 Release build 均为 0 warnings/0 errors，
  三套 verifier 均 exit 0。
- B15 代码节点：`ProjectileTargetEligibilitySystem.CanDamagePlayerTarget` 明确禁止 owner 和
  invalid player handle，hostile non-owner 仍可进入 player damage candidate；simulation route
  使用该 contract，不再只依赖调用方手写 `owner == target` 判断。
- B15 验收：owner/invalid/non-owner 三态 contract verifier 通过；combat、protocol、persistence
  串行 Release build 均为 0 warnings/0 errors，verifier 均 exit 0。
- B16 代码节点：新增 `PlayerDamagePolicy`，将 hostile projectile 分为 `HostileNonPvp` 与
  `PvpOptIn`。type `3` 保持 non-PVP hostile route；type `6` 只有 `WorldRuleState.IsPvpEnabled`
  时才允许 player target，hostile flag 不再自动等价于 PVP。
- B16 验收：registry 顺序、默认关闭 PVP 与显式 policy contract 均通过；combat、protocol、
  persistence 串行 Release build 0 warnings/0 errors，verifier exit 0。

B16 的 PVP rule disk persistence 和跨会话 PVP negotiation 保持 deferred。
- B17 代码节点：NPC ranged commit/spawn 现在只接受 `PlayerDamagePolicy.HostileNonPvp`，拒绝
  PVP-only projectile definition；NPC projectile 保持 `NpcHandle` owner 和独立 typed identity/
  replication projection，不复用 player owner contract。
- B17 验收：NPC verifier 覆盖 PVP-only definition rejection、typed owner 和 replication；combat、
  protocol、persistence focused verifier 串行 Release build 0 warnings/0 errors，均 exit 0。
- B18 代码节点：增加 `ProjectileStateProjection.Project(NpcProjectileReplicationSnapshot)` 的
  明确拒绝边界；V1456 `SyncProjectile` 不会把 `NpcHandle` owner 静默转换成 player slot。NPC
  typed owner/identity 继续只由 `NpcProjectileReplicationSystem` 投影。
- B18 验收：NPC verifier 覆盖 projection rejection；NPC、combat、protocol、persistence 串行
  Release build 0 warnings/0 errors，verifier 均 exit 0。

## 未完成

磁盘格式中的 identity cursor、跨进程恢复后的迟到包隔离、反射/液体/PVP 和完整 definition parity 保持 deferred，
不得由本 CR 的绿色 verifier 推断为已完成。

- B20 代码节点：`DomeSimulation` 增加独立 NPC projectile query、authoritative 创建 API、tick
  移动/行为/lifetime 推进、tile/liquid/player collision 路径和 typed replication collection。
  NPC termination 在共享 despawn commit 中保留 `NpcProjectileReplicationSnapshot` tombstone，
  不经过 player owner dictionary 或 V1456 projection。
- B20 验收：NPC focused verifier 通过真实 `DomeSimulation` tick 验证 expiry -> retained typed
  tombstone；combat focused verifier 继续覆盖 player projectile collision、bounce、liquid 和
  replication，两个 Release build 均 0 warnings / 0 errors。

- B21 代码节点：`CombatReplicationBatch` 增加 `NpcProjectiles` typed contract；
  `CombatReplicationAssembler` 对 NPC projectile 执行 PVS/retention 筛选并保持 `Frames` 为空，
  `DomeServer` 收集该 typed 列表但不把它编码成 V1456 byte frame。
- B21 验收：protocol focused verifier 覆盖 active owner/identity、retained `Expired` tombstone、
  expired filtering 和 zero V1456 frames；server Release build 0 warnings / 0 errors。

- B22 代码节点：新增 NetModules 扩展 kind `NpcProjectileCapabilityOffer/Ack` 和
  `NpcProjectileReplication` envelope。旧 capability offer/ack body 与 V1456 projectile packet
  不变；只有完成基础 capability negotiation 后再协商 NPC projectile version 1。
- B22 验收：protocol focused verifier 覆盖 active/tombstone envelope round-trip、严格字段校验、
  capability offer/ack round-trip 和 session negotiation；未协商 session 仍保持 zero NPC
  projectile V1456 frames。

- B23 代码节点：新增 `NpcProjectileReplicationConsumer`，以 replication ID + revision 做单调
  去重，并在 retention 到期拒绝 inactive tombstone；server `CombatReplicationCursor` 增加
  独立 NPC projectile revision map，确认批次时不与 player projectile cursor 混用。
- B23 验收：protocol focused verifier 覆盖 active 重复帧拒绝、retained `Expired` 接收和到期
  tombstone 拒绝；NPC、combat-protocol、server Release build 均 0 warnings / 0 errors。

- B24 代码节点：`NpcProjectileReplicationConsumer` 增加 snapshot/restore；restore 接收
  `currentTick`，直接丢弃已过 retention 的 inactive tombstone，并保留 revision cursor，避免
  重连后旧 active frame 重新生效。
- B24 验收：protocol focused verifier 覆盖 tombstone restore 后的旧 active frame 拒绝和状态
  保留。Persistence project gate 被 unrelated `TreeCanopyClearanceQuery.cs` 的多维数组
  `ToFrozenSet` 编译错误阻断，未将该错误归因于 B24。

- B25 代码节点：`CombatReplicationCursor` 增加 `NpcProjectileCursorEntry` snapshot/restore；
  `SessionReplicationState` 暴露对应 ownership API。server reconnect 可以在不恢复 player
  projectile cursor 的情况下独立恢复 NPC typed revision cursor。
- B25 验收：protocol focused verifier 覆盖 server cursor 导出、重建 session、旧 revision
  拒绝；protocol/server Release build 均 0 warnings / 0 errors。Host 生命周期 wiring 保持
  下一批，未把 API 级 restore 误报为已接入 reconnect。

- B26 代码节点：`DomeServer` 按 player slot 持有 NPC projectile cursor store；
  `TerrariaProtocolSessionHost` 创建 session 时 restore，`RemoveSessionPlayer` dispose 前 capture。
  只恢复 NPC typed cursor，不恢复 player projectile cursor，也不写入 world persistence snapshot。
- B26 验收：server 和 session-replication Release build 均 0 warnings / 0 errors；现有
  SessionReplication verifier 继续通过。账号 identity keyed reconnect 和跨进程 durable store
  保持 deferred。

- B27 代码节点：`TerrariaProtocolSessionHost` 在账号解析完成后按 `PlayerPersistentState.Uuid`
  restore NPC projectile cursor；断开时 `DestroySessionPlayerCommand` 携带 UUID，
  `DomeServer` 同时写入 slot 和 account-keyed cursor store。slot fallback 保留兼容旧连接时序。
- B27 验收：protocol/server Release build 均 0 warnings / 0 errors；account-keyed restore 不改
  world persistence 或 player projectile cursor。跨进程 durable account store 和 retention cleanup
  保持下一批。

- B29 代码节点：新增独立 `NpcProjectileCursorPersistenceFormat` 和
  `NpcProjectileCursorSaveCoordinator` sidecar；按 account UUID 序列化 cursor entries，带 magic/
  version/数量/重复项/值域校验，save 使用临时文件 flush-to-disk 后 replace。没有修改
  `DomeStatePersistenceFormat`。
- B29 验收：protocol focused verifier 覆盖 account cursor round-trip 和 legacy world header
  rejection；protocol/server Release build 均 0 warnings / 0 errors。DomeServer startup/shutdown
  文件路径绑定保持下一批。

- B30 代码节点：`DomeServer.ConfigureNpcProjectileCursorPersistence(path)` 在 `Start` 前加载
  account cursor sidecar，`Dispose` 在 session/simulation 收尾后保存；默认未配置时保持内存行为。
- B30 验收：persistence verifier 覆盖配置 server 生命周期创建 sidecar、二次 server 加载 sidecar、
  protocol/server/persistence Release build 均 0 warnings / 0 errors。

- B31 代码节点：sidecar load 先验证主文件，主文件无效时验证 `.tmp` recovery candidate；有效
  candidate promotion 为主文件，主文件和 candidate 均无效时显式抛出 `InvalidDataException`。
  读取有效主文件会清理残留 `.tmp`。
- B31 验收：persistence verifier 覆盖损坏主文件 + 有效 tmp 恢复、tmp 清理、双无效候选拒绝；
  persistence/server Release build 均 0 warnings / 0 errors。

- B32 代码节点：`NpcProjectileCursorSaveCoordinator.Compact` 在保存前按 authoritative NPC
  projectile snapshots/current tick 过滤 account cursor；active 和 retention 内 tombstone 保留，
  缺失/过期 ID 删除。`DomeServer.Dispose` 保存前调用 compaction。
- B32 验收：persistence verifier 覆盖 retained ID 保留和 missing ID 删除；persistence/server
  Release build 均 0 warnings / 0 errors。

- B33 代码节点：Persistence verifier 增加非空 account cursor replay：save -> load -> compact
  （保留 retention 内 ID、删除过期 ID）-> save -> 损坏主文件 + 有效 tmp recovery -> load。
- B33 验收：上述组合流程通过；persistence/server Release build 均 0 warnings / 0 errors，
  证明非空 cursor 在重启和中断恢复组合下不丢失、不复活过期 ID。

- B34 代码节点：`ProjectileDefinition`/`ProjectileDefinitionComponent` 增加 authoritative
  `Knockback` 与 `OriginalDamage`；spawn、player/NPC replication projection 和 NPC typed
  envelope 全链路传递并校验该元数据。V1456 player projection 优先读取 definition 来源，未
  修改既有 `SyncProjectile` wire layout。
- B34 验收：Combat.Protocol、Combat、Persistence focused verifiers 与 Server Release
  build 均通过（0 warnings / 0 errors）。本批仅覆盖权威伤害元数据，完整 projectile
  definition/AI/VFX/family parity 仍 deferred。

- B35 代码节点：player `SpawnProjectileCommand.Damage` 不再决定 ECS 或 initial
  replication damage；两处均从已解析的 `ProjectileDefinition` 取值。
- B35 验收：Combat focused verifier 发送 forged damage 后确认 authoritative snapshots
  仍为 definition damage；Server Release build 通过（0 warnings / 0 errors）。

- B36 代码节点：session 为已确认的 active player projectile 记录 sent section；视野替换时仅
  清理离开 PVS 的 projectile revision cursor，避免重新进入时因 revision 未变化而漏发。
- B36 验收：Combat.Protocol focused verifier 覆盖 enter -> leave -> unchanged re-enter，
  构建/运行通过（0 warnings / 0 errors）。NPC typed projectile PVS transition 仍未纳入此批。

- B37 代码节点：NPC projectile 使用独立 sent-section map 与 typed revision cursor；离开 PVS
  时只清除对应 typed cursor，确保 unchanged snapshot 再进入时重发 typed envelope。
- B37 验收：capability-enabled Combat.Protocol focused verifier 覆盖 enter -> leave ->
  unchanged re-enter，构建/运行通过（0 warnings / 0 errors）。

- B38 代码节点：player spawn 的 `LifetimeTicks`、`MaximumPenetration` 与前一批的 `Damage`
  一样不再写入 authoritative component；resolved definition 是唯一生命周期/穿透来源。
- B38 验收：Combat focused verifier 覆盖 forged lifetime snapshot normalization 和 forged
  infinite penetration -> definition one-penetration tombstone，构建/运行通过（0 warnings /
  0 errors）。

- B39 代码节点：player projectile cursor 增加与 NPC cursor 对称的 retention-aware prune，
  并在每次 server combat replication 前执行；关联 sent-section state 同步删除。
- B39 验收：Combat.Protocol focused verifier 覆盖 retained/missing/expired 三种 cursor
  状态；Server Release build 和 verifier 通过（0 warnings / 0 errors）。

- B40 代码节点：新增 `ProjectileBehaviorStateProjection`，将 Linear/Gravity 的 ECS state
  显式映射到 snapshot `Ai0/Ai1/Ai2`，并对未知 behavior、负 phase、非有限值 fail-closed。
  authoritative player snapshot 更新路径接入该映射。
- B40 验收：Combat focused verifier 覆盖 Linear、Gravity、unknown behavior；Combat 与
  Combat.Protocol focused gates 通过（0 warnings / 0 errors）。其余 legacy AI families 仍 deferred。

- B41 代码节点：player projectile spawn 在 server-owned network identity component 中生成
  非空 `Guid`，initial snapshot 与 refresh 路径沿用同一 identity，不重复生成。
- B41 验收：Combat focused fixture 验证初始/refresh snapshot UUID 非空且稳定；Combat 与
  Combat.Protocol Release focused build/run 通过（0 warnings / 0 errors）。V1456 `short Uuid`
  与 `Guid` 的映射尚未定义。

- B42 决策：不从 B41 authoritative `Guid` 推导 V1456 `short Uuid`。旧 `projUUID` 是
  owner-scoped integer lookup key，当前 default types 没有 source-backed `NeedsUUID` 依据；
  legacy short UUID projection 继续 deferred，wire layout 不变。

- B28 代码节点：`CombatReplicationCursor.PruneNpcProjectileSnapshot` 根据 authoritative NPC
  projectile snapshots 和 current tick 清理 cursor；active 状态与 retention 内 tombstone 保留，
  缺失或 retention 到期的 replication ID 删除。`DomeServer` 在每次 combat replication 前调用。
- B28 验收：protocol focused verifier 覆盖 retention 内保留、缺失 ID 删除和 retention 到期删除；
  protocol/server Release build 均 0 warnings / 0 errors。

- B19 代码节点：`NpcProjectileReplicationSnapshot` 增加 typed tombstone reason 和 retention
  tick；`NpcProjectileReplicationSystem.ProjectTombstone` 在实体销毁前保留 NPC owner/identity；
  `NpcProjectileLifetimeSystem` 提供确定性寿命推进。
- B19 验收：NPC focused verifier 覆盖 expiry -> `Expired` inactive snapshot -> retention，且
  继续验证 V1456 projection rejection。B19 不宣称 NPC projectile 已接入 `DomeSimulation`
  的 player-only movement/collision/query；该调度边界保持下一批。
