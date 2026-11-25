# CR-2026-11-25 无敌窗口通道化与 P0–P3 落地

| 项 | 内容 |
| --- | --- |
| 文档 ID | CR-2026-11-25-invincibility-window-p0-p3 |
| 逻辑域 | 跨域 / Combat + Projectile |
| 产物类型 | 变更记录 |
| 状态 | 已实施（P0–P4 完成；P3-5 观测计数未实现，见 §7 第 7 项） |
| 日期 | 2026-11-25 |
| canonical 路径 | docs/cr/CR-2026-11-25-invincibility-window-p0-p3.md |
| 上游依据 | [无敌窗口执行计划](../plans/2026-11-25-invincibility-window-execution-plan.md)、[共享组件整合审计 §34.2](../reviews/audits/2026-11-02-share-component-consolidation-audit.md) |

## 1. 授权与范围

执行 [无敌窗口执行计划](../plans/2026-11-25-invincibility-window-execution-plan.md) 的 P0–P3：
把「玩家身上一个固定 60 tick 的单值窗口」换成与原版对齐的伤害免疫模型。不改协议包号与载荷，
不做表现层（闪烁 / 免疫透明度），不做闪避族，不建 ImmunityCooldownID.Sets 四个策略位，
不建 NPC 对 NPC 伤害的 255 槽。

| 动作 | 文件 |
| --- | --- |
| 新增 | `Share/Combat/System/ImmunityChannel.cs`、`ImmunityDurationPolicy.cs`、`ImmunityGrantPolicy.cs` |
| 新增 | `Share/Combat/Component/ImmunityWindowComponent.cs`、`AttackerImmunityComponent.cs` |
| 新增 | `Share/Combat/ImmunityWindowExpirySystem.cs`、`AttackerImmunityExpirySystem.cs` |
| 新增 | `Share/Combat/Query/DamageImmunityQuery.cs` |
| 删除 | `Share/Combat/ProtectionWindowSystem.cs`、`Share/Combat/Component/ProtectionWindowComponent.cs` |
| 修改 | `CombatDamageSystem`（开窗点改为 `ImmunityGrantPolicy`）、`DamageRequest`（`Channel`）、注册键与注册表、协作规则 |
| 修改 | `NSSLC.Server/Program.cs`（退役 `PlayerHurtProtectionTicks`）、`PlayerEntityDefinition`、`NpcEntityDefinition` |
| 修改 | `Test/Terraria.Components/Terraria.Combat.ProtectionWindow/Program.cs`（五条口径） |
| 修改 | 四份 README（`Share/Combat`、`Player`、`Share/Attributes`、`NSSLC`） |

## 2. 理由

现状是单值递减窗口：无论打 1 点还是被 Boss 打一管都给 60 tick，熔岩与普通受击共用同一个窗口，
NPC 侧完全没有窗口（史莱姆挂的是永久免疫，等价于 dontTakeDamage，根本打不动）。三个问题
都不是调参能解决的，因为缺的是维度：通道维度、伤害反推维度、攻击者维度。

计划 §2.2 已经记录了两处会持续制造 bug 的接缝，本 CR 沿用同一结论：
CheckAdmission 不得承载目标免疫（ApplyCore 会用已提交回执重查，塞进去会让回执自失效）；
窗口期内的命中若不做攻击者侧预检，每 tick 都要走一遍「入队 → 拒绝 → 回滚」。

## 3. 改动要点

**通道模型（D-1）**：ImmunityChannel { General=0, TileContact=1, BossNoCheese=2, WrongBugNet=3,
Lava=4, PaladinsShield=5, Count=6 }。General 不再用 -1 哨兵，与其余通道同列同索引，
_untilTick[(int)channel] 恒成立，越界不可能发生。

**时间表示（D-2）**：状态是 long[6] 到期戳，不做递减。写放大从 O(激活数 × tick) 降到
O(授予次数)。边界口径是**授予于 N、时长 D → 生效 N..N+D-1（恰好 D 个 tick）、N+D 起放行**；
用严格小于而不是小于等于，因为 tick 从 0 起算，小于等于会让「从未授予」的 0 号哨兵在 tick 0
被判成激活，把该帧所有伤害吞掉。

