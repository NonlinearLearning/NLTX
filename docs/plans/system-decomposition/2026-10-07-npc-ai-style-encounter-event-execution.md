# NPC AI Style Encounter / Event 类别执行文档

状态：类别窄切片已由并行会话完成并验收；完整类别仍为 `partial/open`。

类别：`38, 62, 73, 83, 87, 92, 94, 104, 105, 106, 109, 111, 126`

分支：`codex/npc-ai-style-encounter-event`

会话：`01a115c8-f655-7060-88a3-8e5ced07f887`

起点快照：`96d893ac09a6aced38df3e3ad0233eb09ed75621`

## 来源与身份映射

已登记的来源代表包括：style 38 Snowman Gangsta/Mister Stabby/Snow Balla；62 ElfCopter；73 MartianTurret；83 CultistTablet/CultistDevote；87 BigMimic；92 TargetDummy；94 LunarTower variants；104 DD2AttackerTest；105 DD2EterniaCrystal；106 DD2LanePortal；109 DD2DarkMage；111 DD2LightningBug；126 StatueMimic。type/netId/aiStyle 逐项记录，未知 profile 保持 `mapped/open` 或 `indexed/open`。

特别边界：MartianDrone 是 style 74，不属于本类别 style 73，未偷加到本类别；TargetDummy 本地有限 catalog 的 aiStyle 与 reference style 92 存在差异，也未绕过差异冒称生产接线。

## 最小真实切片：TargetDummy

新增窄 profile 契约：

- `NpcTargetDummyProfileInput` 捕获 tile 378 有效性、tile entity 存在性、identity 和目标重选后的事实；
- `NpcTargetDummyProfile` 精确匹配 `type=488/netId=488/aiStyle=92`；
- `NpcTargetDummyProfileResult` 返回 `DeactivateNpc` 与 `DeactivateTrainingDummyTileEntity` 意图；
- single-player/server 可评估；client/未知 mode 返回 default；不消费随机、不直接写 NPC/TileEntity、不请求权威效果；
- 实际 NPC/TileEntity 写入由各自 lifecycle/TileEntity owner 完成，需以 identity/anchor 校验避免重复释放。

来源顺序：读取 tile entity/anchor → 目标无效时由 owner `TargetClosest(faceTarget:false)` → profile 计算失效决定 → lifecycle owner 提交 NPC 停用和 tile owner 提交 tile entity deactivation。HitEffect、波次、DD2 crystal/portal、Lunar tower 世界 owner 未在本批补造。

## 共享 client AI 审计与 gate

旧 `RuntimeNpcStore.Update` 无 netMode gate，会继续进入 `UpdateMovement` 和 `_aiSystem.Evaluate`；原 Eye gate 只阻止 Servant allocation。类别会话没有修改共享入口，也没有编写客户端 AI。

主会话已接入统一 `NpcAiAuthorityGate`：client 在 `RuntimeNpcStore.Update`、Active NPC phase、natural spawn 和 `TrySpawn` 处 fail-closed，只观察复制状态/authority trace；服务端仍运行真实 authority tick。类别 profile 仍未接入 production host。

## 验证记录

- TargetDummy identity、client default、modes 0/2 allowlist、invalidation facts、input contract：源级检查 `exit 0`，5/5 pass；
- TargetHost `Program.cs` 共享 client/server authority assertions：保留；类别临时共享修改已撤销；
- 类别未执行 build/restore/run；主会话唯一 TargetHost 增量 build：`exit 0`，无 error。

## 缺口与精确回退

- styles 38/62/73/83/87/94/104/105/106/109/111/126 的世界/波次/保护目标/攻击/同步/奖励算法仍 open；
- TargetDummy local catalog aiStyle=0 与 reference 92 的差异、production adapter、HitEffect/presentation、真实 TileEntity owner 提交未验证；
- 精确回退：删除三个 `NpcTargetDummyProfile*` 文件和本 Markdown；保留主会话 authority gate、TargetHost assertions 和用户既有修改。
