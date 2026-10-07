# 162 个网络消息分类并行执行入口

文档状态：active

本目录是 162 个 Terraria MessageID 按大类并行执行的任务入口。有效消息编号为
1..161，MessageID.Count 为 162。格式目录完整不等于业务 handler、方向准入、阶段
准入和领域 owner 已全部完成；每个类别必须以代码、测试和证据完成闭环。

## 并行调度约束

本轮共 10 个独立类别、10 个独立会话、10 个独立 Git 分支，同时派出，不等待其他
类别完成后再启动。统一分支前缀为 codex/network-message-。

| 类别 | 主题 | 分支 | 执行文档 |
| ---: | --- | --- | --- |
| 01 | 会话与连接生命周期 | codex/network-message-01-session | 01-session-lifecycle.md |
| 02 | 初始世界装载与区域流送 | codex/network-message-02-world-stream | 02-world-streaming.md |
| 03 | 玩家身份、输入和角色状态 | codex/network-message-03-player-state | 03-player-state.md |
| 04 | 动态实体快照与生命周期 | codex/network-message-04-entity-lifecycle | 04-entity-lifecycle.md |
| 05 | 世界修改、Tile、连线与 Tile Entity | codex/network-message-05-world-modification | 05-world-modification.md |
| 06 | 容器、库存、装备、Buff 与经济 | codex/network-message-06-inventory-commerce | 06-inventory-commerce.md |
| 07 | 战斗、伤害、死亡与实体结果 | codex/network-message-07-combat-results | 07-combat-results.md |
| 08 | NPC、Boss、入侵、任务与世界事件 | codex/network-message-08-npc-world-events | 08-npc-world-events.md |
| 09 | UI、聊天、社交、音效与调试 | codex/network-message-09-ui-social | 09-ui-social.md |
| 10 | NetModules、未知与废弃包 | codex/network-message-10-netmodules-legacy | 10-netmodules-legacy.md |

## 所有子代理的共同执行规则

1. 会话启动后先阅读根 AGENTS.md、Context/progress.md、Context/约束/开发协作与变更约束.md
   和 Context/约束/构建与验证约束.md，再阅读本类别文档列出的主题约束。
2. 必须读取 C:\Users\shan\.agents\skills\pua\SKILL.md，并在自己的执行过程中使用其
   主动搜索、验证、同类排查、边界检查和失败换方向规则。不要把 PUA 当成口号。
3. 必须为自己的会话声明一个 /goal，目标只覆盖本类别的实现、10% 核心测试和证据回报。
   目标完成前不能以“代码已写”代替验收；每次失败要记录证据和下一种本质不同的方案。
4. 只改本类别直接负责的源码、测试和证据文件；不得顺手重构网关、生成器或其他类别。
   共享 seam 不足时做最小兼容改动，并在回报中列出共享文件和合并风险。
5. 不修改、清理或重置开始时已有的无关工作树内容，不使用 git reset --hard、git clean、
   checkout -- 或递归删除。
6. 只运行本类别文档列出的约 10% 核心测试。不得为了“顺便确认”运行整个 solution
   或整个 Network Verification Program；构建只构建受影响项目，验证使用 --no-build
   --no-restore。
7. 完成时必须回报：分支、修改文件、实现/未实现消息 ID、测试命令与 exit code、
   warning/error 计数、Build/bin 输出路径、证据文件、已知差异和合并冲突点。

主会话负责验收子代理报告，不以子代理自报通过为最终事实。没有命令输出、源码定位或
可复现证据的“完成”状态只能记为 partial。
