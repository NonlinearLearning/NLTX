# CR-2026-10-08：执行 A8 旧 ECS 清理

状态：partial；A8 零引用清理已执行，生产消费者仍存在的 facade 保留
提出人：用户
渠道：当前会话消息
提出时间：2026-10-08（Asia/Shanghai）
建议分级：major

## 需求原文

> 执行A8 旧 ECS 清理

## 结果

静态引用审计确认 `ComponentStore<TComponent>` 与 `IComponentStore` 没有生产调用者，已从
共享 EntityEcs 项目移除。`EntitySnapshotProjection<TProjection>` 仍由 `EntityRuntime`
使用，已迁移到独立文件以避免与已退出的旧 store 共存。

`EntityRuntime`、`RuntimeEntityHandle`、`EntityComponentSnapshot<T>`、`ComponentAccess` 和旧访问
方法仍被应用、Simulation、网络、存储或兼容尾部调用，因此本批次不删除这些类型。旧的
`IWorldSimulationTickPhase` 已删除，Simulation 阶段已改用 `Arch.System.ISystem<T>`；完整 A8 仍需等
所有 L1 caller 切换并取得适用的 A7 证据。

## 验收口径

本批次遵循编译-only 门禁：不运行测试、verifier、simulation、host smoke、benchmark 或
行为探针；只执行静态审计和受影响生产项目增量构建。运行时行为证据保持 `not-run`。

## 2026-10-09 最终复核

后续 caller 已全部改为直接访问 Arch `World/Entity`；原文中“仍保留的通用组件访问 facade”属于历史状态。当前 `EntityLifecycleRegistry` 仅负责领域身份和生命周期投影，不再存储组件或提供通用组件 API。生产项目入口已从 `Terraria.EntityEcs` 改为 `Terraria.ArchEntities`。行为测试按当前用户要求未运行。