**时长（D-3）**：ImmunityDurationPolicy.ForHit(appliedDamage, pvp, longInvince) 对齐参考端算式：
PvP 8、擦伤（实际伤害 1）20、其余 40，长无敌变体 40 / 80。调用点恒传 longInvince: false——
饰品属性来源尚未落地，不建没有生产者的输入。

**NPC 分槽（D-4）**：AttackerImmunityComponent 是 EntityRuntimeHandle → untilTick 的稀疏表，
照抄参考端「NPC 侧不分通道」的不对称。键用句柄而不是玩家下标，因为 ECS 没有稳定下标，
而句柄带 generation，槽位复用不会误命中。

**拦截点（D-5）**：DamageImmunityQuery.Check 是无敌语义的唯一实现，纯只读；
DamageAdmissionSystem 的权威裁决与 ProjectileHitResolutionSystem.CanHit 的预检都调它。

**开窗位置**：留在 CombatDamageSystem.ProcessOne 的 Applied 分支内，不拆成独立系统。
计划原文的 ImmunityGrantSystem 方案在实施时被否：开窗必须留在本 tick 的请求循环里，
否则同 tick 的多发命中会全部落地（参考端同 tick 第二发是被挡掉的）。协调器只负责调用位置，
时长规则与组件写入都在 ImmunityGrantPolicy。

**史莱姆行为变更（有意）**：NpcEntityDefinition.BuildSlime 从 DamageImmunityComponent 换成
AttackerImmunityComponent，史莱姆从「打不动」变成「能打，但同一攻击者 40 tick 内打不第二次」。

## 4. 验证

| 命令 | 退出码 | 结果 |
| --- | --- | --- |
| dotnet build src/NSSLC/Terraria.Components.csproj | 0 | 0 警告 / 0 错误；产物 Build/bin/Terraria.Components/Debug/net10.0/ |
| dotnet build src/NSSLC.Server/NSSLC.Server.csproj | 0 | 0 错误；产物 Build/bin/NSSLC.Server/Debug/net10.0/ |
| Terraria.Combat.ProtectionWindow | 0 | 7 条断言全 PASS（收口后复核追加第 8 条 DoT 豁免断言，见 §8） |
| Terraria.Projectile | 0 | 147 PASS / 0 FAIL（含影子装配等价） |
| Terraria.Npc.Ai | 0 | 5 PASS / 0 FAIL |
| Terraria.Entity.Definition | 0 | 7 PASS / 0 FAIL |
| Terraria.ItemUse | 0 | 16 PASS / 0 FAIL |
| Terraria.Server.Lvcs | 0 | 73 PASS / 0 FAIL，RESULT: PASS |
| 真实启动 20 s（NSSLC_WORLD_FILE=src/World/科研.wld） | — | ecs.server.ready 出现 1 次；stderr 0 行 |

Terraria.Combat.ProtectionWindow 的 7 条断言与计划 §1 五条口径的对应关系
（完整对照见计划 §15.2）：

| 口径 | 断言 | 覆盖度 |
| --- | --- | --- |
| 边界精确 | hit-applies-and-opens-window、window-boundary-is-exactly-duration-ticks、never-granted-window-is-not-active-at-tick-zero | 完整（两端都钉） |
| 通道隔离 | channel-isolation-lava-vs-general | 完整 |
| 时长阶梯 | duration-ladder-chip-damage-is-shorter | **部分**：PvP 与长无敌分支无断言 |
| NPC 分槽 | npc-attacker-scoped-immunity | 完整 |
| 有证据 | expiry-system-clears-stale-ticks + §4 验证表 | 完整 |

断言集是骨架而非全覆盖：用户要求「测试只写 10%」，计划 §15.3 记录了这处裁剪的代价——
红测未逐条确认变红，断言的有效性只由收口时一次全绿背书。

## 5. 依赖影响

- **注册键**：combat.protection-window → combat.immunity-window，新增 combat.attacker-immunity；
  EcsCoordinationRules.rules 同步，否则建服期协作校验拒绝。
- **文档**：四份 README 里 ProtectionWindowComponent 的描述全部替换；docs/reviews/ 与
  docs/plans/ 里的历史记录保留原样（它们是当时状态的快照，不是现行描述）。
- **协议与存档**：无变化。免疫窗口不参与任何投影，不落盘。

## 6. 回滚

