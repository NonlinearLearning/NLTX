# P07 玩家移动 System 拆分设计

| 字段 | 值 |
|---|---|
| 文档 ID | DOC-2026-09-30-P07-player-mobility-design |
| 逻辑域 | plans / system-decomposition |
| 产物类型 | design |
| 文档状态 | active |
| 设计状态 | proposed |
| 迁移状态 | deferred |
| 验证状态 | not-run |
| 范围 | 权威分区 P07，Player armor-set 与 player mobility 行为边界 |
| 证据基线 | [权威 P07 System 报告](../../system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P07-player-mobility.md) |
| 配套执行计划 | [P07 玩家移动 System 实施执行计划](2026-09-30-p07-player-mobility-system-implementation-plan.md) |
| canonical 路径 | 本文档唯一正式路径 |

## 1. 决策摘要

保留两个概念 owner：

- PlayerArmorSetSystem：集中处理 Beetle 与 Solar armor-set 的计时、tier、orbit 与移动能力转移。Nebula 仅在目标源码行为得到核对后纳入；外部 Buff、随机数、Dust、渲染和 Combat 效果仍由 integration review 指定 owner。
- PlayerMobilitySystem：协调同一个 Player.Update 顺序帧中的跳跃、pulley/rope、dash、wall/slide、carpet、flight/rocket 与 grapple 转移。内部可以有职责清楚的阶段 API，但本报告没有证据支持每个叶子行为成为独立调度 System。

这是一份边界提案，不定义可直接实现的完整签名，不新增事件总线或 Command/Query 类型，也不声称组件或行为已完成迁移。旧 Player 方法在调用者闭包、生命周期和行为验收通过前继续作为兼容入口。

## 2. 证据范围与来源

| 来源 | 证据 | 限制 |
|---|---|---|
| 目标源码 D:/TRbackup/Version4/Terraria/Player.cs | SHA-256 E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86；Player.Update 顺序与被检查的 P07 方法位置见权威报告 | 此工作副本不是 Git checkout；文件哈希只固定本次读取文件 |
| 目标源码 ArmorSetBonuses.cs | SHA-256 E5B960C22E731357D79A444E6438B53055344B394C6C140F4796C8D4136E8DF3 | 不能据此闭合跨文件、动态或运行时调用 |
版本与向量 helper | Version4 `Main.cs` SHA-256 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520；参考 `Main.cs` SHA-256 E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F；两者都声明 `v1.4.5.6`。Version4 `Utils.cs` SHA-256 C6D828318257BC778103458841EC1C187E22587248B9C3A881E7410B6DB6895A；参考 `Utils.cs` SHA-256 D77A5DA5B2E27113BF108E91DC49D104A81701C408B99139209AAEDFEB13F1B1；两者的 `SafeNormalize` 均在零向量/NaN 时返回 fallback，其他情况调用 `Vector2.Normalize` | 版本声明和所读 helper 语义对应，不证明两个完整项目的来源/构建状态相同；不绑定 CPG snapshot |
| Version4 CPG SQLite | manifest SHA-256 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364；SourceSnapshotId 为 null | CPG 导入快照未与上述源码哈希绑定 |
| CPG Query API 本次复查 | `RefreshDoubleJumps` 返回 complete、2 条；`JumpMovement`、`GrappleMovement`、`CanMoveForwardOnRope`、`UpdateJumpHeight`、`SolarDashStart` 的 caller 查询为 partial、0 条；`GetGrapplingForces`、`RefreshMovementAbilities` 各返回 1 条；Nebula helper 返回 3 条 | 这些 call-site 查询的 `ScannedShardCount` 均为 0；索引没有 `SourceSnapshotId`。complete 仅表示索引请求完成，不是 caller 闭包证明 |
| 用户指定完整参考项目 D:/TRbackup/无任何删减通过编译 | `Terraria/Main.cs` 声明 `v1.4.5.6`；`Terraria/Player.cs` SHA-256 367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C；`Terraria.DataStructures/ArmorSetBonuses.cs` SHA-256 B1A56D71964D69BE91FBF81AB4981555D573C62AF4F75F451E1A55EB595C9A91 | 版本标识、P07 阶段锚点、主要方法签名及 Solar/Beetle callback 结构与 Version4 对齐；文件哈希不同。此为静态对应证据，不证明项目来源完全相同、目标 stub 的历史实现唯一，或运行时行为等价。本轮未重建该项目 |
| NLTX P07 候选实现 | 已有 Jump、Dash、Flight、Grapple、Beetle、Solar 状态组件；新增 `PlayerMobilitySystem` 与 `PlayerArmorSetSystem` 的 isolated-core API | 当前只实现 jump parameters/availability refresh、flight/rocket refresh、rope eligibility、grapple force、Solar dash start 和 Nebula tier 更新；没有 Player.Update 编排或旧入口路由，完整状态 writer/lifecycle 仍未闭合 |

