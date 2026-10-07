# NPC AI Style Air / Water 类别执行文档

状态：类别窄切片已由并行会话完成并验收；完整类别仍为 `partial/open`。

类别：`2, 5, 9, 14, 16, 17, 18, 21, 24, 44, 49, 63, 70, 72, 74, 80, 85, 86, 91, 95, 96, 99, 108, 113, 118, 119`

分支：`codex/npc-ai-style-air-water`

会话：`01a115c8-cb96-7912-8069-8bbfc811b736`

起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`

## 执行边界

本类别负责飞行、水中、潜冲、湿态、`oldVelocity`、`collideX/Y`、反弹、重力门控、视线和退出行为的来源映射与窄切片迁移。`type`、`netId`、`aiStyle` 和变体必须分别登记；无来源证据的条目保持 `indexed/mapped/open`，不得用 fallback 冒充等价实现。

全局 authority 约束：AI 决策、目标选择、随机消费、权威物理/碰撞、生成、变换、奖励、任务推进、关系修改和 authority effects 只能在单机权威端或服务端执行。`netMode=1` 只能观察复制状态、网络同步意图和呈现数据。

## 来源与身份映射

已核对的代表性来源入口包括：

- style 2 `AI_002_FloatingEye`：DemonEye 等飞行单位；当前有限 catalog 对 DemonEye 有局部 profile 事实。
- style 5 `AI_005_EaterOfSouls`：Servant of Cthulhu、Eater of Souls、Meteor Head；本轮精确切片为 Eater of Souls `type=6/netId=6/aiStyle=5`。
- styles 9、14、16、17、18、21、24、44、49、63、70、72、74、80、85、86、91、95、96、99、108、113、118、119：保留 reference index 的来源锚点和候选 type，不在没有 `SetDefaults`/profile 接线证据时推断完整映射。

已证明的 profile 身份：

| 切片 | identity | 状态 |
| --- | --- | --- |
| Eater collision | `type=6, netId=6, aiStyle=5` | `mapped/partial` |
| Floating Eye / Servant | 既有有限 profile 的局部 identity | `mapped/partial`，不等于完整 style coverage |
| 其余本类别 styles | 逐项 reference-index 候选 | `indexed/open` 或 `mapped/open` |

## 最小真实切片

并行会话实现了 Eater of Souls 的碰撞 profile：

- `NpcEaterOfSoulsCollisionInput` 捕获旧速度、水平/垂直碰撞、湿态和当前速度等不可变事实；
- `NpcEaterOfSoulsCollisionProfile` 精确校验 `type/netId/aiStyle`，按来源的 `0.4` 反弹系数输出速度和同步意图；
- `NpcEaterOfSoulsCollisionResult` 只返回结果，不直接写入实体、网络或随机源；
- `NpcEaterOfSoulsCollisionProfileVerification` 是类别专属源级 verifier，未接入共享 TargetHost。

`netMode=1` 在碰撞计算前拒绝；profile 不消费随机，不创建实体，不提交权威效果。真实宿主接线留待后续 profile integration 批次。

## 状态 owner、生命周期和更新顺序

状态 owner 仍是每个 NPC 实例的行为/本地 AI 槽和 movement/collision 组件；profile 输入是当前 tick 的只读快照，profile 结果由权威宿主按固定顺序提交。目标重选、旧速度捕获、湿态读取、碰撞结果、反弹、网络同步意图和 presentation 必须保持来源顺序。

本轮没有把随机、物理写入、生成、任务推进或复制提交放入客户端 profile。客户端 profile 仅允许作为复制/呈现契约的观察结果，不允许通过 client tick 补算 AI。

## 共享宿主审计与主会话集成

审计确认旧路径 `RuntimeNpcStore.Update → UpdateMovement → profile/_aiSystem.Evaluate` 原先可在 `netMode=1` 进入，因此类别局部 guard 不能代替共享入口 gate。并行会话没有修改 `RuntimeNpcStore.cs` 或 `NpcAiSystem.cs`，避免 7 个类别竞争写入公共入口。

主会话已在共享边界接入 `NpcAiAuthorityGate`：

- `RuntimeNpcStore.Update` 在伤害 tracking、自然 despawn、目标、profile、物理和碰撞之前 fail-closed；
- `ActiveNpcTickPhase` 在自然生成和 NPC 接触伤害之前 fail-closed；
- `RuntimeNpcNaturalSpawnPass` 和 `RuntimeNpcStore.TrySpawn` 拒绝 client/未知模式的权威生成；
- client 只保留观察性 authority trace，不构造 `NpcAiInput`、不选目标、不消费随机、不推进 movement/AI 状态；
- effect port 的 Servant guard 保留为防御性第二道边界。

## 验证记录

并行类别会话遵守验证边界：

- 类别文档、profile identity、authority guard、来源公式和 whitespace 源级检查：`exit 0`；
- 类别 verifier 没有可用的现成 DLL；一次受影响项目 focused build 因缺少 `project.assets.json` 失败，随后按主会话边界不 restore、不重试、不运行；
- TargetHost authority 源码断言由主会话保留并增强；
- 主会话唯一增量构建：

  ```text
  dotnet build Test/Terraria.NpcAi.TargetHostVerification/Terraria.NpcAi.TargetHostVerification.csproj --no-restore --nologo -v:minimal
  ```

  结果：`exit 0`，无 error；未运行 solution/full regression/full test sweep 或 TargetHost 程序。

## 缺口与精确回退

- 本类别除 Eater 窄 profile 外仍是 `indexed/mapped/open`，完整飞行、水中、变体、世界条件、helper 闭包、随机流、网络复制、保存/卸载、来源 golden 和 runtime coverage 未关闭；
- Eater profile 尚未接入真实 `RuntimeNpcStore`；
- client observe/replication/presentation owner 仍需后续网络批次完成；
- 精确回退：删除本类别新增的 `NpcEaterOfSoulsCollisionProfile*` 文件及本 Markdown；不回退公共 gate、TargetHost authority 断言或用户已有修改。

本文件是主树交付记录；类别专属窄代码的原始分支由统一 `codex/npc-ai-style-` 前缀管理。