| 批次 | 回滚方式 | 数据影响 |
| --- | --- | --- |
| P0–P2 | 恢复 ProtectionWindowComponent / ProtectionWindowSystem，开窗内联回 CombatDamageSystem，注册键改回；史莱姆恢复 DamageImmunityComponent | 无（窗口是每 tick 临时状态） |
| P1 时长 | 删除 ImmunityDurationPolicy，恢复宿主常量 | 无 |
| P3 预检 | 删 CanHit 与 DamageImmunityQuery，ProjectileDamageRequestSystem 恢复单次 CheckAdmission | 无 |

各批不改变协议、存档与网络载荷。

## 7. 未闭合

1. **表现层未实现**：闪烁与免疫透明度没有投影，客户无法感知无敌。SuppressBlink 字段已留，
   但没有消费者。闭合条件：渲染侧出现窗口投影，且 SuppressBlink 参与其中。
2. **longInvince 无来源**：参数恒 false。闭合条件：饰品属性链路出现无敌时长修正来源，
   且 ForHit 的对应分支被断言覆盖。
3. **闪避族未实现，权威归属未定**：原版是客户端掷骰 + 62 号包复读。闭合条件：确定 RNG 归属
   并落地。
4. **出生 / 复活保护未接线**：策略表有值，无消费者。闭合条件：PlayerDeathRespawnSystem
   链路验证后，在复活提交处开窗并断言。
5. **ImmunityCooldownID.Sets 四个策略位未建**。闭合条件：队友分摊、日耀护盾、反伤链路
   任一落地时同期建立。
6. **无敌窗口无网络投影**：与投射物网络投影同批。
7. **预检与裁决的差额已可观测（2026-11-25 二次复核收口）**：计数
   `ProjectileHitResolutionSystem.PrecheckAdmittedAuthorityRejected` 由
   `ProjectileDamageResultSystem` 在免疫类拒绝时累加，`Program.cs` 每秒输出
   `ecs.projectile.precheck admittedAuthorityRejected=<n> tick=<n>`（实测 20 s 恒为 0）。
   三条断言固定口径：稳定态恒为 0、只统计免疫类拒绝、窗口与永久免疫都计入。
   剩余：跨 tick 边界的「非负可读」没有专项断言（需要构造预检与裁决跨 tick 的用例）。

## 8. 收口后复核修正（2026-11-25 二次）

复核发现一处**语义反转**，与 §1 第 1 条（通道隔离）同源但方向相反，已在收口后修复：

| 项 | 修复前 | 修复后 |
| --- | --- | --- |
| DoT 开窗 | 已豁免（`ImmunityGrantPolicy` 对 `DamageOverTime` 直接返回 0） | 不变 |
| DoT 被拦 | **未豁免**：`DamageImmunityQuery` 用请求默认的 `General` 通道查窗口，一次普通受击落地后 40 tick 内所有灼烧跳数被 `ProtectionWindowActive` 拒绝 | `Check` 增加 `damageType` 参数，`DamageOverTime` 在查受击窗口与攻击者槽之前直接放行；永久免疫与状态免疫仍生效 |

依据：参考端灼烧的前置条件是 `hurtCooldowns[Lava] <= 0`，不存在 `!immune`（`Player.cs:27887`、`29206`）。
生产点是 `StatusEffectPeriodicSystem` 以自伤合同产出的 `DamageOverTime` 请求，默认通道 `General`，
所以修复前每一次被点燃都只在「目标刚好没有窗口」时掉血——把「被点燃」变成了「免疫灼烧」。

证据：

| 项 | 结果 |
| --- | --- |
| 红测 | 新增 `damage-over-time-is-not-blocked-by-hit-window`，修复前 FAIL（`burn=ProtectionWindowActive`） |
| 反向证据 | 临时删掉查询里的 DoT 豁免分支，该断言重新 FAIL，还原后 PASS |
| 全组回归 | `ProtectionWindow` 8 PASS / 0 FAIL；`Projectile` 94、`Npc.Ai` 4、`Entity.Scheduling` 18、`Entity.Definition` 6、`ItemUse` 15、`Server.Lvcs` 73，均 0 FAIL |
| 预检差额诊断 | 真实启动 20 s 内 `ecs.projectile.precheck` 输出 10 条，`admittedAuthorityRejected=0` 恒定 |
| 构建 | `src/NSSLC/Terraria.Components` 与 `src/NSSLC.Server` 均 0 警告 0 错误 |
| 真实启动 20 s | `ecs.server.ready` 1 次；stderr 0 行 |
