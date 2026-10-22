# CR-2026-10-08：删除剩余自造 ECS 框架

状态：completed（源码与受影响生产项目编译闭包已完成；运行验收按用户要求未执行）
提出人：用户
渠道：当前会话消息
提出时间：2026-10-08（Asia/Shanghai）
建议分级：major

## 需求原文

> 检查是否存在剩下的自己造的ECS框架把,把ECS框架也删除

## 当前结果

静态审计确认自造 ECS 的通用组件访问层已经删除：

- `EntityLifecycleRegistry` 只保留领域句柄分配、发布、终止、销毁和 Arch World 所有权；生命周期状态已进入 Arch 的 `EntityLifecycleComponent`，注册表不再维护实体记录表；
- `TryEdit`、`TryInspect`、`TryCapture`、`TryCaptureVersioned`、`TryAttach`、`TryDetach`、
  `TryReplace`、`Match`、`Has` 等通用组件 API 已从生产源码移除；
- `ComponentRevisionAccess.cs`、`ComponentRevisionValidation.cs` 和版本快照类型已删除；
- Player、NPC、Item、WorldItem、TileEntity、网络 owner、Projectile 相关迁移路径均直接使用
  Arch `World/Entity` 的 `Get/Set/Add/Remove/Has`；
- `EntityIdentityRegistry` 仅提供领域句柄到 live Arch Entity 的身份解析，不承担组件存储或查询；
- 共享 `ArchComponentDelegates` 已删除，剩余局部回调类型归属于具体业务 owner；
- 生产源码中旧的 `EntityRuntime`、`EntityEcs`、`ComponentStore` 和旧 phase 入口保持零命中。

`RuntimeEntityHandle` 仍作为领域身份/协议投影保留；它不再提供组件存储能力，也不构成另一套
ECS。测试目录中仍有面向已删除通用 API 的历史验证源码，但按用户要求未运行测试；这些文件
不属于当前生产编译闭包。

## 验收口径

只执行静态扫描和受影响生产项目增量编译，不运行任何测试或运行时验证。生产项目编译结果为：

- `Terraria.ArchEntities`：0 warning / 0 error；
- `Terraria.WorldStorage`：6 个既有字段 warning / 0 error；
- `NSSLC.Application`：0 / 0；
- `NSSLC.Tools.Simulation`：0 / 0；
- `NSSLC.Tools.NetworkServer`：0 / 0；
- `Terraria.Projectile`：0 / 0；
- `Terraria.SpatialSimulation`：0 / 0。
