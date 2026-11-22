# 当前上下文

> 只回答“现在要知道什么”：基线、未闭合、技术债、轮次索引。细节在 CR / 计划 / 审计里，此处不复述。

## 长期未闭合

- （11-17 起）原版客户端不识别服务器权威扩展位，画面验收未完成 → [CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md)
- （11-07 起）聊天系统真实客户端“输入 → 全服显示”未联调 → [审计](../docs/reviews/audits/2026-11-07-chat-system-introduction.md)
- （11-06 起）服务器运算系统未覆盖传送塔、展示人偶/帽架/锚、蓝量消耗、全物品表、群系刷怪分区 → [审计](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)
- （11-22 起）`Build/bin/` 仍保留扁平化前的旧项目名产物目录，属历史构建残留。
- （11-17 起）史莱姆雨未接入运行时 → [CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md)
- （11-20 起）ECS 架构整改总计划仍 active，WorldRuntime 与项目配置收窄未纳入 → [计划](../docs/plans/2026-11-08-ecs-architecture-remediation-master-plan.md)

## 技术债

- `StunComponent` 无移动消费者；`BuffApplyRequestComponent` 无内容侧生产者；NPC 未装配 Buff 接收；即时效果未建模；旧枚举键零读写。
- Inventory 属性贡献与装备来源身份 deferred → [计划](../docs/plans/2026-11-20-attribute-system-execution-plan.md)

## 基线（2026-11-22）

- 活动项目 `src/NSSLC/Terraria.Components.csproj`（net10.0）；`src/NSSLC/` 分两层：`Share/` 收 AI、Attributes、Combat、Entity、Inventory、Item、Physics 七个共享能力域，其余领域直挂项目根，无 `Components/` 中间层。
- 服务器 `src/NSSLC.Server`，`--manual-ai` 监听 `127.0.0.1:7777`；13 个验证项目按 `Test/<生产边界>/<项目>/` 落位 → [规则](../Test/README.md)
- `src/NSSLC.Infrastructure/` 整体受保护：仅 `Diagnostics/` 参与当前编译，其余旧项目不随组件库编译。

## 工作区事件

- 2026-11-22 20:35–20:37 未跟踪测试项目被外部进程清除；Attributes、EcsScheduling 已重建并入索引，Communication.Verification 未恢复。

## 最近轮次

- **2026-11-23 恢复 NSSLC 的 Share 收容层**：按 git 历史把七个共享能力域收进 `src/NSSLC/Share/`，命名空间与公共 API 不变，14 个验证项目全通过 → [CR](../docs/cr/CR-2026-11-23-nsslc-share-layer-restore.md)
- **2026-11-22 服务器诊断日志迁出共享组件域**：日志端口与控制台适配器移入基础设施侧独立工程，命名空间、公共 API 与输出格式不变 → [CR](../docs/cr/CR-2026-11-22-diagnostics-to-infrastructure.md)
- **2026-11-22 NSSLC 领域内按 ECS 职责分目录**：移除 `Model` 式收容目录，19 个领域（含语义子域）的组件、命令、查询与系统附属文件分别归入 Component/Command/Query/System，主系统文件留在领域根
  - 未闭合：`Communication` 迁出由并行任务收尾；扁平目录出现首个系统时需补建四类目录 → [CR](../docs/cr/CR-2026-11-22-nsslc-ecs-role-directory-layout.md)
- **2026-11-22 出站 tick 批仲裁**：每 tick 必须同步的权威快照成批发布，批内不再被普通流量切断，闭合后自动恢复
  - 未闭合：重负载世界整 tick 超时跳格待优化（液体降频或投影切片）；待最新客户端画面验收 → [计划](../docs/plans/2026-11-22-outbound-tick-batch-execution-plan.md)
- **2026-11-22 NSSLC 领域目录扁平化**：删除 `Components/`、`Share/` 两层，18 领域直挂 `src/NSSLC/`，移动 592 文件、`.cs` 零改动
  - 未闭合：服务器仍持有旧程序集，需重启复验 → [CR](../docs/cr/CR-2026-11-22-nsslc-domain-directory-flatten.md)
- **2026-11-22 调试工具客户端操作手册**：新增工具链总入口（启动参数、HeadlessPacketProxy、`-networktest` 契约、RawNpcMonitor、npcprobe）
  - 未闭合：客户端重建 193 错误与 `Lighting.Clear()` 夹具限制 → [手册](调试工具/调试工具客户端操作手册.md)
- **2026-11-22 项目文件组织从 ECS 约束拆出**：仓库与项目层落位规则独立成文 → [CR](../docs/cr/CR-2026-11-22-project-file-organization-split.md)
- **2026-11-22 Test 按生产边界重组**：13 个验证项目改两层布局、去统一后缀，13/13 构建并运行通过
  - 未闭合：`Build/bin/` 仍有旧项目名产物目录 → [规则](../Test/README.md)

## 更早轮次

- 11-22 NPC 同步出站优先级隔离（realtime / normal 分道，配额与 Tile 限流为保守默认值） → [CR](../docs/cr/CR-2026-11-22-npc-sync-outbound-priority.md)
- 11-22 属性与状态效果边界收敛 B0–B9（Inventory 属性桥接 deferred） → [计划](../docs/plans/2026-11-20-attribute-system-execution-plan.md)
- 11-20 ECS Commit / Projection 边界收敛（NPC 身份 bind 与会话走 Commit 阶段 CommandBuffer） → [计划](../docs/plans/2026-11-08-ecs-architecture-remediation-master-plan.md)
- 11-17 手写史莱姆 AI 主动状态（昼夜、当前/最大生命、相对地表位置） → [CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md)
- 11-09 ECS command buffer deferred 实体生命周期 → [计划](../docs/plans/2026-11-08-ecs-command-buffer-commit-plan.md)
- 11-07 聊天系统（NetModules 82 + NetTextModule） → [审计](../docs/reviews/audits/2026-11-07-chat-system-introduction.md)
- 11-07 WorldItem / TileEntity / NpcSpawn / ItemUse 四域独立 → [审计](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)
- 11-06 服务器运算系统引入（刷怪 / 世界物品 / Tile 实体 / 物品使用） → [审计](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)
- 10-13 L2 组件恢复与 components-only 清理基线 → [清理记录](../docs/cr/CR-2026-10-09-components-only-cleanup.md)、[组件清单](../docs/reviews/audits/2026-10-09-components-only-inventory.md)

## 历史

更早轮次与已关闭条目见 `docs/cr/`、`docs/reviews/audits/`、`docs/plans/`；旧 progress 全文可用 `git log -p -- Context/progress.md` 取回。
