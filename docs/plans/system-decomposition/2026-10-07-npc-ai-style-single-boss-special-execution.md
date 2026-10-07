# NPC AI Style Single Boss / Special 类别执行文档

状态：类别窄切片已由并行会话完成并验收；完整类别仍为 `partial/open`。

类别：`4, 15, 43, 45, 51, 54, 57, 58, 60, 61, 69, 93, 97, 110, 117, 120, 121, 122, 123`

分支：`codex/npc-ai-style-single-boss-special`

会话：`01a115c8-e60c-7612-9ea2-35b4d18df768`

起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`

## 范围与来源映射

本类别包括 Eye、King Slime、Queen Bee、Golem、Plantera、Brain、季节事件、Duke Fishron、Pirate Ship、Nebula Brain、Betsy、Blood Nautilus、Hallow Boss、Queen Slime、Pirate Ghost 和 Deerclops 等单体/特殊 encounter。

并行会话登记了 20 个精确 `(type, netId, aiStyle)` 元组，并纠正了关键来源：style 97 是 NebulaBrain `type/netId=420`，style 110 是 Betsy `551`；styles 117/120/121/122/123 分别对应 BloodNautilus `618`、HallowBoss `636`、QueenSlime `657`、PirateGhost `662`、Deerclops `668`；style 57 同时保留 MourningWood `325` 和 Everscream `344` 两个 tuple。完整阶段、难度、世界事件、奖励和生成链仍 `open`。

## 窄 authority adapter

新增 `NpcSingleBossSpecialAuthorityAdapter.cs`：

- 精确登记本类别 source identity；
- 拒绝 type/netId/aiStyle 任一字段错配；
- `netMode=1` 返回 client observation-only；
- `netMode=0/2` 才允许类别 authority AI adapter 继续；
- 不消费随机、不生成投射物/Servant、不提交 reward/transform/network side effect；
- 未修改公共 `RuntimeNpcStore`、`NpcAiSystem`、TargetHost `Program.cs`、项目文件或中央 ledger。

## 生命周期、API 与更新顺序

每个 boss/special NPC 的阶段计数、目标快照、随机流、变换、生成和奖励必须由实例 owner、encounter/world owner、network owner 和 lifecycle owner 分开持有。adapter 只做 authority identity/permission composition；真正来源 profile 仍需在权威入口按“读取世界事实 → 目标/阶段 → 随机 → profile decision → authority effects → 同步”的顺序接线。

client 只观察复制阶段/同步意图/呈现数据，不构造 `NpcAiInput`，不选择目标、不消费 random、不推进 dash/phase/task，不预测生成或奖励。

## 共享宿主审计与主会话 gate

旧路径 `RuntimeNpcStore.Update → UpdateMovement → UpdateEyeOfCthulhuMovement → NpcEyeOfCthulhuProfile.EvaluateWithRandom` 可能在 client mode 运行 profile；原有 effect port 只阻止 Servant allocation。该类别没有复制公共 gate。

主会话已把 `NpcAiAuthorityGate` 放在公共 NPC tick 入口、自然生成和权威 spawn API 之前，并在 client 分支只保留 observation trace。TargetHost 断言保留：client 不生成 Servant，server 可以生成并记录 Servant。

## 验证记录

- 20 个 identity、client observation-only、server allowlist 和 no-global-random 源级检查：`exit 0`；
- TargetHost authority strings 保留；类别会话未改共享 verifier；
- 本类别未 build/restore/run；主会话唯一 `Terraria.NpcAi.TargetHostVerification` 增量 build：`exit 0`，无 error。

## 缺口与精确回退

- 完整 19 styles 的阶段表、世界/难度条件、攻击顺序、生成、变换、奖励、网络复制、保存/卸载和来源 differential 未关闭；
- adapter 未接入每个真实 profile，不能把类别登记为 `implemented/verified`；
- 精确回退：删除本类别 adapter 与本 Markdown，保留公共 gate、TargetHost 断言和其他类别工作。
