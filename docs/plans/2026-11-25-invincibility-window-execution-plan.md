# 无敌窗口执行计划

**文档 ID**：PLAN-2026-11-25-INVINCIBILITY-WINDOW
**逻辑域**：plans
**产物类型**：plan
**策略**：sequential（P0 是硬前置；P1–P2 相互独立、可分批验收；P3 依赖 P1、P2；P4 依赖 P0–P3）
**状态**：已实施（P0–P4 全部完成；批次状态见 §4，偏差见 §4.1，证据与断言覆盖见 §10、§15）
**上游依据**：
参考客户端反编译源码（D:\TRbackup\最新的客户端，只读参考材料，不入 src/）的逐项提取（§2.1）、
[共享组件整合审计 §34.2](../reviews/audits/2026-11-02-share-component-consolidation-audit.md)（受击无敌窗口的原始决策与已修排序缺陷）、
[服务器系统引入缺口](../reviews/audits/2026-11-06-version4-server-systems-introduction-gap.md)（Player.UpdateImmunity 的映射认定）
**适用约束**：
[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、
[ECS Entity 组织设计约束](../architecture/ECSEntity组织设计约束.md)、
[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)、
[组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)、
[项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md)、
[构建与验证约束](../../Context/项目约束/构建与验证约束.md)、
[Test 验证项目组织](../../Test/README.md)
**范围**：src/NSSLC/Share/Combat/**、src/NSSLC/Projectile/**、
src/NSSLC.Server/Registration/**、src/NSSLC.Server/Program.cs、
Test/Terraria.Components/Terraria.Combat.ProtectionWindow/**
**排除范围**：不改参考客户端；不做表现层（闪烁 / 免疫透明度）；不做闪避族；不做
ImmunityCooldownID.Sets 四个策略位；不做 NPC 对 NPC 伤害；不做无敌窗口的网络投影；
不改协议包号与载荷格式

---

## 1. 执行目标

把当前「玩家身上一个固定 60 tick 的单值窗口」升级为「与原版对齐的伤害免疫窗口模型」，判定口径
五条，缺一条不算达成：

1. **通道隔离**：窗口按 6 个受击通道分槽；`Lava` 通道的命中不受 `General` 窗口影响，反之亦然。
   断言用同一目标、同一 tick，只改通道，验证一次被挡一次放行。
2. **时长由实际伤害反推**：PvP 8 tick；实际伤害为 1 时 20 tick；实际伤害大于 1 时 40 tick；
   长无敌变体翻倍。断言不接受「配置值」与「实测受保护 tick 数」不一致。
3. **边界精确**：授予于 tick N、时长 D 的窗口，在 N..N+D 全部生效，N+D+1 起放行。
   断言必须钉住两端，不能只钉一端。
4. **NPC 侧按攻击者分槽**：攻击者 A 打出的窗口不影响攻击者 B；窗口期的命中不再进入伤害请求。
5. **有证据**：每批给出构建命令、项目、退出码、警告/错误数、产物路径与验证项目 PASS 行；
   未执行项一律记 unknown。

本计划**先把形状改对，再填数值**：P0 只改数据模型与系统分解（行为变化仅一处边界，见 D-2），
P1 才引入原版阶梯。这样每批都能独立回退，且「形状改错」不会和「数值不对」混在同一个提交里。

## 2. 当前基线与证据等级

> 证据等级：参考客户端源码逐行核对（§2.1）+ 本仓库代码静态核对（§2.2）。
> **没有实测证据**——本计划开工前不存在覆盖通道、阶梯与 NPC 侧的断言（§5.2 列出期望失败）。

### 2.1 参考客户端模型（已逐行核对）

无敌帧在原版不是「一个系统」，是四层叠加 + 五个授予源。行号为 D:\TRbackup\最新的客户端 树内行号。

**四层状态**

| 层 | 承载者 | 状态 | 索引维度 | 检查点 |
| --- | --- | --- | --- | --- |
| L1 通用无敌 | 玩家 | `immune` + `immuneTime` + `immuneNoBlink` + `immuneAlpha`（`Player.cs:1403-1417`，递减在 `18970-19008`） | 全来源一视同仁 | `Hurt` 入口 |
| L2 通道冷却 | 玩家 | `hurtCooldowns[6]`（`Player.cs:3143`），枚举见 `Terraria.ID/ImmunityCooldownID.cs`：`General=-1`、`TileContactDamage=0`、`BossNoCheese=1`、`LegacyUnused2=2`、`WrongBugNet=3`、`Lava=4`、`PaladinsShield=5`、`Count=6` | 6 个受击通道 | `Hurt` 入口 |
| L3 攻击者免疫 | NPC | `immune[256]`（`NPC.cs:6451`），递减在 `91445-91451`；`GetImmuneTime(fromWho,time)` 在 `93533-93546`，`fromWho==-1` 填满全部槽位 | **攻击者下标**（255 = 非玩家） | 攻击者的碰撞检测里 |
| L4 弹体自免疫 | 弹体 | `localNPCImmunity[]` + `localNPCHitCooldown`（`Projectile.cs:368/466`，递减 `17375-17386`）；静态档 `perIDStaticNPCImmunity[,]` + `idStaticNPCHitCooldown`（`296/468`） | 弹体×目标 / 弹种×目标 | 弹体 PVE 遍历 `12595` |

**五个授予源**

| 授予源 | 值 | 位置 |
| --- | --- | --- |
| 受击后 | `pvp ? 8 : ((num2 != 1.0) ? (longInvince ? 80 : 40) : (longInvince ? 40 : 20))` | `Player.cs:38668-38677` |
| 闪避（忍术/黑腰带/混乱之脑/神圣套） | `SetImmuneTimeForAllTypes(longInvince ? 120 : 80)`，同时写满 6 个通道槽 | `Player.cs:38292-38400` |
| 出生 / 复活 | PvP 死 300、`ReviveFromDeath` 180、进世界 60，且 `immuneNoBlink` | `Player.cs:37937-37951` |
| 微光闪避 | 在 `Hurt` 最前返回 0；Boss 级 / 入侵 NPC / `CanHitPastShimmer` 弹体不可闪 | `Player.cs:39043-39067` |
| 碰撞连击 | 20 tick 内第 3 次起停发（`_immuneStrikes < 3`），强制 `immuneNoBlink` | `Player.cs:20652-20669` |

**四个必须注意的边界**

- 时长由**减伤后实际伤害**反推，不是常量：打 1 点只给 20 tick，PvP 只给 8 tick。
- 两个检查点语义相反：玩家侧「进函数再返回 0」（`38489-38492`），NPC 侧「根本进不了函数」
  （`NPC.StrikeNPC` 全流程 `NPC.cs:81897-81941` + `StrikeNPC_Inner 81943+` 不读 `immune[]`，
  检查只在攻击方的 `Player.cs:20540/20571`、`Projectile.cs:12636`、`Mount.cs:5278` 等处）。
- `ImmuneTimerOnlyLimitsEffects`（只对 `PaladinsShield`）让「计时器没到期」不等于「拦截」：
  伤害照扣，计时器只限制附加效果（`Player.cs:38489`、`38620`、`38643`）。
- 远程玩家实体不看判定结果，无条件续期通道槽：`flag2 = whoAmI != Main.myPlayer && !pvp`
  （`Player.cs:38674`），纯粹为权威端时钟一致。

**DoT 走独立通道**：熔岩的前置条件是 `hurtCooldowns[Lava] <= 0` 而非 `!immune`（`Player.cs:27887`），
瓦片接触伤害同理（`29206`）。所以「身上有无敌」完全不影响灼烧 / 岩浆 / 贴墙掉血。

**网络投影三处**：玩家受伤 16 号包（`MessageBuffer.cs:1100-1123`）、闪避 62 号包（`2894-2922`）、
NPC 免疫 131 号包（`4158-4172`）。注意**通道 id 不上线**——16 号包不携带 `cooldownCounter`，
通道语义只在权威端成立。

### 2.2 本仓库现状与缺口

| 项 | 现状 | 证据 |
| --- | --- | --- |
| 状态形状 | 单个 `int RemainingTicks` + `Start/Advance/Clear/IsActive` | `Share/Combat/Component/ProtectionWindowComponent.cs:6-33` |
| 推进 | 每 tick 对所有带组件实体 `Advance()` | `Share/Combat/ProtectionWindowSystem.cs:11-23` |
| 授予 | 内联在协调器：`Applied` 分支后 `Start()` | `Share/Combat/CombatDamageSystem.cs:175-181` |
| 拦截 | 准入最后一条，返回 `ProtectionWindowActive` | `Share/Combat/DamageAdmissionSystem.cs:10-13,37-39`，枚举见 `System/DamageRejectionReason.cs:13` |
| 时长 | 宿主常量 60 tick，经构造参数注入 | `NSSLC.Server/Program.cs:63-69,700-709`；`CombatSystemRegistration.cs:30,41,107` |
| 装配 | 只有玩家挂载 | `Registration/PlayerEntityDefinition.cs:59-61` |
| NPC 侧 | **无窗口**；史莱姆挂的是永久免疫标记 `DamageImmunityComponent` | `Registration/NpcEntityDefinition.cs:104`；`CombatDamageSystem.cs:116-121` |
| 通道 | 无。`src` 内 `ImmunityCooldown*`、`hurtCooldowns` 命中 0 | 全仓 grep |
| 弹体自免疫 | **已有**：`ProjectileHitImmunityStateComponent`（弹体×目标 + `Prune(tick)`），等价 L4 的 `localNPCImmunity[]` | `Projectile/Component/ProjectileHitImmunityStateComponent.cs:42-81` |
| 攻击者侧预检 | **已有**：入队前调 `CheckAdmission` | `Projectile/ProjectileDamageRequestSystem.cs:73-90` |
| 已知死路径 | `Clear()` 全仓无调用点；`Start` 的「只延长」语义在窗口内命中被拒时走不到 | 全仓 grep |

**两个会持续制造 bug 的接缝**

- **S1｜CheckAdmission 不能承载目标免疫**：`ApplyCore` 拿已提交回执重查一次
  （`Projectile/ProjectileHitResolutionSystem.cs:143`），而那份回执在
  `CombatDamageSystem.cs:177-181` 刚刚给目标开了窗。把目标免疫塞进 `CheckAdmission`
  会让回执自己判自己不合法，穿透预扣不确认。源码 `164-165` 的注释写的正是这个约束。
- **S2｜窗口期内的命中每 tick 重放空转**：`PhysicsContactCaptureSystem.cs:64-82` 每 tick 对每个
  相交对都产出接触事实、不去重；而「打过就别再打」的记录只在 `Apply`（accepted）里写
  （`ProjectileHitResolutionSystem.cs:177-185`），`Reject` 只回滚穿透（`97-110`）。
  结果：目标在窗口内、弹体持续接触时，是每 tick 一次「入队 → 攻击意图 → 伤害请求 → 拒绝 → 回滚」。

### 2.3 明确不在本计划

- **表现层**：`immuneAlpha` 的 50↔205 闪烁、`immuneNoBlink` 的视觉含义。仓库当前无渲染，
  只保留 `SuppressBlink` 字段供未来投影，不实现效果。
- **ImmunityCooldownID.Sets 四个策略位**（`Retaliate` / `Counter` / `TeamDamageShare` /
  `ImmuneTimerOnlyLimitsEffects`）：依赖队友分摊、日耀护盾与反伤链路，当前都没有消费者。
  造没有消费者的策略位正是 `Context/progress.md` 点名的技术债模式。
- **闪避族**（忍术/黑腰带/混乱之脑/神圣套/微光）：原版是**客户端掷骰 + 62 号包复读**，
  服务端权威下不能照抄这个归属，需单独立项讨论 RNG 归属。
- **NPC 对 NPC 伤害**（`immune[255]`）：`GetHurtByOtherNPCs`（`NPC.cs:93438-93469`）在
  本仓库没有对应链路，本计划不建 255 槽，也不提供 `GrantAll`。
- **无敌窗口的网络投影**：客户看不到窗口，与投射物网络投影同批处理。
- **碰撞连击防刷**（`_immuneStrikes`）：没有碰撞伤害路径，无可防之物。

## 3. 决策记录

### D-1 通道模型：6 个显式槽位，General 不再用 -1 哨兵

**结论**：新增 `ImmunityChannel { General=0, TileContact=1, BossNoCheese=2, WrongBugNet=3,
Lava=4, PaladinsShield=5, Count=6 }`。`General` 与其余通道同列、同数组、同索引。

**理由**：原版 `General = -1` 是哨兵值，导致每次访问都要先分支再索引（`Player.cs:38487`、`38669`），
数组本身永远收不到 -1。ECS 里没有理由保留一个靠「调用方记得先分支」来维持的不变量。
把 `General` 落在 0 号后，`_untilTick[(int)channel]` 恒成立，越界不可能发生。
原版的 `LegacyUnused2` 不移植，槽位总数恰好仍是 6。

**被否方案**：沿用 `-1` 哨兵 + `hurtCooldowns[6]` 双结构——判定与存储分成两套索引，
正是[组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)要消除的「同名不同义」。

### D-2 时间表示：到期戳 untilTick，不做递减

**结论**：状态是 `long[] _untilTick`（按通道索引，0 = 未激活）。
`Grant(c, tick, duration)` 置 `_untilTick[c] = tick + duration`；
`IsActive(c, tick) => tick <= _untilTick[(int)c]`。窗口**从授予 tick 起算即生效**。

**理由**：递减模型每 tick 必须写每一个激活中的组件，写放大是 `O(激活数 × tick)`；
到期戳只在授予时写一次，读的时候比一次整数，写放大降到 `O(授予次数)`。
原版用递减是因为它是逐帧游戏循环、字段直接暴露给渲染；ECS 里没有这个约束。

**边界惯例（必须钉住）**：授予于 N、时长 D → 生效 N..N+D-1，N+D 起放行。
即 D 就是受保护的 tick 数（含授予 tick），`IsActive` 用 `t < untilTick`。

**实施期修正**：本节初稿写的是「N..N+D、N+D+1 起放行」，那是 D+1 个 tick，与同句「D 就是受保护的
tick 数」自相矛盾。核对参考端递减顺序后定为 `N..N+D-1`：参考端在 tick N 把 `immuneTime` 设为 D，
tick N+1 先递减到 D-1，到 tick N+D 才递减到 0 放行（`Player.cs:38672` 授予、`18974` 递减）。

**0 是哨兵，因此必须用严格小于**：宿主 tick 从 0 起算（`Program.cs:1069` 的 `server.Update(tick++)`），
若用 `t <= untilTick`，从未授予过窗口的实体会在 tick 0 被判成激活（`0 <= 0`），
把该帧所有伤害吞掉。`AttackerImmunityComponent` 与 `ProjectileHitImmunityStateComponent` 同步改为
`t < untilTick`、`Prune` 用 `untilTick <= tick`。

**这意味着 P0 有一处行为变化**：现有递减实现在授予于 N、时长 60 时，实际受保护区间是
N+1..N+59（59 个 tick），因为第 N+60 tick 的推进先于伤害裁决。改成到期戳后是 N..N+59
（60 个 tick）。**这是修 bug，不是引入差异**——「配置 60 就给 60」才对得上
[审计 §34.2](../reviews/audits/2026-11-02-share-component-consolidation-audit.md) 声称的
「保护时长与配置值严格相等」。因此 P0 必须同步改掉
`Test/Terraria.Components/Terraria.Combat.ProtectionWindow/Program.cs:94-102` 那条把
递减边界当预期的断言，并新增两端边界断言。

**不采用双列表**：注册列表 / 激活列表是在 Arch 的 archetype 查询之上手搓第二份索引，
会引入「实体销毁后列表留死句柄」这类影子状态，而
[ECS Entity 组织设计约束 §4.1](../architecture/ECSEntity组织设计约束.md) 要求
「各索引、事件订阅和缓存必须明确在组件移除、实体删除和世界卸载时如何清理」、
且「辅助索引必须指向同一份状态」。真正的「两个集合」在 Arch 里的正确表达是两个 archetype
（一个 tag 组件的有无），而组件增删是跨 chunk 搬行的结构性变更，用「省一次整数比较」换它
在这个实体规模上是负优化。

### D-3 时长策略表化，但长无敌输入先恒为 false

**结论**：`ImmunityDurationPolicy` 把原版 `Player.cs:38668` 的算式表达成具名常量与一个
`ForHit(appliedDamage, pvp, longInvince)`。阶梯为 8 / 20 / 40 / 40（长无敌+1点）/ 80（长无敌+多点）/ 120 / 180 / 300。

**理由**：固定 60 让「打 1 点」和「被 Boss 打一管」给同样的保护，也让 Lava 通道失去意义。

**longInvince 的处理**：本批恒传 `false`，参数位保留。原版的 `longInvince` 来自十字项链
（类型 554）、星星面纱（862）与 6210 系饰品（`Player.cs:14695/14732/15260/15702`），
本仓库没有饰品属性来源。**不建一个没有生产者的输入**——参数存在、调用点恒定，
等饰品落地再接，避免重演 `BuffApplyRequestComponent` 那种「有消费者无生产者」的状态。

### D-4 NPC 侧按攻击者分槽，且不分通道

**结论**：新增 `AttackerImmunityComponent`，`Dictionary<EntityRuntimeHandle, long>` 稀疏表，
键是攻击者运行时句柄，值是 `untilTick`。

**理由**：原版 `NPC.immune[256]` 按玩家下标索引（`NPC.cs:6451`），ECS 里没有稳定的玩家下标，
必须换成句柄。`EntityRuntimeHandle` 已带 generation，槽位复用不会误命中
（[ECS Entity 组织设计约束 §3.2](../architecture/ECSEntity组织设计约束.md)）。

**为什么不给 NPC 也分通道**：原版 NPC 侧就是单值、无通道维度（`GetImmuneTime` 只按 `fromWho` 写一格）。
「顺手纠正」这个不对称等于造一个没有参考依据的维度，且立刻要面对「同一攻击者用不同通道打同一
NPC 该不该各算一份」这个原版没回答的问题。本计划照抄不对称，理由写进组件注释。

**形状先例**：`ProjectileHitImmunityStateComponent` 已是「稀疏表 + `Prune(tick)`」，且它是
**组件**而不是注入的可变队列，符合
[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md) 规则 3 的
「把队列放成组件」；规则 2 支持域内 tick 推进直接读写组件。

### D-5 拦截点：两层同规则，但 CheckAdmission 不承载目标免疫

**结论**：
- 抽 `DamageImmunityQuery.IsBlocked(target, channel, tick) → DamageRejectionReason`，纯只读，
  放 `Share/Combat/Query/`。
- `DamageAdmissionSystem` 调它做**权威裁决**。
- 投射物新增 `ProjectileHitResolutionSystem.CanHit(target, channel, tick)`，只读，
  供 `ProjectileDamageRequestSystem` 在 `CheckAdmission` 之后「别入队」。
- **CheckAdmission 的签名与职责不变**，并在其注释里写明为什么不能加目标免疫（对齐 S1）。

**理由**：S1 是硬约束——把目标免疫放进 `CheckAdmission` 会让刚开窗的那份回执自己失效。
S2 是成本约束——没有攻击者侧预检，窗口期内每 tick 都要走一遍完整的请求-拒绝-回滚。

**明确备案**：预检是**经济性优化，不是保证**。预检在 Simulation 跑，看到的是上一 tick 末的窗口；
裁决在 Commit 跑，中间隔一次 `ImmunityWindowExpirySystem`。因此预检可能放行而裁决拒绝，
这个差额必须可观测（§8.3），且绝不允许反向——预检拒绝而裁决本应放行的命中被静默丢弃。
任何「仅预检」的实现都算缺陷。

**被否方案**：仅攻击者侧 pre-hit（原版做法）——原版把这个谓词复制到至少 9 个调用点
（`Player.cs:20540/20571/20700/20738/20775/21297/21364`、`Projectile.cs:12636`、`Mount.cs:5278`、
`NPC.cs:55622`），在 ECS 里意味着每个伤害来源都要记得查，漏一个就是能被无敌帧穿透的后门；
且攻击者看不见 `ExpectedTargetLife` 快照与同 tick 多请求竞争。

## 4. 批次与状态

| 批次 | 状态 | 前置 | 交付目标 | 判据 |
| --- | --- | --- | --- | --- |
| P0 形状与红测 | 已完成 | 无 | 通道化 + 到期戳 + 开窗拆系统，行为变化只有 D-2 那一个边界 | §1 第 3 条；§5.3 |
| P1 时长阶梯与通道接线 | 已完成（P1-5 见 §15） | P0 | 时长由实际伤害反推；通道随请求走到裁决 | §1 第 1、2 条 |
| P2 NPC 攻击者槽 | 已完成 | P0 | 攻击者分槽生效；史莱姆从永久免疫改为可受击 | §1 第 4 条 |
| P3 拦截点收口 | 已完成（P3-5 于二次复核补落，见 §4.1） | P1、P2 | 预检只减少请求；CheckAdmission 职责不变且有守门注释 | §8.3 未全过（`CanHit` 无状态断言未补，见 §12） |
| P4 验证与收口 | 已完成（验证为精简集，见 §15） | P0–P3 | 文档与注册键同步；断言按用户要求只铺 10% | §15 |

### 4.1 实施偏差记录

| 计划项 | 实际做法 | 理由 |
| --- | --- | --- |
| P0-5 拆出 `ImmunityGrantSystem` | 改为静态策略类 `ImmunityGrantPolicy`，仍由协调器在 `Applied` 分支后调用 | 开窗必须在同 tick 的请求循环内完成，独立系统只能排在循环之后，会让同 tick 多发命中全部落地；协调器仍不持有任何时长规则 |
| P0-6 `DamageAdmissionSystem.Check` 改签名 | 未改。目标免疫移入只读查询 `DamageImmunityQuery.Check`，准入类保持纯函数 | 准入是纯函数、免疫查询要世界状态；混在一起会让准入类被迫持有世界引用 |
| P0-7 注册键 `combat.immunity-grant` | 未新增；新增 `combat.attacker-immunity` | 没有独立的授予系统，就不该有一个指向空气的注册键 |
| P1-5 环境伤害接 `Lava` / `TileContact` 通道 | **未接线**：`src` 内不存在熔岩与瓦片接触的实体伤害生产点 | 不为不存在的生产者建输入（同 D-3 对 `longInvince` 的处理）。通道枚举与时长表已就位，生产者出现时直接传参即可 |
| P3-1 `DamageImmunityQuery.IsBlocked(target, channel, tick)` | 实际签名是 `Check(world, target, attacker, channel, tick)`，返回 `DamageRejectionReason` 而不是 `bool` | 它同时吸收永久免疫、状态免疫、通道窗口与攻击者槽四种原因；返回布尔会把「为什么被挡」丢掉，调用方只能重新判断一遍 |
| P3-2 `CanHit(target, channel, tick)` | 实际签名带弹体：`CanHit(projectile, target, channel, tick)` | 弹体自免疫（L4）也是「现在打不上」的一种，漏掉它会让窗口期内的弹体自免疫照样入队 |
| P3-5 预检拒绝计数进诊断 | 首轮未实现；**2026-11-25 二次复核补落**：`ProjectileHitResolutionSystem.PrecheckAdmittedAuthorityRejected` 累加「预检放行而裁决按免疫拒绝」的次数，`ProjectileDamageResultSystem` 在拒绝分支调用；`Program.cs` 的每秒诊断输出 `ecs.projectile.precheck` | 首轮按「无消费者不建计数器」跳过，但 §8.2 明确要求该差额可判定，属于计划内任务而非可选项。只统计免疫类拒绝，阵营/目标已死/减伤归零不算差额 |
| P4-1 断言扩展到五条口径全覆盖 | 只铺骨架 8 条 | 用户明确要求「测试只写 10%」 |
| 收口后追加 · DoT 不吃受击窗口 | `DamageImmunityQuery.Check` 增加 `damageType` 参数，`DamageOverTime` 直接放行；`ProjectileHitResolutionSystem.CanHit` 与 `ProjectileDamageRequestSystem` 同步传 `attack.Payload.Type` | 收口复核发现语义反转：开窗只发生在一次性打击落地后，但窗口随后会挡掉灼烧跳数，等于「刚被打过 = 火烧不疼」。参考端灼烧前置条件是 `hurtCooldowns` 而非 `!immune`（`Player.cs:27887`），本仓库此前只在**开窗**侧豁免了 DoT，**拦截**侧漏掉 |

依赖图：

    P0 ──┬──→ P1 ──┐
         └──→ P2 ──┴──→ P3 ──→ P4

## 5. P0 形状与红测

### 5.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P0-1 | 记录基线：两项目构建 + `Terraria.Combat.ProtectionWindow` + 真实启动 20 s，填进 §10 | — |
| P0-2 | 新增 `ImmunityChannel` 枚举（D-1 的 6 槽，`General=0`） | `Share/Combat/System/` |
| P0-3 | 新增 `ImmunityWindowComponent`：`long[Count] + SuppressBlink`，`Grant/IsActive/Clear` 按 D-2；删 `ProtectionWindowComponent` | `Share/Combat/Component/` |
| P0-4 | `ProtectionWindowSystem` → `ImmunityWindowExpirySystem`：只做过期归零，不承担判断 | `Share/Combat/` |
| P0-5 | 新增 `ImmunityGrantSystem`：消费本 tick `CombatDamageResultQueue` 的 `Applied`，按策略开窗；从 `CombatDamageSystem` 删掉 `175-181` 与 `_protectionWindowTicks` 字段 | `Share/Combat/`、`CombatDamageSystem.cs` |
| P0-6 | `DamageAdmissionSystem.Check` 签名改为 `(request, targetLife, ImmunityWindowComponent?)`，内部按 `request.Channel` 取槽 | `Share/Combat/DamageAdmissionSystem.cs` |
| P0-7 | 注册键 `combat.protection-window` → `combat.immunity-window`，新增 `combat.immunity-grant`；`ImmunityGrantSystem` 排在 `combat.damage-request` 之后 | `EcsServerRegistrationKeys.cs`、`CombatSystemRegistration.cs` |
| P0-8 | 时长仍取宿主常量 60，但改成 `ImmunityDurationPolicy` 的占位常量（数值不变，P1 才换算式） | `NSSLC.Server/Program.cs` |

### 5.2 期望失败（先红）

P0 开工前先落下列断言并确认它们**失败**。任何一条没有按预期变红，说明断言没钉住缺口：
先改断言，再动 `src/`。

| 断言 | 期望 | 失败原因 |
| --- | --- | --- |
| `window-boundary-inclusive` | FAIL | 现有递减实现给 59 tick，到期戳给 60 tick（D-2） |
| `channel-isolation-lava-vs-general` | FAIL | 没有通道，Lava 命中被 General 窗口挡掉 |
| `duration-ladder-chip-vs-normal` | FAIL | 固定 60，1 点与 10 点给同样的保护 |
| `npc-attacker-scoped-immunity` | FAIL | NPC 无窗口组件；史莱姆被永久免疫标记整体挡死 |
| `prequery-vs-authority-observable` | FAIL | 预检与裁决的差额无法观测（无预检，也无计数） |

### 5.3 退出条件

- 基线两项目构建 0 warning / 0 error，六个验证项目全绿并记录在 §10；
- 五条新断言按 §5.2 全部失败；
- 本批只动断言文件与 `src/` 的形状，不动时长数值（仍 60）；
- `CombatDamageSystem` 不再持有任何保护窗口字段（用编译错误证明，而不是靠阅读证明）。

## 6. P1 时长阶梯与通道接线

### 6.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P1-1 | 新增 `ImmunityDurationPolicy`：八档具名常量 + `ForHit(appliedDamage, pvp, longInvince)`，算式对齐 `Player.cs:38668` | `Share/Combat/System/` |
| P1-2 | `DamageRequest` 增 `ImmunityChannel Channel`（可选参数，默认 `General`）；**不进 `DamageRequestKey`** | `Share/Combat/Command/DamageRequest.cs:9-77` |
| P1-3 | `CombatAttack` 增 `ImmunityChannel Channel`（可选参数，默认 `General`）；`CombatAttackCatalog` 不用改 | `Share/Combat/System/CombatAttack.cs:10-28` |
| P1-4 | 攻击请求转伤害请求时把定义上的通道带过去 | `Share/Combat/CombatAttackRequestSystem.cs` |
| P1-5 | 环境伤害（熔岩、瓦片接触）走 `Environmental` + `Lava` / `TileContact` 通道 | 环境伤害生产处 |
| P1-6 | `ImmunityGrantSystem` 改用 `ForHit`；`pvp` 取请求载荷里的敌我语义，`longInvince` 恒 `false`（D-3） | `Share/Combat/ImmunityGrantSystem.cs` |
| P1-7 | 退役 `PlayerHurtProtectionTicks` 常量 | `NSSLC.Server/Program.cs:63-69` |

### 6.2 验证

- `duration-ladder-chip-vs-normal` 转绿：实际伤害 1 → 20 tick，实际伤害 2 与 10 → 都是 40 tick；
- `duration-ladder-pvp`：PvP 语义下的命中给 8 tick；
- `channel-isolation-lava-vs-general` 转绿；
- 端到端：同一目标先吃 `General` 窗口，再吃 `Lava` 请求，生命继续下降。

### 6.3 退出条件

- 阶梯四条断言全绿；
- `DamageRequestKey` 的形状与既有去重用例不变（通道不进 key）；
- 三个既有验证项目（`Terraria.Projectile`、`Terraria.Npc.Ai`、`Terraria.Entity.Definition`）无回归。

### 6.4 风险

| 风险 | 规避 |
| --- | --- |
| 通道随请求传播后，某些生产点忘了赋值，全部落到 `General` | 默认值就是 `General`，行为退化为现状而非崩溃；P1-5 用断言钉住环境伤害必须显式声明 |
| `pvp` 语义被误解为「玩家对玩家」 | 只在 `ImmunityDurationPolicy` 一处解释，不散落在调用点 |

## 7. P2 NPC 攻击者槽

### 7.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P2-1 | 新增 `AttackerImmunityComponent`：稀疏表 + `IsActive/Grant/Prune`，**不提供** `GrantAll`（§2.3） | `Share/Combat/Component/` |
| P2-2 | 新增 `AttackerImmunityExpirySystem`：Commit 最前 `Prune(tick)` | `Share/Combat/` |
| P2-3 | `ImmunityGrantSystem` 在目标带 `AttackerImmunityComponent` 时同时写攻击者槽 | 同 P0-5 落点 |
| P2-4 | 移除史莱姆定义里的 `DamageImmunityComponent` | `NSSLC.Server/Registration/NpcEntityDefinition.cs:104` |
| P2-5 | `DamageImmunityComponent` 保留但清空生产点，并在组件注释里写明它现在等价于「永久不可受击」，与无敌帧无关 | `Share/Combat/Component/DamageImmunityComponent.cs` |

### 7.2 验证

- `npc-attacker-scoped-immunity` 转绿：A 打出的窗口内 B 仍能造成伤害；
- 史莱姆可被弹体扣血，且扣血后 40 tick 内同一攻击者打不动；
- 攻击者句柄复用（generation 不同）不会误命中——断言用一个已销毁再分配的句柄。

### 7.3 退出条件

- NPC 侧三条断言全绿；
- `DamageImmunityComponent` 在生产代码里零生产点（grep 证明）；
- 无 `Dictionary` 被注入到任何系统构造参数（对照
  [ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md) 规则 3 的判定清单）。

### 7.4 风险

| 风险 | 规避 |
| --- | --- |
| 史莱姆从「打不动」变成「能打」，现有伤害测试的期望值全变 | 这是**有意的行为变更**，写进 CR 的变更说明；受影响断言在同一批更新，不留在下一批 |
| 稀疏表无上限增长 | `Prune` 在 Commit 最前无条件跑；断言过期项被移除 |

## 8. P3 拦截点收口

### 8.1 任务

| ID | 动作 | 落点 |
| --- | --- | --- |
| P3-1 | 新增 `DamageImmunityQuery.IsBlocked(target, channel, tick)`，纯只读，无副作用；`DamageAdmissionSystem` 改为调它 | `Share/Combat/Query/` |
| P3-2 | 新增 `ProjectileHitResolutionSystem.CanHit(target, channel, tick)`：只读，只回答「现在打得上吗」 | `Projectile/ProjectileHitResolutionSystem.cs` |
| P3-3 | `ProjectileDamageRequestSystem` 在 `CheckAdmission` 之后调 `CanHit`，为假则不入队、不预扣 | `Projectile/ProjectileDamageRequestSystem.cs:73-90` |
| P3-4 | 在 `CheckAdmission` 注释里写明：**目标免疫不得加入此处**，因为 `ApplyCore` 会用已提交回执重查（S1） | `ProjectileHitResolutionSystem.cs:36-76` |
| P3-5 | 预检拒绝计数进诊断（预检放行而裁决拒绝的次数可观测） | 诊断端口 |

### 8.2 验证

- 窗口期内的稳定态：`ProjectileDamageResultSystem` 的 `Reject` 分支调用次数为 0
  （即 S2 的空转消失）；
- 预检放行但裁决拒绝的次数**恒为 0**（除跨 tick 边界），且该计数可读；
- 穿透弹穿过带窗口的目标后，穿透预算与命中次数与改动前一致（不因预检而多扣）。

### 8.3 退出条件

- 三条断言全绿；
- `CheckAdmission` 的签名与既有 4 个 `ProjectileHitAdmissionResult` 分支完全未变（diff 证明）；
- `CanHit` 不修改任何状态（代码审查 + 断言：调用前后目标组件逐字段相等）。

## 9. P4 验证与收口

### 9.1 任务

| ID | 动作 |
| --- | --- |
| P4-1 | 把 `Test/Terraria.Components/Terraria.Combat.ProtectionWindow/Program.cs` 的 5 条断言扩展到 §1 五条口径全覆盖 |
| P4-2 | 更新 `src/NSSLC/Share/Combat/README.md`、`src/NSSLC/Player/README.md`、`src/NSSLC/README.md`、`src/NSSLC/Share/Attributes/README.md` 里 `ProtectionWindowComponent` 的描述 |
| P4-3 | 按 [项目文件组织设计约束](../../Context/架构约束/项目文件组织设计约束.md) §6 逐条核对新增文件落位 |
| P4-4 | 写 CR：变更说明含源路径、目标路径、依赖影响与回滚方式 |
| P4-5 | 更新 `Context/progress.md` 本轮条目（L2，2 行；未闭合写成可判定条件） |

### 9.2 退出条件

- §1 五条口径各自有至少一条断言，且全绿；
- 四份 README 不再出现 `ProtectionWindowComponent` 的过期描述（grep 证明）；
- 真实启动 20 s，`ecs.server.ready` 出现且 stderr 0 行。

## 10. 验证与证据

每批必须填满本表，未执行的写 unknown，不填通过（[构建与验证约束](../../Context/项目约束/构建与验证约束.md)）。

> **证据口径**：P0–P4 在同一次连续实施中完成，中间批次没有单独留档的构建记录。
> 下表只记录**收口时实测到的那一次全组运行**，按批次分行的历史数值一律记 unknown，
> 不据事后运行倒填。

| 批次 | 命令 | 退出码 | 警告 | 错误 | 产物 / 结果 |
| --- | --- | --- | --- | --- | --- |
| P0–P3 | （中间批次单独运行） | unknown | unknown | unknown | unknown——未留档，不倒填 |
| 收口 | `dotnet build src/NSSLC/Terraria.Components.csproj -v m` | 0 | 0 | 0 | `Build/bin/Terraria.Components/Debug/net10.0/` |
| 收口 | `dotnet build src/NSSLC.Server/NSSLC.Server.csproj -v m` | 0 | 0 | 0 | `Build/bin/NSSLC.Server/Debug/net10.0/` |
| 收口 | `dotnet run --project Test/Terraria.Components/Terraria.Combat.ProtectionWindow --no-build --no-restore` | 0 | — | — | 7 条断言全 PASS |
| 收口 | `dotnet run --project Test/Terraria.Components/Terraria.Projectile --no-build --no-restore` | 0 | — | — | 147 PASS / 0 FAIL |
| 收口 | `dotnet run --project Test/Terraria.Components/Terraria.Npc.Ai --no-build --no-restore` | 0 | — | — | 5 PASS / 0 FAIL |
| 收口 | `dotnet run --project Test/Terraria.Components/Terraria.Entity.Definition --no-build --no-restore` | 0 | — | — | 7 PASS / 0 FAIL |
| 收口 | `dotnet run --project Test/Terraria.Components/Terraria.ItemUse --no-build --no-restore` | 0 | — | — | 16 PASS / 0 FAIL |
| 收口 | `dotnet run --project Test/NSSLC.Server/Terraria.Server.Lvcs --no-build --no-restore` | 0 | — | — | 73 PASS / 0 FAIL，`RESULT: PASS` |
| 收口后复核（2026-11-25 二次） | `dotnet build src/NSSLC/Terraria.Components.csproj -v q` / `src/NSSLC.Server` | 0 | 0 | 0 | 两项目均 0 警告 0 错误 |
| 收口后复核（2026-11-25 二次） | `Terraria.Combat.ProtectionWindow` | 0 | — | — | **8 PASS / 0 FAIL**（新增 DoT 豁免断言） |
| 收口后复核（2026-11-25 二次） | `Terraria.Projectile` / `Npc.Ai` / `Entity.Scheduling` / `Entity.Definition` / `ItemUse` | 0 | — | — | 91 / 4 / 18 / 6 / 15 PASS，0 FAIL |
| 收口后复核（2026-11-25 二次） | 真实启动 20 s | — | — | — | `ecs.server.ready` 1 次；stderr 0 行 |
| 收口 | 真实启动 20 s（`NSSLC_WORLD_FILE=src/World/科研.wld`） | 0 | — | — | `ecs.server.ready listen=127.0.0.1:7777` 出现 1 次；stderr 0 行 |

## 11. 回滚

| 批次 | 回滚方式 | 数据影响 |
| --- | --- | --- |
| P0 | 恢复 `ProtectionWindowComponent` 与 `ProtectionWindowSystem`，把开窗重新内联回 `CombatDamageSystem`，注册键改回 | 无（窗口是每 tick 临时状态，不落盘） |
| P1 | 删除 `ImmunityDurationPolicy`；`Channel` 参数删掉后默认值路径自然消失；恢复宿主常量 | 无 |
| P2 | 恢复史莱姆的 `DamageImmunityComponent`，删 `AttackerImmunityComponent` 与其过期系统 | 无 |
| P3 | 删 `CanHit` 与 `DamageImmunityQuery`，`ProjectileDamageRequestSystem` 恢复单次 `CheckAdmission` | 无 |

各批独立提交、逐批回退。四批都不改变协议、存档与网络载荷——无敌窗口当前不参与任何投影。

## 12. 未闭合（本计划完成后仍在）

1. **表现层未实现**：闪烁与免疫透明度没有投影，客户无法感知无敌。
   闭合条件：渲染侧出现窗口投影，且 `SuppressBlink` 参与其中。
2. **ImmunityCooldownID.Sets 四个策略位未建**。
   闭合条件：队友分摊、日耀护盾、反伤链路任一落地时，同期建立对应策略位并有消费者。
3. **longInvince 无来源**：参数恒 `false`。
   闭合条件：饰品属性链路出现无敌时长修正来源，且 `ForHit` 的对应分支被断言覆盖。
4. **闪避族未实现，且权威归属未定**：原版是客户端掷骰 + 62 号包复读。
   闭合条件：确定 RNG 归属（服务端掷骰、客户端只做表现）并落地。
5. **出生 / 复活保护未接线**：策略表有值，但没有消费者。
   闭合条件：`PlayerDeathRespawnSystem` 链路被验证后，在复活提交处开窗并断言。
6. **无敌窗口无网络投影**：与投射物网络投影同批。
   闭合条件：出现窗口投影系统与出站断言。
7. **预检与裁决的差额已可观测，跨 tick 边界未专项断言**：计数与每秒诊断
   `ecs.projectile.precheck` 已落地（实测 20 s 恒为 0），三条断言钉住「稳定态为 0」「只计免疫类拒绝」
   「窗口与永久免疫都计入」。§8.2 的「`CanHit` 调用前后目标组件逐字段相等」仍无断言。
   闭合条件：补跨 tick 用例与只读断言。

## 13. 风险与规避

| 风险 | 触发条件 | 规避 |
| --- | --- | --- |
| 目标免疫被后人塞进 CheckAdmission | 有人觉得「都是准入，放一起」 | P3-4 写守门注释；§2.2 S1 记录成因与后果；`prequery-vs-authority-observable` 会在回执自失效时变红 |
| 到期戳边界写成 until-1 或 until+1 | 直接照抄递减实现的心算 | D-2 写死边界惯例；`window-boundary-inclusive` 两端都断言 |
| 通道默认值掩盖漏赋值 | 新生产点忘了写 `Channel` | 默认 `General` 是安全退化；环境伤害用断言强制显式声明 |
| NPC 稀疏表泄漏 | `Prune` 未跑或实体销毁不触发 | `Prune` 无条件在 Commit 最前跑；断言过期项被移除 |
| 史莱姆行为变更被当成回归 | 下游断言期望「打不动」 | 变更写进 CR；受影响断言同批更新 |
| 拆系统后排序丢一次 tick | `ImmunityGrantSystem` 排到 `Expiry` 之前 | 注册键显式 `after`，并用真实启动对照；D-2 的边界断言兜底 |
| 预检被当成保证 | 后人删掉 admission 里的裁决 | D-5 写明「仅预检算缺陷」；§8.3 要求 `CheckAdmission` 的 diff 为空 |

## 14. 设计取舍记录

| 决策 | 采用 | 替代方案 | 理由 |
| --- | --- | --- | --- |
| 通道索引 | `General=0` 同列 | 沿用 `-1` 哨兵 | 消除「先分支再索引」的不变量 |
| 时间表示 | 到期戳 `untilTick` | 每 tick 递减 | 写放大 `O(授予次数)` 而非 `O(激活数×tick)` |
| 激活集合 | archetype 查询 | 注册/激活双列表 | 双列表是影子状态，且 Arch 已提供该索引 |
| 时长 | 策略表 + 实际伤害反推 | 固定宿主常量 | 固定值让通道与阶梯都没有意义 |
| 长无敌 | 参数保留、恒 false | 现在就建饰品来源 | 不造没有生产者的输入 |
| NPC 分槽键 | `EntityRuntimeHandle` | 玩家下标 | ECS 无稳定下标；句柄带 generation |
| NPC 是否分通道 | 不分 | 与玩家统一 | 原版就不分；纠正它要回答原版没回答的问题 |
| 拦截点 | 两层同规则，预检只做优化 | 仅 admission / 仅 pre-hit | S1 禁止动 `CheckAdmission`；S2 要求预检存在 |
| 开窗位置 | 独立 `ImmunityGrantSystem` | 内联在协调器 | 协调器不该持有窗口策略；拆开才叫独立系统 |

## 15. 断言覆盖与收口

### 15.1 实测断言清单

`Test/Terraria.Components/Terraria.Combat.ProtectionWindow` 收口时实测 7 条，全部 PASS；
2026-11-25 收口后复核追加第 8 条（DoT 豁免），当前实测 8 条全部 PASS：

| # | 断言 | 钉住的东西 |
| --- | --- | --- |
| 1 | `hit-applies-and-opens-window` | 命中落地、生命下降、授予 tick 本身已在窗口内、时长为非零 |
| 2 | `window-boundary-is-exactly-duration-ticks` | 两端边界：`N+D-1` 仍被挡、`N+D` 放行（D-2 的边界惯例） |
| 3 | `never-granted-window-is-not-active-at-tick-zero` | 0 号哨兵不被误判激活（严格小于而非小于等于） |
| 4 | `channel-isolation-lava-vs-general` | 同一目标同一 tick：General 挡、Lava 放行、General 再挡 |
| 5 | `duration-ladder-chip-damage-is-shorter` | 实际伤害 1 → `ChipDamage` 档，且严格短于普通档 |
| 6 | `npc-attacker-scoped-immunity` | 攻击者 A 的窗口挡 A 不挡 B |
| 7 | `expiry-system-clears-stale-ticks` | 归零系统真在清过期戳，不是空转 |
| 8 | `damage-over-time-is-not-blocked-by-hit-window` | 受击窗口挡住一次打击后，同 tick 的灼烧跳数仍然落地（收口复核补） |

### 15.2 与 §1 五条口径的对照

| §1 口径 | 覆盖情况 | 依据 |
| --- | --- | --- |
| 1 通道隔离 | 已覆盖 | 断言 4 |
| 2 时长由实际伤害反推 | **部分覆盖** | 断言 5 只钉住「1 点比普通档短」；PvP 8 tick、长无敌变体未断言 |
| 3 边界精确 | 已覆盖（两端都钉） | 断言 1、2、3 |
| 4 NPC 按攻击者分槽 | 已覆盖 | 断言 6 |
| 附加 · DoT 不被受击窗口吞 | 已覆盖（收口后复核补） | 断言 8 |
| 5 有证据 | 已覆盖 | 本节与 §10 的证据表 |

口径 2 的残缺是**有意留下的**：用户明确要求测试只铺 10%，PvP 与长无敌两条分支在
`ImmunityDurationPolicy` 里已实现但无断言。它记在 §12 未闭合项里，不当作已完成。

### 15.3 为什么只有 7 条

§5.2 原本排了 5 条红测、§9.1 的 P4-1 要求扩到五条口径全覆盖。实际只铺骨架 7 条，
理由记录在 §4.1：用户要求「测试只写 10%」。**代价必须明写**——红测没有逐条跑过变红，
所以「断言是否真能钉住缺口」只由收口时的一次全绿证明，不由先红后绿证明。
后续补断言时，应把每条断言先在旧实现上确认变红，再接受它转绿。

### 15.4 收口结论

- P0–P4 的代码、注册键、协作规则、四份 README 与验证断言均已落地并通过 §10 的全组验证；
- §12 六项未闭合**全部仍在**，本计划不因批次完成而关闭它们；
- 本计划状态改为「已实施」，后续改动走新计划，不在本文件追加批次。
