# L1 基本玩法删减实施记录

日期：2026-10-08  
分支：`codex/l1-basic-runtime`  
范围：`src/NSSLC/Component`

本次实现把运行目标收敛到 L0 基础运行和 L1 基本玩法：实体身份、世界加载、地图存储、玩家移动与生命、简单 NPC、基础战斗、物品闭环和基础射弹生命周期。世界时钟只推进 tick、昼夜和月相；天气、入侵、特殊事件和复杂世界生成不再由模拟时钟推进。

## 已删除

当前工作区统计如下：

| 项目 | 数量 |
| --- | ---: |
| 删除的生产组件源码 | 287 |
| 删除的生产系统源码 | 75 |
| 删除的生产源码总数 | 446 |
| 删除的专项验证源码 | 15 |
| 删除后剩余组件源码 | 208 |
| 删除后剩余系统源码 | 136 |

主要删除范围：

- 玩家装备效果、Buff 资源、状态效果、护盾、专用移动、宠物和表现投影。
- 世界事件、天气推进、入侵、Boss 进度、复杂世界生成阶段、特殊生物群系、液体工作队列和 Pylon 注册。
- 城镇住房、救援、宠物、门交互、传送门和传送过渡。
- 电路传播、压力板、装饰性 TileEntity 和未接入的方块实体能力。
- 专用射弹能力、拖尾缓存、潮湿状态、反射、钓鱼和其他扩展射弹路径。
- 拴系实体、死亡复仇、钓鱼、规则覆盖，以及没有 L1 生产消费者的 NPC、物品、物理和空间候选模型。

审核清单中的 L0/L1 组件仍全部存在。由于部分旧状态对象是混合模型，不能通过删除单个文件安全拆除；当前保留的典型混合入口包括 `PlayerNetworkStateComponent`、`WorldTimeWeatherState`、`WorldEventProgressState` 以及基础网络和持久化投影。它们的 L1 字段仍用于连接、生命、移动、物品和世界加载，扩展字段不再由 L1 模拟时钟或基础 tick 使用。

## 验证

以下生产项目均使用 `dotnet build --no-restore --nologo -v:q` 构建成功，0 错误；其中 WorldGeneration 在本次最终构建中也为 0 警告：

- Player、Combat、StatusEffects、Projectile
- WorldStorage、WorldSession、WorldInteraction、Town、Teleportation
- `NSSLC.Application`
- `NSSLC.Tools.WorldGeneration`
- `NSSLC.Tools.Simulation`
- `NSSLC.Tools.NetworkServer`

以下 L1 验证入口通过：

- 世界 tick、昼夜边界和跨边界快进。
- 射弹基础 disposition、penetration、network、hydration、lifecycle、network apply 和 tick coordinator。
- NPC 基础伤害、生命、追踪和死亡 smoke test。
- 玩家输入控制和生命周期/休息核心规则。
- 物品 wire/domain 闭环。
- 空间模拟核心查询。

`Terraria.EntityOrganization.Verification` 可以编译，但运行时缺少 `Arch, Version=2.0.0.0` 程序集，因此未将它计入通过项；该失败来自验证环境依赖，不是本次删减产生的编译错误。

## 仍需单独审核的混合路径

审核 JSON 中仍存在 L2-L4 标记但有生产消费者的组件，主要集中在旧的玩家网络兼容字段、NPC 表现/生命周期兼容字段、物品存储兼容字段和世界文件兼容字段。这些类型没有被整块删除，以避免切断基础存档恢复、玩家连接和简单战斗路径；后续若要把组件文件数量进一步压到仅 L0/L1，需要先拆分这些混合组件，再删除扩展字段和对应网络协议映射。当前审核清单中仍有 48 个 L2、73 个 L3 和 19 个 L4 文件存在，但它们都还有生产消费者，不属于可以直接删除的孤立候选。

外部参考目录 `src/NSSLC.Infrastructure/分类参考/` 未修改。