本次 CPG Query API 按方法符号查询 `Terraria/Player.cs`，索引健康信息显示 manifest 为上述 SHA-256、`SourceSnapshotId` 为 null。`RefreshDoubleJumps` 的静态调用边有 2 条，而目标源码直接搜索到 12286、13772、15660 共 3 处；索引不能说明缺失的第三处是否未导入、未解析或来自不同源码快照。`JumpMovement`、`GrappleMovement`、`CanMoveForwardOnRope` 和 `UpdateJumpHeight` 查询均为 partial、0 条且 `ScannedShardCount` 为 0，但目标源码分别在 Player.Update:16319、17092、15798、15612 直接调用。`SolarDashStart` 通过 callback/delegate 传入 `DoCommonDashHandle`，CPG 没有解析出调用边。以上 caller closure 均未关闭，零命中不能解释成没有调用者。

完整参考项目的 `Main.cs` 与 Version4 都声明 `v1.4.5.6`。在 `Player.cs` 中，UpdateArmorSets、jump-height、rope、两处 dash、jump、wall/slide/carpet、wing/rocket、grapple 的调用顺序及对应方法签名能够逐一对照；完整参考的 `ArmorSetBonuses.cs` 也在相同 callback 位置调用 Solar 和 Beetle bonus。两份 `Utils.cs` 对 `SafeNormalize` 的零向量/NaN fallback 实现相同。该证据支持“同一声明版本且所检查的结构锚点对应”，不支持把完整参考的方法体直接当成目标行为的已验证规格。

## 3. 概念职责与排除项

| 边界 | 拟负责 | 明确不负责 |
|---|---|---|
| PlayerArmorSetSystem | P07 Beetle/Solar 计数器、tier、orbit 与 Solar dash capability 的状态转移 | Buff 存储/提交、随机源、Dust 和 render、Combat 结算；这些跨域 owner 仍待决定 |
| PlayerMobilitySystem | 单 Player、有序 mobility phase 的转移；按来源阶段区分两次 DashMovement；按原调用点刷新 jump availability | Player 位置/速度最终提交、碰撞和地图查询 owner、Projectile identity 生命周期、Mount owner、Combat 命中结算、Buff/network/persistence |
| Utility/accessory capability | 暂不分配 P07 最终 writer | 不从字段名称、现有候选组件或局部调用推断唯一 owner |
| Player.Update 兼容编排 | 在调用者迁移期间保留原入口与确定顺序 | 不据本次源码闭包推断整个游戏循环的 server/client 权威或全局调度 |

NLTX 中已看到候选状态如 PlayerJumpAvailabilityComponent、PlayerJumpExecutionComponent、PlayerDashStateComponent、PlayerFlightStateComponent、PlayerGrappleRelationComponent、PlayerBeetleArmorStateComponent 和 PlayerSolarArmorStateComponent。新增 System 为部分状态提供 isolated-core writer；其余字段的 writer、reset、序列化、完整生命周期与行为覆盖仍须逐项核对。新增代码尚未接管 Player.Update 或 legacy public API。

## 4. 概念 API 与组合规则

下列名称描述责任，不是已批准的 C# 签名：

