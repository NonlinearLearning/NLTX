# NPC AI Style Town / Rescue 类别执行文档

状态：类别窄切片已由并行会话完成并验收；完整类别仍为 `partial/open`。

类别：`0, 7, 42, 124, 125, 127`

分支：`codex/npc-ai-style-town-rescue`

会话：`01a115c8-cbab-7bf2-8d0c-6c1a7e011cc1`

起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`

## 执行范围与硬约束

本类别覆盖城镇、救援、返回、日夜状态、变换和关系/住房边界。客户端不得运行 NPC AI，不得推进救援任务、变换、关系、目标、物理或 authority effects；客户端只能消费服务端复制状态、同步意图和呈现数据。

并行会话没有修改公共 `RuntimeNpcStore`、`NpcAiSystem`、Simulation `Program.cs`、项目文件或中央 coverage ledger。共享宿主 gate 由主会话统一接入。

## 来源与身份映射

| style | 来源/身份状态 | 本轮状态 |
| ---: | --- | --- |
| 0 | Guide/Old Man 等有限 catalog 与通用 town fallback 的差异 | `mapped/open`，不可把 catalog style 当作完整来源证明 |
| 7 | Lost Girl/救援相关分支 | `mapped/partial` |
| 42 | Lost Girl `type=195/netId=195/aiStyle=42` | `mapped/partial implementation` |
| 124 | Mechanic 等变体，不能与 `type=124` 机械编号混同 | `indexed/mapped/open` |
| 125 | town/rescue 关联特殊单位 | `indexed/open` |
| 127 | Guide/Old Man/有限 town 状态差异 | `mapped/open` |

文档保留了 type、netId、aiStyle 的独立登记，并特别记录 `type=124 Mechanic` 与 `aiStyle=124` 不能互相推断。

## 最小真实切片：Lost Girl Rescue

新增的窄 profile 精确匹配 `type=195/netId=195/aiStyle=42`：

- `NpcLostGirlRescueProfileInput` 接受当前 AI 槽、tick、模式和来源条件；
- `NpcLostGirlRescueProfile` 在权威端按来源条件推进 `ai[0]`，达到第 21 tick 返回 `Transform(196)` 意图；
- `NpcLostGirlRescueProfileResult` 只描述状态转移/变换意图，不执行变换；
- `NpcLostGirlRescueProfileVerification` 只验证类别契约，未替代宿主 authority verifier；
- `NpcAiAuthorityMode` 明确区分 single-player、client、server。

client 输入在决策前返回原状态，不请求变换、不消费随机、不提交权威效果。

## Owner、生命周期与更新顺序

NPC 实例拥有 rescue counter/行为槽；变换由 NPC lifecycle/identity owner 提交，profile 只返回决定。更新顺序必须是：读取复制/世界事实 → authority gate → 读取实例槽 → 计算救援阶段 → 返回 transform intent → 权威 owner 校验 identity 并提交。客户端不推进 cursor、不释放任务、不修改住房/关系。

Guide/Old Man 的对话、住房和返家任务仍由各自 task/relationship owner 管理；本轮没有用一个 rescue profile 覆盖其他 town NPC。

## 共享宿主审计与集成

会话确认旧 `RuntimeNpcStore.Update` 在 client mode 仍可能推进 damage tracking、自然 despawn、Guide task、移动/碰撞和 `_aiSystem.Evaluate`。Eye 的旧 gate 只阻止 Servant 分配，不能证明客户端 AI 关闭。

主会话已接入统一 `NpcAiAuthorityGate`，位于自然生成、伤害 tracking、自然 despawn、AI、物理和接触伤害之前；client 仅观察 active replicated state，并保留防御性 trace，不构造 AI 输入。

## 验证记录

- Lost Girl identity、client early return、21 tick threshold、transform target、类别 verifier 和文档唯一性源级检查：`exit 0`；
- `git diff --check`：`exit 0`；
- TargetHost `Program.cs` 共享文件未被类别会话修改，client/server authority strings 仍存在；
- 本类别未运行 build/restore/test；主会话只执行指定 TargetHost `--no-restore` 增量 build，结果 `exit 0`，无 error。

## 缺口与精确回退

- styles 0/7/124/125/127 的完整变体、对话、住房、日夜、关系、任务终止和网络复制未关闭；
- Lost Girl profile 尚未接入生产 `RuntimeNpcStore`，因此不宣称完整 style 42 runtime verified；
- 精确回退：删除 `NpcAiAuthorityMode.cs`、`NpcLostGirlRescueProfile*`、类别 verifier 和本 Markdown；保留主会话公共 gate、TargetHost 断言和用户既有修改。
