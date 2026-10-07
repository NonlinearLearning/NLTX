# NPC AI Style Bio / Environment 类别执行文档

状态：类别窄切片已由并行会话完成并验收；完整类别仍为 `partial/open`。

类别：`64, 65, 68, 112, 114, 115, 116`

分支：`codex/npc-ai-style-bio-environment`

会话：`01a115c8-d703-7bb2-a6d6-2fc42580d5ec`

起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`

## 范围与身份

本类别覆盖昆虫、鸟类/水鸟、环境生物、水面/湿态、日夜和环境随机。reference index 已登记：64 Firefly/LightningBug、65 Butterflies、68 Duck/Seagull、112 FairyCritter、114 Dragonflies、115 LadyBugs、116 WaterStriders。除 WaterStrider 窄切片外，其余保持 `mapped/open` 或 `indexed/open`，不由相邻 type 或 netId 推断 profile。

全局约束：client 不执行 AI、随机、目标/物理、生成、变换、奖励、任务或关系效果，只消费复制和呈现数据。

## 最小真实切片：WaterStrider

新增 `NpcBioEnvironmentProfile.cs`，选择 style 116 WaterStrider，并精确支持：

- `type=612/netId=612/aiStyle=116`；
- `type=613/netId=613/aiStyle=116`。

来源顺序保留为：读取水线、湿态、位置、底部高度和当前速度；水线下上浮并限制 `-4` 垂直速度；水面限制底部并标记接触；无水线但 wet 时应用 `-0.2f` 垂直调整；保留 `ai[0] != 0` 早退；递增 `ai[1]`；应用水平阻尼；达到阈值后通过显式随机 port 选择方向并返回跳跃速度/网络更新意图。

profile 只返回值，不直接写状态。`netMode=1` 在随机端口检查和所有随机消费前返回 observation-only 结果，不推进 AI、不消费随机、不请求 authority update。

## Owner 与更新顺序

WaterStrider 的 ai 槽由 NPC 实例 owner 持有；随机由显式 port 注入；movement、碰撞、网络更新由权威宿主提交。客户端不得补算水线行为、跳跃或随机方向。其余环境生物的日夜/天气/变体条件仍需逐来源核验。

## 共享宿主审计与主会话集成

子会话确认旧入口会在 `RuntimeNpcStore` 内直接进入 `_aiSystem.Evaluate`，Eye 的旧 effect guard 只阻止 Servant allocation。类别会话没有重复修改公共入口。

主会话已将 `NpcAiAuthorityGate` 接在 NPC tick、自然生成、生成 API 和 contact-damage phase 的 authority 边界。client `RuntimeNpcStore.Update` 只产生既有 authority observation trace，不进入 profile/目标/物理路径。

## 验证记录

- WaterStrider identity、来源公式顺序、client early return、随机 port 和结果 owner 的源级检查：`exit 0`；
- 文档栏目、authority 断言、类别范围和 whitespace 检查：`exit 0`；
- 本类别未执行 build/restore/run；主会话唯一 TargetHost 增量 build：`exit 0`，0 error，未运行程序。

## 缺口与精确回退

- styles 64/65/68/112/114/115 的完整行为、type 变体、环境事实 owner、复制、保存/卸载和来源 golden 未完成；
- WaterStrider profile 尚未接入生产宿主，不能登记为 `verified`；
- 精确回退：删除 `NpcBioEnvironmentProfile.cs` 与本 Markdown；不删除公共 gate、TargetHost 断言或用户既有修改。
