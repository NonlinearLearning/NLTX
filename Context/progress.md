# 当前上下文

> 只回答“现在要知道什么”：基线、未闭合、技术债、轮次索引。细节在 CR / 计划 / 审计里。

## 长期未闭合

- （11-17 起）原版客户端不识别服务器权威扩展位，画面验收未完成 → [CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md)
- （11-07 起）聊天系统真实客户端“输入 → 全服显示”未联调 → [审计](../docs/reviews/audits/2026-11-07-chat-system-introduction.md)
- （11-06 起）服务器运算系统未覆盖传送塔、展示人偶/帽架/锚、蓝量消耗、全物品表、群系刷怪分区 → [审计](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)
- （11-22 起）`Build/bin/` 仍保留扁平化前的旧项目名产物目录，属历史构建残留。
- （11-17 起）史莱姆雨未接入运行时 → [CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md)
- （11-20 起）ECS 架构整改总计划仍 active，WorldRuntime 与项目配置收窄未纳入 → [计划](../docs/plans/2026-11-08-ecs-architecture-remediation-master-plan.md)
- （11-22 起）`Communication` 迁出四类职责目录待收尾；首个系统出现时需补建 → [CR](../docs/cr/CR-2026-11-22-nsslc-ecs-role-directory-layout.md)
- （11-23 起）LVCS 其余内容族未接 → [设计](../docs/architecture/2026-11-23-lvcs-unified-loading-api-design.md)
- （11-24 起）ECS 协作契约未覆盖全部注册键 → [设计](../docs/architecture/2026-11-24-ecs-coordination-rules-design.md)
- （11-24 起）投射物内容族 P5 按需能力菜单未开工；客户端输入适配缺失 → [CR](../docs/cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md)

## 技术债

- `StunComponent` 无移动消费者；`BuffApplyRequestComponent` 无内容侧生产者；NPC 未装配 Buff 接收；即时效果未建模；旧枚举键零读写。
- Inventory 属性贡献与装备来源身份 deferred → [计划](../docs/plans/2026-11-20-attribute-system-execution-plan.md)
- 物品实体路径已于 11-23 删除；"物品自带子库存"如需恢复须走 P1-B → [计划](../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md)

## 基线（2026-11-22）

- 活动项目 `src/NSSLC/Terraria.Components.csproj`（net10.0）；`Share/` 收八个共享能力域（LVCS 已迁出到基础设施），其余直挂项目根；`World/` 下另有 EntitySpawn、Session、TileEntity 子域。
- 服务器 `src/NSSLC.Server`，监听 `127.0.0.1:7777`；14 个验证项目按 `Test/<生产边界>/<项目>/` 落位 → [规则](../Test/README.md)
- `src/NSSLC.Infrastructure/` 受保护：`Diagnostics/`、`Lvcs/`（内容加载契约）参与当前编译，其余子项目仍未接入。

## 工作区事件

- 2026-11-22 未跟踪测试项目被外部进程清除；Communication.Verification 未恢复。
- 2026-11-24 夜间 `docs/defects/` 被增量并行写入（新增与改名都有），按目录现状判断已记录缺陷，不按旧快照。

## 最近轮次

- **2026-11-25 手持概念并入 Inventory**：Version4 的 `HeldItem => inventory[selectedItem]` 由 `InventoryComponent.Held`（`HeldSlot?`）+ `InventorySystem.TrySetHeldSlot/TryGetHeldStack` 表达；包 13 选中槽改走 `PlayerInputRequestKind.SelectHeldSlot`，物品使用只对当前手持槽生效（`NotHeld`），legacy `Player.HeldItem` 改为视图 → [CR](../docs/cr/CR-2026-11-25-held-item-inventory-merge.md)
  - 未闭合：无独立 `HandsComponent`（物品实体路径已删，重建即违反 INV-1）
- **2026-11-25 LVCS 契约归位**：内容族加载契约从领域共享程序集下沉到基础设施程序集，依赖方向改为单向，领域侧只留加载产物 → [CR](../docs/cr/CR-2026-11-25-lvcs-contract-relocation.md)
- **2026-11-25 无敌窗口通道化（P0–P4）**：单值 60 tick 窗口换成「6 通道到期戳 + 伤害反推阶梯 + NPC 攻击者分槽 + 只读免疫查询」，持续伤害两头都不受窗口影响，预检与裁决的差额进每秒诊断
  - 未闭合：表现层与网络投影未做、longInvince 无来源、出生/复活保护未接线、跨 tick 差额与预检只读无断言 → [CR](../docs/cr/CR-2026-11-25-invincibility-window-p0-p3.md)
- **2026-11-25 错误处理约束成文**：失败先分类再选机制，按层选编码并单向跨层，码 append-only 且内部诊断码与协议码分治，结果未知不按失败重试
  - 未闭合：三项落地整改须由后续轮次按约束 §8 清单逐条勾选通过方算闭合（反例组件改封闭失败原因枚举、单一码注册表建立、技术异常转换点清单补齐） → [CR](../docs/cr/CR-2026-11-25-error-handling-constraint.md)
- **2026-11-25 AI 声明接口收口（P0–P4）**：输入、输出、顺序锚点与提交目标四类声明全部被机器消费，缺声明在建服期失败 → [CR](../docs/cr/CR-2026-11-24-ai-declaration-interface.md)
- **2026-11-25 投射物链路修复计划立项**：四条缺陷与两处接缝不一致排成六批，先红后绿、按批验收
  - 未闭合：五条判据全过前不得标记完成，缺陷文件未删即未闭合 → [计划](../docs/plans/2026-11-25-projectile-chain-repair-execution-plan.md)
- **2026-11-24 约束与代码漂移审计与同步**：10 份约束逐条对码，8 项失效、6 项代码已深化；A/B 类 13 条已改入正文，D 类待定 → [审计](../docs/reviews/audits/2026-11-24-constraint-drift-audit.md)、[CR](../docs/cr/CR-2026-11-24-constraints-drift-sync.md)
- **2026-11-24 玩家侧战斗结果消费者边界判定**：确认为空转的冗余队列清理者，与共享结果生产者的后置条件重叠，建议删除
  - 未闭合：按审计 §7 落地清单执行删除并跑通其验收条件，或确认需要结果级玩家数据后改走补齐 → [审计](../docs/reviews/audits/2026-11-24-player-combat-result-system-boundary.md)

## 历史

已关闭轮次与更早内容见 `docs/cr/`、`docs/reviews/audits/`、`docs/plans/`；旧 progress 全文可用 `git log -p -- Context/progress.md` 取回。