| 旧入口/阶段 | 提议的组合职责 | 约束 |
|---|---|---|
| UpdateArmorSets(i) | PlayerArmorSetSystem 更新当前 armor-set 状态并发布供 mobility 消费的 capability 结果 | 必须先于本帧 mobility 消费；Buff/effect 输出 owner 未定 |
| UpdateJumpHeight() | `PlayerMobilitySystem.UpdateJumpParameters` | 已实现显式 jump/mount/equipment 状态输入并更新 mobility modifiers；输入生产者、调用时机与最终 velocity writer 未闭合 |
| RefreshDoubleJumps() | PlayerMobilitySystem.RefreshJumpAvailability | 是状态写入，不是纯 Query；必须保留原调用位置与重复次数 |
| pulley/rope inline block、CanMoveForwardOnRope | PlayerMobilitySystem.ResolvePulleyStep / ResolveRopeEligibility | tile、collision 和 rope helper 的目标行为需核对；不可把默认 false 当成规则 |
| 两处 DashMovement() | PlayerMobilitySystem.ResolveDashStep(frame, phase) | phase 至少区分 first 与 after-jump 两次调用；不能合并为一次 tick |
| JumpMovement() | PlayerMobilitySystem.ResolveJumpStep(frame) | 接触攻击、immunity 与 velocity 写入须拆清 Combat/Spatial owner |
| WallClimbMovement / WallslideMovement / CarpetMovement | PlayerMobilitySystem 中有序 traversal 阶段 | mount、collision、presentation 边界尚未闭合 |
| WingMovement 与 inline rocket logic | ResolveFlightStep / ResolveRocketStep | rocket 行为是 inline 逻辑，提取前须盘点完整读写和 effect 集 |
| GrappleMovement() | ResolveGrappleStep | Projectile relation、mount transition、force 算法及终止条件未闭合 |

API 默认留在所属 System 的直接方法；只有目标架构确实要求跨 owner 延迟提交，才为对应副作用定义 Command/port。只读 Query 必须证明不会写状态、刷新缓存、推进时间或消耗随机数。现阶段不要为职责分类而新增 CQRS 类型。

## 5. 有序帧与依赖

目标 Player.Update 中已核对的顺序：

| 顺序 | 目标源码位置 | 阶段 |
|---|---:|---|
| 1 | 15348 | UpdateArmorSets(i) |
| 2 | 15612 | UpdateJumpHeight() |
| 3 | 15660 | grounded/sliding 条件下 RefreshDoubleJumps() |
| 4 | 15723-15820 | pulley 与 rope inline 阶段；CanMoveForwardOnRope 调用位于 15798 |
| 5 | 16072 | 第一处 DashMovement() |
| 6 | 16319 | JumpMovement() |
| 7 | 16332 | 第二处 DashMovement() |
| 8 | 16335-16341 | WallClimbMovement 或 WallslideMovement，随后 CarpetMovement |
| 9 | 16393 及其条件分支 | WingMovement 与 inline rocket 阶段 |
| 10 | 17092 | GrappleMovement() |

这只确认 Player.Update 内的调用先后，不证明调度注册、跨实体并行安全、server/client 权威、其他入口或全局 phase。后续迁移先保留兼容编排中的顺序，不新增独立调度节点。

依赖提案：

Player.Update 兼容编排
  -> PlayerArmorSetSystem
  -> PlayerMobilitySystem 有序阶段
  -> 待 integration review 指定的 Spatial/Collision、Combat、Projectile、Mount、Buff 与 effect owners

图中的跨域 owner 与交接时机仍属 integration-review，不能当成已确认依赖合同。

## 6. 参考源码中的待核行为

完整参考项目能提供目标 stub 的重建候选。版本声明、所检查的阶段锚点、签名和 callback 结构现已对齐；但目标与参考源码文件哈希不同，CPG 快照也未绑定目标源码，因此参考方法体仍是静态重建线索，不是目标行为等价证明。

