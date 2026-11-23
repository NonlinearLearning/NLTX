# 当前上下文

> 只回答“现在要知道什么”：基线、未闭合、技术债、轮次索引。细节在 CR / 计划 / 审计里。

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
- 物品实体路径已于 11-23 删除；"物品自带子库存"如需恢复须走 P1-B → [计划](../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md)

## 基线（2026-11-22）

- 活动项目 `src/NSSLC/Terraria.Components.csproj`（net10.0）；`src/NSSLC/` 分两层：`Share/` 收七个共享能力域，其余直挂项目根；`World/` 下另有 EntitySpawn、Session、TileEntity 三个语义子域。
- 服务器 `src/NSSLC.Server`，监听 `127.0.0.1:7777`；14 个验证项目按 `Test/<生产边界>/<项目>/` 落位 → [规则](../Test/README.md)
- `src/NSSLC.Infrastructure/` 受保护：仅 `Diagnostics/` 参与当前编译。

## 工作区事件

- 2026-11-22 未跟踪测试项目被外部进程清除；Communication.Verification 未恢复。

## 最近轮次

- **2026-11-23 LVCS 统一加载系统落地**：契约、分派层、ItemUse 族落地，49 项验证与真实启动对照通过
  - 未闭合：其余族待接 → [设计](../docs/architecture/2026-11-23-lvcs-unified-loading-api-design.md)
- **2026-11-23 投射物穿透预算联动设计**：属性给上限、弹体持剩余，命中与反弹共用消费入口
  - 未闭合：上限快照、反弹顺序待决 → [设计](../docs/architecture/2026-11-23-projectile-penetration-budget-linkage-design.md)
- **2026-11-23 物品双宿主边界落地**：载荷层与两宿主 owner 收敛为不变量，实体路径已删，世界侧补拾取延迟与存活计数 → [计划](../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md)
- **2026-11-23 刷怪、会话与 Tile 实体收进 World 语义子域**：三域在 `World/` 下保留角色目录，API 不变 → [CR](../docs/cr/CR-2026-11-23-world-semantic-subdomains.md)
- **2026-11-23 Item 与 WorldItem 拆分判定**：载荷/世界宿主/容器宿主三分解成立，单一系统与双份身份均被否决 → [审计](../docs/reviews/audits/2026-11-23-item-worlditem-split-necessity.md)
- **2026-11-22 NSSLC 领域内按 ECS 职责分目录**：组件、命令、查询与系统附属文件分别归入四类目录，主系统文件留领域根
  - 未闭合：`Communication` 迁出由并行任务收尾；扁平目录出现首个系统时需补建四类目录 → [CR](../docs/cr/CR-2026-11-22-nsslc-ecs-role-directory-layout.md)

## 更早轮次

- 11-23 刷怪域改名 EntitySpawn（目录、类型与命名空间统一） → [CR](../docs/cr/CR-2026-11-23-entity-spawn.md)
- 11-23 Share 收容层恢复（七域入 `Share/`，API 不变，14 项目通过） → [CR](../docs/cr/CR-2026-11-23-nsslc-share-layer-restore.md)
- 11-22 出站 tick 批仲裁（成批发布，闭合后恢复） → [计划](../docs/plans/2026-11-22-outbound-tick-batch-execution-plan.md)
- 11-22 领域目录扁平化（18 领域、592 文件迁移） → [CR](../docs/cr/CR-2026-11-22-nsslc-domain-directory-flatten.md)
- 11-22 服务器诊断日志迁出共享组件域 → [CR](../docs/cr/CR-2026-11-22-diagnostics-to-infrastructure.md)
- 11-22 Test 按生产边界重组（两层布局、去统一后缀） → [规则](../Test/README.md)
- 11-22 NPC 同步出站优先级隔离 → [CR](../docs/cr/CR-2026-11-22-npc-sync-outbound-priority.md)
- 11-22 项目文件组织从 ECS 约束拆出，仓库与项目层落位规则独立成文 → [CR](../docs/cr/CR-2026-11-22-project-file-organization-split.md)
- 11-22 属性与状态效果边界收敛 B0–B9（Inventory 属性桥接 deferred） → [计划](../docs/plans/2026-11-20-attribute-system-execution-plan.md)
- 11-20 ECS Commit / Projection 边界收敛 → [计划](../docs/plans/2026-11-08-ecs-architecture-remediation-master-plan.md)
- 11-17 手写史莱姆 AI 主动状态 → [CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md)
- 11-09 ECS command buffer deferred 实体生命周期 → [计划](../docs/plans/2026-11-08-ecs-command-buffer-commit-plan.md)
- 11-07 聊天系统（NetModules 82 + NetTextModule） → [审计](../docs/reviews/audits/2026-11-07-chat-system-introduction.md)
- 11-07 WorldItem / TileEntity / NpcSpawn / ItemUse 四域独立 → [审计](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)
- 11-06 服务器运算系统引入 → [审计](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)
- 10-13 L2 组件恢复与 components-only 清理基线 → [清理记录](../docs/cr/CR-2026-10-09-components-only-cleanup.md)、[组件清单](../docs/reviews/audits/2026-10-09-components-only-inventory.md)

## 历史

更早轮次与已关闭条目见 `docs/cr/`、`docs/reviews/audits/`、`docs/plans/`；旧 progress 全文可用 `git log -p -- Context/progress.md` 取回。
