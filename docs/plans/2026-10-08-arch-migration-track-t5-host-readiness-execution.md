# Arch 迁移 T5-B5：宿主接线准备、A0 基线冻结与退出审计合同

文档 ID：DOC-2026-10-08-ARCH-TRACK-T5-B5  
状态：active；只推进宿主准备和基线证据，不提前切换默认路径或删除旧框架  
主线：T5 宿主、性能与退出；对应 A0、A6–A8  
前置：T2-B2/T4-B4 的生产签名与 System 生命周期合同；缺失时相关部分必须 blocked

## 1. 目标

将 T5 从“custom ECS 一次 tick smoke”推进为可重复的宿主验收入口：固定 A0 输入、seed、容量、
world file hash、tick/players/选项和输出 schema；盘点 Simulation/NetworkServer 的 World owner、
发布/失败/取消/Dispose 边界；准备 Arch.System 生命周期接线和 A8 残留审计。不能把 custom ECS
smoke、旧 DLL 或构建成功冒充 Arch host 证据。

## 2. 只读宿主/基线工作

1. 从当前源码索引实际定位 Simulation、NetworkServer、WorldLoadCoordinator、save/load、cancel、
   late-finalize 和 zero-tick 入口；每条入口记录 source/target、owner、状态读写、外部 effect、
   stop/rollback 语义。
2. 建立 `Build/diagnostics/ArchMigration/A0/` 输入 manifest：small/medium world、固定 seed、
   Player/NPC/Projectile 代表容量、原始 hash、生成方式、只读约束；没有真实输入就记录 missing，
   不伪造 hash。
3. 保留一条 custom ECS 基线 smoke 作为旧基线，并与未来 Arch host 输出分目录、分状态、分 hash；
   不重新扩大测试矩阵。
4. 读取 T4-B4 System lifecycle handoff 后，标出 host 需要显式调用的 Initialize/BeforeUpdate/
   Update/AfterUpdate/Dispose 和中途 session-invalid barrier；不先把它实现成自制 scheduler。

## 3. 约 10% 宿主准备验证

只选择一条代表路径：兼容输入 → load → publish → 1 tick → exit，并记录 world input/output hash、
session published、最终 tick、cancel/recovery、source unchanged。若 Arch host 尚未接线，结果为
`custom-baseline-only`，不是迁移通过。不要运行性能 benchmark、两次切换、完整 save/load、全容量
矩阵或 A8 删除。

## 4. A8 退出审计

生成或刷新旧框架候选清单，排除 `分类参考`、历史文档和生成输出；分别统计声明、项目引用、生产
构造、测试夹具和默认装配。旧类型的零命中不等于行为退出，必须结合 compile inclusion 和 T2–T4
调用闭包。任何新命名的 `ComponentStore`、`RuntimeEntityFacade`、通用 Query/Edit/Snapshot、
System/Group、CommandBuffer 或关系 graph 也要纳入审计。

## 5. 交接

证据目录：`Build/diagnostics/ArchMigration/T5-B5/`。生成 `handoff.md`，明确
`custom-baseline-only`、`arch-host-not-run`、`performance-not-run`、`delete-forbidden` 等状态，
记录命令、hash 和下一 owner。只有 T2/T4/T5 真实 Arch 证据闭合后才允许另开 A8 删除提交。