| 方法 | 完整参考源码中可见内容 | Version4 目标状态 / 可用结论 |
|---|---|---|
| UpdateJumpHeight | mount active 时使用 mount 的 jump height/speed；否则按 jump boost、装备、werewolf 与 portable stool 累加，再应用 sticky/dazed 降幅 | Version4 与完整参考方法体一致；NLTX `UpdateJumpParameters` 已按该顺序实现，focused verifier 覆盖 mount/non-mount 两路。输入生产者和 Player.Update 路由仍未验证 |
| UpdateBuffs_NebulaBuffs | 从 `buffType` 派生 nebula tier；当 `buffTime == 2 && level > 1` 时降一级、减小 buff 类型并将时间设为 480 | Version4 为空体；参考实现可作重建候选，目标等价仍 `unknown`；Buff owner 与边界未闭合 |
| GetGrapplingForces | 按 legacy slot 顺序遍历 `ai[0] == 2` 且位置不含 NaN 的 grapple projectile；按类型计算控制方向、目标位置及速度，限制最大速度 | Version4 仅给 out 参数默认值；NLTX 有隔离候选算法。输入不验证 Projectile identity，stale slot、多 hook 与速度边界仍 `unknown` |
| RefreshMovementAbilities | 重置 `wingTime`、`rocketTime`、`rocketDelay`；可选调用 `RefreshDoubleJumps` | Version4 为空体；字段写入行为可从参考源码定位，目标 writer/lifecycle 与调用语义仍 `unknown` |
| RefreshDoubleJumps | 清除 down-dash 执行标记；按每种 `hasJumpOption_*` 恢复对应 `canJumpAgain_*` | Version4 方法体存在；NLTX `PlayerMobilitySystem.RefreshDoubleJumps` 已按相同逐项转移实现并通过 isolated-core verifier。调用闭包、legacy 路由和整体等价仍未验证 |
| CanMoveForwardOnRope | 检查前方 tile 是否为 active rope，再以 `Collision.SolidCollision` 判断可通行性 | Version4 返回默认 bool；参考算法有据可查，但 tile 边界与 Collision owner 未定，目标行为仍 `unknown` |
| SolarDashStart | 设置 `solarDashing = true`、`solarDashConsumedFlare = false`；`dashDirection` 参数未参与该状态转移 | Version4 为空体；NLTX `PlayerArmorSetSystem.BeginSolarDash` 已实现状态转移并通过 isolated-core verifier。Solar callback 与 DoCommonDashHandle 的传递关系可静态确认，效果/写入合同仍待验证 |

上表不是可直接复制的行为规格。实施前仍须逐项核对目标调用上下文、字段读写、依赖 API、错误语义及行为结果；目标等价无法验证时继续标 `unknown`，并阻止相应切片切换。目录名中的“通过编译”不作为本任务已独立验证构建的证据。

## 7. 副作用、生命周期与 integration review

已观察的目标源码副作用包括 Player velocity/位置相关变更、碰撞与 NPC helper 调用、Buff 变更、Main.rand、Dust/声音/动画、Projectile slot 读取和 hook 清除、Mount 转换、tile/rope 检查。不得因此将这些共享状态或外部 effect 归给 PlayerMobilitySystem。

以下 owner 和语义仍为 unknown / integration-review：

- Spatial/Collision 的查询 API、权威位置/速度 writer、移动 commit 点与阶段顺序；
- Combat 接触攻击、NPC immunity、伤害随机数及其提交时间；
- Projectile 身份有效性、stale slot、hook 清除及两端生命周期；
- Mount 的 capability 与状态切换 owner；
- Buff、随机、Dust、audio、animation 的权威出口和 replay 去重；
- utility/accessory capability 的 producer、所有 consumer 与 reset；
- spawn/reset/death/disconnect/reconnect、save/restore、network、prediction、rollback、多 world 与异常重试。

任何新增 System-local cache 都需要独立生命周期、失效规则和多 world 隔离合同。

## 8. 提案状态

| 状态 | 值 |
|---|---|
| designStatus | proposed |
| migrationStatus | deferred |
| verificationStatus | not-run |

本设计仍为 proposed，且没有 Player.Update 接线或行为等价结论。NLTX 已新增两个 isolated-core System 与一个 focused verifier；仅验证这些 helper 的局部输入/输出和状态变化。Version4 caller 闭包、跨域 owner、生命周期及整个 P07 行为验证仍未完成，migrationStatus 保持 deferred，verificationStatus 对整体迁移保持 not-run。
